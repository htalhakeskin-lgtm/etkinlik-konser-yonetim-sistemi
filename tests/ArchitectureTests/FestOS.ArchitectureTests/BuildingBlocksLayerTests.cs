using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnitV3;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace FestOS.ArchitectureTests;

/// <summary>Layer rules for the shared building blocks (docs/08-architecture.md §12.2).</summary>
public sealed class BuildingBlocksLayerTests
{
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies([.. FestOSAssemblies.All])
        .Build();

    private static readonly IObjectProvider<IType> DomainTypes = Types()
        .That()
        .ResideInAssembly(FestOSAssemblies.BuildingBlocksDomain)
        .As("BuildingBlocks.Domain");

    private static readonly IObjectProvider<IType> ApplicationTypes = Types()
        .That()
        .ResideInAssembly(FestOSAssemblies.BuildingBlocksApplication)
        .As("BuildingBlocks.Application");

    private static readonly IObjectProvider<IType> InfrastructureTypes = Types()
        .That()
        .ResideInAssembly(FestOSAssemblies.BuildingBlocksInfrastructure)
        .As("BuildingBlocks.Infrastructure");

    private static readonly IObjectProvider<IType> ContractsTypes = Types()
        .That()
        .ResideInAssembly(FestOSAssemblies.BuildingBlocksContracts)
        .As("BuildingBlocks.Contracts");

    [Fact]
    [Trait("ArchitectureRule", "AT-01")]
    public void Domain_Always_DoesNotDependOnOtherLayersOrInfrastructurePackages()
    {
        IArchRule rule = Types()
            .That()
            .Are(DomainTypes)
            .Should()
            .NotDependOnAny(ApplicationTypes)
            .AndShould()
            .NotDependOnAny(InfrastructureTypes)
            .AndShould()
            .NotDependOnAny(ContractsTypes)
            .AndShould()
            .NotDependOnAny(
                Types().That().ResideInNamespaceMatching(@"^Microsoft\.(EntityFrameworkCore|AspNetCore)(\..*)?$")
            );

        rule.Check(Architecture);
    }

    [Fact]
    [Trait("ArchitectureRule", "AT-03")]
    public void Contracts_Always_DoesNotDependOnOtherFestOSAssemblies()
    {
        IArchRule rule = Types()
            .That()
            .Are(ContractsTypes)
            .Should()
            .NotDependOnAny(DomainTypes)
            .AndShould()
            .NotDependOnAny(ApplicationTypes)
            .AndShould()
            .NotDependOnAny(InfrastructureTypes);

        rule.Check(Architecture);
    }

    [Fact]
    [Trait("ArchitectureRule", "AT-04")]
    public void Application_Always_DoesNotDependOnInfrastructure()
    {
        IArchRule rule = Types().That().Are(ApplicationTypes).Should().NotDependOnAny(InfrastructureTypes);

        rule.Check(Architecture);
    }
}
