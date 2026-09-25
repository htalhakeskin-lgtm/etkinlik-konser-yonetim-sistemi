using System.Reflection;

namespace FestOS.BuildingBlocks.Application;

/// <summary>
/// Gives tests and hosting code a stable handle to the FestOS.BuildingBlocks.Application assembly, which contains
/// command and query abstractions, decorators and application services shared by all modules.
/// </summary>
public static class AssemblyReference
{
    /// <summary>The FestOS.BuildingBlocks.Application assembly.</summary>
    public static Assembly Assembly { get; } = typeof(AssemblyReference).Assembly;
}
