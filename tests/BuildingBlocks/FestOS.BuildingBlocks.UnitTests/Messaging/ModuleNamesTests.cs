using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Sample.Application;

namespace FestOS.BuildingBlocks.UnitTests.Messaging;

public sealed class ModuleNamesTests
{
    [Fact]
    public void Of_ForATypeInAModule_ReturnsTheModuleName() =>
        ModuleNames.Of(typeof(PlaceSampleCommand)).ShouldBe("Sample");

    [Fact]
    public void Of_ForATypeOutsideAModule_ReturnsNull() => ModuleNames.Of(typeof(ModuleNamesTests)).ShouldBeNull();
}
