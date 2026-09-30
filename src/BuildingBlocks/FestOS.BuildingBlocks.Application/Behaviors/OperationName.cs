using FestOS.BuildingBlocks.Application.Messaging;

namespace FestOS.BuildingBlocks.Application.Behaviors;

/// <summary>
/// The name of a command or query without its suffix (<c>ConfirmEventCommand</c> → <c>ConfirmEvent</c>)
/// and the module it belongs to, read from the namespace <c>FestOS.Modules.{Module}.…</c>.
/// </summary>
internal readonly record struct OperationName(string Name, string? ModuleName)
{
    private static readonly string[] Suffixes = ["Command", "Query"];

    public static OperationName Of<TRequest>() => Cache<TRequest>.Value;

    private static OperationName Create(Type type)
    {
        string name = type.Name;
        string? suffix = Array.Find(
            Suffixes,
            candidate => name.Length > candidate.Length && name.EndsWith(candidate, StringComparison.Ordinal)
        );
        return new OperationName(suffix is null ? name : name[..^suffix.Length], ModuleNames.Of(type));
    }

    private static class Cache<TRequest>
    {
        public static readonly OperationName Value = Create(typeof(TRequest));
    }
}
