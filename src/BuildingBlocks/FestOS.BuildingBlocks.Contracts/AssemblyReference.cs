using System.Reflection;

namespace FestOS.BuildingBlocks.Contracts;

/// <summary>
/// Gives tests and hosting code a stable handle to the FestOS.BuildingBlocks.Contracts assembly, which contains
/// integration event base types that every module may reference.
/// </summary>
public static class AssemblyReference
{
    /// <summary>The FestOS.BuildingBlocks.Contracts assembly.</summary>
    public static Assembly Assembly { get; } = typeof(AssemblyReference).Assembly;
}
