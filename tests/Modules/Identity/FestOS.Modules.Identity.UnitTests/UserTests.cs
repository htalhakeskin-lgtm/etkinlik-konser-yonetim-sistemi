using FestOS.BuildingBlocks.Domain.Rules;
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
    [Trait("Rule", "BR-SYS-014")]
    public void AWarehouseManager_NeedsAWarehouse_WhenCreatedAndWhenEdited()
    {
        var user = User.Create("Ali Bal", "ali@example.com", [Role.BookingManager], [], "hash");

        Should
            .Throw<BusinessRuleViolationException>(() =>
                User.Create("Ali Bal", "ali@example.com", [Role.WarehouseManager], [], "hash")
            )
            .RuleCode.ShouldBe("BR-SYS-014");
        Should
            .Throw<BusinessRuleViolationException>(() =>
                user.Edit("Ali Bal", "ali@example.com", [Role.WarehouseManager], [])
            )
            .RuleCode.ShouldBe("BR-SYS-014");
    }

    [Fact]
    public void Warehouses_BelongToTheWarehouseManagerRole()
    {
        var warehouse = Guid.CreateVersion7();
        var user = User.Create("Ali Bal", "ali@example.com", [Role.WarehouseManager], [warehouse], "hash");

        user.Edit("Ali Bal", "ali@example.com", [Role.BookingManager], [warehouse]);

        user.WarehouseIds.ShouldBeEmpty();
        user.NeedsWarehouse.ShouldBeFalse();
    }

    [Fact]
    public void RemovingTheLastWarehouse_LeavesTheManagerActive_WaitingForANewOne()
    {
        var first = Guid.CreateVersion7();
        var second = Guid.CreateVersion7();
        var user = User.Create("Ali Bal", "ali@example.com", [Role.WarehouseManager], [first, second], "hash");

        user.RemoveWarehouse(first);
        bool waitsAfterFirst = user.NeedsWarehouse;
        user.RemoveWarehouse(second);

        waitsAfterFirst.ShouldBeFalse();
        user.NeedsWarehouse.ShouldBeTrue();
        user.DeactivatedAt.ShouldBeNull();
    }

    [Fact]
    public void Edit_KeepsTheEmailAndTheSearchKeyInTheirStoredForms()
    {
        var user = User.Create("Ali Bal", "ali@example.com", [Role.BookingManager], [], "hash");

        user.Edit(" Işık Bal ", " Isik@Example.com ", [Role.TechnicalManager, Role.TechnicalManager], []);

        user.FullName.ShouldBe("Işık Bal");
        user.FullNameSearch.ShouldBe("isik bal");
        user.Email.ShouldBe("isik@example.com");
        user.Roles.ShouldBe([Role.TechnicalManager]);
    }

    [Fact]
    [Trait("Rule", "BR-SYS-001")]
    public void Deactivate_KeepsTheUser_AndActivateOpensTheAccessAgain()
    {
        var user = User.Create("Ali Bal", "ali@example.com", [Role.BookingManager], [], "hash");
        var by = Guid.CreateVersion7();
        var at = new DateTimeOffset(2027, 1, 4, 6, 0, 0, TimeSpan.Zero);

        user.Deactivate(by, at);
        user.Deactivate(Guid.CreateVersion7(), at.AddDays(1));
        DateTimeOffset? deactivatedAt = user.DeactivatedAt;
        Guid? deactivatedBy = user.DeactivatedBy;
        user.Activate();

        deactivatedAt.ShouldBe(at);
        deactivatedBy.ShouldBe(by);
        user.DeactivatedAt.ShouldBeNull();
        user.DeactivatedBy.ShouldBeNull();
    }

    [Fact]
    [Trait("Rule", "BR-SYS-006")]
    public void ResetPassword_AsksForANewPasswordAndLiftsTheLock()
    {
        var user = User.Create("Ali Bal", "ali@example.com", [Role.BookingManager], [], "hash");
        var now = new DateTimeOffset(2027, 1, 4, 6, 0, 0, TimeSpan.Zero);
        user.ChangePassword("own hash");
        user.RecordFailedSignIn(now, new LockoutPolicy(MaxFailedAttempts: 1, Duration: TimeSpan.FromMinutes(15)));

        user.ResetPassword("temporary hash");

        user.PasswordHash.ShouldBe("temporary hash");
        user.MustChangePassword.ShouldBeTrue();
        user.IsLockedAt(now).ShouldBeFalse();
        user.FailedLoginCount.ShouldBe(0);
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
