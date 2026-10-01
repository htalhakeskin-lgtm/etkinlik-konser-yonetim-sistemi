using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.UnitTests;

public sealed class UserTests
{
    [Fact]
    [Trait("Rule", "BR-SYS-015")]
    public void Create_StoresTheEmailInItsComparedForm() =>
        User.Create("Ayşe Kaya", "  Ayse.Kaya@Example.COM ", [Role.BookingManager], [], "hash")
            .Email.ShouldBe("ayse.kaya@example.com");

    [Fact]
    [Trait("Rule", "BR-SYS-006")]
    public void Create_MakesTheUserSetANewPassword()
    {
        var warehouse = Guid.CreateVersion7();

        var user = User.Create(
            " Ayşe Kaya ",
            "ayse@example.com",
            [Role.WarehouseManager, Role.BookingManager, Role.WarehouseManager],
            [warehouse, warehouse],
            "hash"
        );

        user.FullName.ShouldBe("Ayşe Kaya");
        user.MustChangePassword.ShouldBeTrue();
        user.Roles.ShouldBe([Role.BookingManager, Role.WarehouseManager]);
        user.WarehouseIds.ShouldBe([warehouse]);
        ((FestOS.BuildingBlocks.Domain.Entities.IDeactivatable)user).IsActive.ShouldBeTrue();
    }
}
