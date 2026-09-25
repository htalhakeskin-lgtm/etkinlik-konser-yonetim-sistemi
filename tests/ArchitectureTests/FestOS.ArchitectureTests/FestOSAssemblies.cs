using System.Reflection;

namespace FestOS.ArchitectureTests;

/// <summary>The FestOS assemblies that architecture tests inspect. Modules are added as they are built.</summary>
internal static class FestOSAssemblies
{
    public static Assembly BuildingBlocksDomain { get; } = BuildingBlocks.Domain.AssemblyReference.Assembly;

    public static Assembly BuildingBlocksApplication { get; } = BuildingBlocks.Application.AssemblyReference.Assembly;

    public static Assembly BuildingBlocksInfrastructure { get; } =
        BuildingBlocks.Infrastructure.AssemblyReference.Assembly;

    public static Assembly BuildingBlocksContracts { get; } = BuildingBlocks.Contracts.AssemblyReference.Assembly;

    public static IReadOnlyList<Assembly> All { get; } =
    [BuildingBlocksDomain, BuildingBlocksApplication, BuildingBlocksInfrastructure, BuildingBlocksContracts];
}
