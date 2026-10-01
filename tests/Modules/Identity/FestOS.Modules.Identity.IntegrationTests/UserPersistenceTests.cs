using FestOS.BuildingBlocks.Domain.Rules;
using FestOS.Modules.Identity.Domain.Users;
using FestOS.Modules.Identity.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.Modules.Identity.IntegrationTests;

public sealed class UserPersistenceTests(IdentityFixture fixture) : IAsyncLifetime
{
    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync() => await fixture.ResetAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task User_IsStoredWithItsRolesAndWarehouses()
    {
        var warehouse = Guid.CreateVersion7();
        var user = User.Create("Ayşe Kaya", "ayse@example.com", [Role.WarehouseManager], [warehouse], "hash");
        await SaveAsync(user);

        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        User stored = await scope
            .ServiceProvider.GetRequiredService<IdentityDbContext>()
            .Users.SingleAsync(found => found.Id == user.Id, Cancellation);
        stored.Roles.ShouldBe([Role.WarehouseManager]);
        stored.WarehouseIds.ShouldBe([warehouse]);
        stored.Version.ShouldBe(1);
    }

    [Fact]
    [Trait("Rule", "BR-SYS-015")]
    public async Task SecondUserWithTheSameEmail_IsRefusedWhateverTheCase()
    {
        await SaveAsync(User.Create("Ayşe Kaya", "ayse@example.com", [Role.BookingManager], [], "hash"));

        BusinessRuleViolationException refused = await Should.ThrowAsync<BusinessRuleViolationException>(() =>
            SaveAsync(User.Create("Ayşe K.", "AYSE@Example.com", [Role.TechnicalManager], [], "hash"))
        );

        refused.RuleCode.ShouldBe("BR-SYS-015");
    }

    private async Task SaveAsync(User user)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        IdentityDbContext context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        context.Users.Add(user);
        await context.SaveChangesAsync(Cancellation);
    }
}
