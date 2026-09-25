using System.Reflection;

namespace FestOS.BuildingBlocks.Domain;

/// <summary>
/// Gives tests and hosting code a stable handle to the FestOS.BuildingBlocks.Domain assembly, which contains
/// entity and aggregate base types, domain events, value types shared by all modules.
/// </summary>
public static class AssemblyReference
{
    /// <summary>The FestOS.BuildingBlocks.Domain assembly.</summary>
    public static Assembly Assembly { get; } = typeof(AssemblyReference).Assembly;
}
