using FestOS.BuildingBlocks.Domain.Identifiers;
using FestOS.BuildingBlocks.Domain.Monetary;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace FestOS.BuildingBlocks.Infrastructure.OpenApi;

/// <summary>
/// Describes the API's own value formats (api §5.2, §14.1), which .NET cannot see behind the custom JSON
/// converters: text, decimals and currencies as text, strongly typed identifiers as UUIDs, durations as
/// whole minutes, and enums as text.
/// </summary>
internal sealed class ApiSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(context);

        Type type = context.JsonTypeInfo.Type;
        Type? underlying = Nullable.GetUnderlyingType(type);
        bool nullable = underlying is not null;
        type = underlying ?? type;

        if (type == typeof(string))
        {
            // Text properties have the trimming converter, which hides their type from the generator.
            schema.Type = Maybe(JsonSchemaType.String, context.JsonPropertyInfo?.IsGetNullable == true);
        }
        else if (type == typeof(decimal))
        {
            Describe(schema, Maybe(JsonSchemaType.String, nullable), "decimal");
        }
        else if (type == typeof(TimeSpan))
        {
            Describe(schema, Maybe(JsonSchemaType.Integer, nullable), "int32");
            schema.Description ??= "Whole minutes.";
        }
        else if (type == typeof(Currency))
        {
            Describe(schema, Maybe(JsonSchemaType.String, nullable), format: null);
            schema.Pattern = "^[A-Z]{3}$";
        }
        else if (IsStronglyTypedId(type))
        {
            Describe(schema, Maybe(JsonSchemaType.String, nullable), "uuid");
        }
        else if (type.IsEnum)
        {
            // .NET may leave the type out of text enums (dotnet/aspnetcore#62022).
            schema.Type = Maybe(JsonSchemaType.String, nullable);
        }

        return Task.CompletedTask;
    }

    private static JsonSchemaType Maybe(JsonSchemaType type, bool nullable) =>
        nullable ? type | JsonSchemaType.Null : type;

    private static void Describe(OpenApiSchema schema, JsonSchemaType type, string? format)
    {
        schema.Type = type;
        schema.Format = format;
        schema.Pattern = null;
        schema.Properties?.Clear();
        schema.Required?.Clear();
    }

    private static bool IsStronglyTypedId(Type type) =>
        type.IsValueType
        && type.GetInterfaces()
            .Any(contract =>
                contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IStronglyTypedId<>)
            );
}
