using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;

namespace FestOS.BuildingBlocks.Infrastructure.OpenApi;

/// <summary>The API's OpenAPI document (api §14.1, building-blocks §10).</summary>
public static class OpenApiExtensions
{
    /// <summary>The document's name; the build writes it to <c>src/web/openapi/festos.json</c>.</summary>
    public const string DocumentName = "v1";

    /// <summary>
    /// Registers the OpenAPI 3.1 document with the project's transformers. It uses the HTTP pipeline's JSON
    /// rules, so the document describes the JSON the API really reads and writes.
    /// </summary>
    public static IHostApplicationBuilder AddApiDocument(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.AddOpenApi(
            DocumentName,
            options =>
            {
                options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
                options.AddSchemaTransformer<ApiSchemaTransformer>();
                options.AddOperationTransformer<ApiOperationTransformer>();
                options.AddDocumentTransformer(
                    (document, _, _) =>
                    {
                        document.Info = new OpenApiInfo { Title = "FestOS API", Version = DocumentName };

                        // The wrappers of query string enums are bound as text; their parameters point to the
                        // enum itself (ApiOperationTransformer), so the wrapper's own schema is never used.
                        foreach (
                            string name in document
                                .Components?.Schemas?.Keys.Where(name =>
                                    name.StartsWith("EnumQueryValueOf", StringComparison.Ordinal)
                                )
                                .ToList()
                                ?? []
                        )
                        {
                            document.Components!.Schemas!.Remove(name);
                        }

                        return Task.CompletedTask;
                    }
                );
            }
        );
        return builder;
    }
}
