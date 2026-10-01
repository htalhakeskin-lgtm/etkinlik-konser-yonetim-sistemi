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
    [Trait("Rule", "BR-SYS-005")]
    public void WrongPasswords_LockTheAccountAtTheLastAllowedOneAndStartTheCountAgain()
    {
        var user = User.Create("Ayşe Kaya", "ayse@example.com", [Role.BookingManager], [], "hash");
        var now = new DateTimeOffset(2027, 1, 4, 6, 0, 0, TimeSpan.Zero);
        var policy = new LockoutPolicy(MaxFailedAttempts: 3, Duration: TimeSpan.FromMinutes(15));

        user.RecordFailedSignIn(now, policy);
        user.RecordFailedSignIn(now, policy);
        bool lockedBeforeTheLast = user.IsLockedAt(now);
        user.RecordFailedSignIn(now, policy);

        lockedBeforeTheLast.ShouldBeFalse();
        user.LockedUntil.ShouldBe(now.AddMinutes(15));
        user.FailedLoginCount.ShouldBe(0);
        user.IsLockedAt(now.AddMinutes(15).AddTicks(-1)).ShouldBeTrue();
        user.IsLockedAt(now.AddMinutes(15)).ShouldBeFalse();
    }

    [Fact]
    [Trait("Rule", "BR-SYS-005")]
    public void SuccessfulSignIn_ForgetsTheEarlierWrongPasswords()
    {
        var user = User.Create("Ayşe Kaya", "ayse@example.com", [Role.BookingManager], [], "hash");
        var now = new DateTimeOffset(2027, 1, 4, 6, 0, 0, TimeSpan.Zero);
        var policy = new LockoutPolicy(MaxFailedAttempts: 2, Duration: TimeSpan.FromMinutes(15));

        user.RecordFailedSignIn(now, policy);
        user.RecordSuccessfulSignIn();
        user.RecordFailedSignIn(now, policy);

        user.IsLockedAt(now).ShouldBeFalse();
        user.FailedLoginCount.ShouldBe(1);
    }

    [Fact]
    [Trait("Rule", "BR-SYS-006")]
    public void ChangePassword_EndsTheTemporaryPassword()
    {
        var user = User.Create("Ayşe Kaya", "ayse@example.com", [Role.BookingManager], [], "temporary hash");

        user.ChangePassword("new hash");

        user.PasswordHash.ShouldBe("new hash");
        user.MustChangePassword.ShouldBeFalse();
    }

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
