using System.Text.Json.Nodes;
using FestOS.BuildingBlocks.Infrastructure.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace FestOS.BuildingBlocks.Infrastructure.OpenApi;

/// <summary>
/// Adds what every operation shares (api §14.1): the required <c>If-Match</c> and <c>Idempotency-Key</c>
/// headers where the endpoint needs them, and Problem Details as the shape of every error response. The
/// generated client then cannot forget a header, and knows its error type.
/// </summary>
internal sealed class ApiOperationTransformer : IOpenApiOperationTransformer
{
    public const string ProblemSchema = "ApiProblem";

    public const string NoIdempotencyKeyExtension = "x-festos-no-idempotency-key";

    // A query string enum is bound as text, so its schema says only "string". It takes the names of the
    // enum's JSON form (EnumQueryValue<T>), so the parameter points to the enum's own schema and the
    // generated client offers the same type as in the bodies.
    private static async Task DescribeEnumQueryValuesAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        OpenApiDocument document,
        CancellationToken cancellationToken
    )
    {
        foreach (ApiParameterDescription parameter in context.Description.ParameterDescriptions)
        {
            Type? type = parameter.Type is null ? null : Nullable.GetUnderlyingType(parameter.Type) ?? parameter.Type;
            if (
                type is not { IsGenericType: true }
                || type.GetGenericTypeDefinition() != typeof(EnumQueryValue<>)
                || operation
                    .Parameters?.OfType<OpenApiParameter>()
                    .FirstOrDefault(found => string.Equals(found.Name, parameter.Name, StringComparison.Ordinal))
                    is not { } documented
            )
            {
                continue;
            }

            Type enumType = type.GetGenericArguments()[0];
            if (document.Components?.Schemas?.ContainsKey(enumType.Name) != true)
            {
                document.AddComponent(
                    enumType.Name,
                    await context.GetOrCreateSchemaAsync(enumType, cancellationToken: cancellationToken)
                );
            }

            documented.Schema = new OpenApiSchemaReference(enumType.Name, document);
        }
    }

    public async Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);

        IList<object> metadata = context.Description.ActionDescriptor.EndpointMetadata;
        if (metadata.OfType<RequiresVersionMetadata>().Any())
        {
            AddRequiredHeader(
                operation,
                "If-Match",
                "The version the change is based on, as a strong entity tag, e.g. \"7\" (api §9).",
                format: null
            );
        }

        if (
            metadata.OfType<RequiresIdempotencyKeyMetadata>().Any()
            && !metadata.OfType<NoIdempotencyKeyMetadata>().Any()
            && IdempotencyFilter.ChangesData(context.Description.HttpMethod ?? string.Empty)
        )
        {
            AddRequiredHeader(
                operation,
                IdempotencyFilter.KeyHeader,
                "A new UUID for each user action, and the same one when the action is retried (api §10).",
                "uuid"
            );
        }

        // Stated in the document, so AT-15 can tell a deliberate exemption from a forgotten header.
        if (metadata.OfType<NoIdempotencyKeyMetadata>().Any())
        {
            operation.Extensions ??= new Dictionary<string, IOpenApiExtension>(StringComparer.Ordinal);
            operation.Extensions[NoIdempotencyKeyExtension] = new JsonNodeExtension(JsonValue.Create(true));
        }

        OpenApiDocument document =
            context.Document ?? throw new InvalidOperationException("The operation belongs to no document.");
        await DescribeEnumQueryValuesAsync(operation, context, document, cancellationToken);
        if (document.Components?.Schemas?.ContainsKey(ProblemSchema) != true)
        {
            document.AddComponent(
                ProblemSchema,
                await context.GetOrCreateSchemaAsync(typeof(ApiProblem), cancellationToken: cancellationToken)
            );
        }

        operation.Responses ??= [];
        operation.Responses["default"] = new OpenApiResponse
        {
            Description = "An error, as Problem Details (api §8).",
            Content = new Dictionary<string, OpenApiMediaType>(StringComparer.Ordinal)
            {
                ["application/problem+json"] = new() { Schema = new OpenApiSchemaReference(ProblemSchema, document) },
            },
        };
    }

    private static void AddRequiredHeader(OpenApiOperation operation, string name, string description, string? format)
    {
        operation.Parameters ??= [];
        operation.Parameters.Add(
            new OpenApiParameter
            {
                Name = name,
                In = ParameterLocation.Header,
                Required = true,
                Description = description,
                Schema = new OpenApiSchema { Type = JsonSchemaType.String, Format = format },
            }
        );
    }
}
