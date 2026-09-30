namespace FestOS.BuildingBlocks.Application.Messaging;

/// <summary>Reads the module a type belongs to from its namespace, <c>FestOS.Modules.{Module}.…</c> (naming §4.1).</summary>
public static class ModuleNames
{
    /// <summary>The module name, e.g. <c>Booking</c>; <see langword="null"/> for types outside a module.</summary>
    public static string? Of(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return type.Namespace?.Split('.') is ["FestOS", "Modules", var module, ..] ? module : null;
    }
}
