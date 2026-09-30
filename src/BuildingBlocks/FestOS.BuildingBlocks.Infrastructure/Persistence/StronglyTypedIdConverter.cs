using System.Reflection;
using FestOS.BuildingBlocks.Domain.Identifiers;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FestOS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>Stores a strongly typed identifier as its UUID (database §5.2).</summary>
internal sealed class StronglyTypedIdConverter<TId>()
    : ValueConverter<TId, Guid>(id => id.Value, value => Create(value))
    where TId : struct, IStronglyTypedId<TId>
{
    // Expression trees cannot call a static abstract member directly.
    private static TId Create(Guid value) => TId.From(value);
}

/// <summary>Finds the strongly typed identifiers a module's model may use.</summary>
internal static class StronglyTypedIdConverter
{
    /// <summary>
    /// The identifier types in the context's assembly and the FestOS module assemblies it references,
    /// such as the module's Domain project.
    /// </summary>
    public static IEnumerable<Type> FindIdTypes(Assembly contextAssembly)
    {
        IEnumerable<Assembly> assemblies =
        [
            contextAssembly,
            .. contextAssembly
                .GetReferencedAssemblies()
                .Where(name =>
                    name.Name is { } assemblyName
                    && assemblyName.StartsWith("FestOS.", StringComparison.Ordinal)
                    && !assemblyName.StartsWith("FestOS.BuildingBlocks", StringComparison.Ordinal)
                )
                .Select(Assembly.Load),
        ];

        return assemblies.SelectMany(assembly => assembly.GetTypes()).Where(IsStronglyTypedId);
    }

    public static Type ConverterTypeFor(Type idType) => typeof(StronglyTypedIdConverter<>).MakeGenericType(idType);

    private static bool IsStronglyTypedId(Type type) =>
        type.IsValueType
        && type.GetInterfaces()
            .Any(contract =>
                contract.IsGenericType
                && contract.GetGenericTypeDefinition() == typeof(IStronglyTypedId<>)
                && contract.GetGenericArguments()[0] == type
            );
}
