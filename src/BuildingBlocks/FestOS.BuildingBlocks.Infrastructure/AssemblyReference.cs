using System.Reflection;

namespace FestOS.BuildingBlocks.Infrastructure;

/// <summary>
/// Gives tests and hosting code a stable handle to the FestOS.BuildingBlocks.Infrastructure assembly, which contains
/// persistence, messaging and hosting building blocks shared by all modules.
/// </summary>
public static class AssemblyReference
{
    /// <summary>The FestOS.BuildingBlocks.Infrastructure assembly.</summary>
    public static Assembly Assembly { get; } = typeof(AssemblyReference).Assembly;
}
