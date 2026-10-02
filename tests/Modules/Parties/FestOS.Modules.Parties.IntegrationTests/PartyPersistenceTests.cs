using FestOS.BuildingBlocks.Infrastructure.Auditing;
using FestOS.Modules.Audit.Infrastructure;
using FestOS.Modules.Parties.Domain.Parties;
using FestOS.Modules.Parties.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.Modules.Parties.IntegrationTests;

public sealed class PartyPersistenceTests(PartiesFixture fixture) : IAsyncLifetime
{
    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync() => await fixture.ResetAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Party_IsSavedWithItsRolesAndContactPoints_AndFoundByThem()
    {
        var party = Party.CreateOrganization(
            "Işık Ses",
            "Işık Ses Sistemleri A.Ş.",
            [PartyRole.Supplier, PartyRole.VenueOperator],
            [new ContactPointDetails(null, ContactPointKind.Phone, "0212 111 22 33", "Santral", false)]
        );
        await SaveAsync(party);

        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        PartiesDbContext context = scope.ServiceProvider.GetRequiredService<PartiesDbContext>();
        Party saved = await context
            .Parties.Include(found => found.ContactPoints)
            .SingleAsync(found => found.Roles.Contains(PartyRole.VenueOperator), Cancellation);
        saved.LegalName.ShouldBe("Işık Ses Sistemleri A.Ş.");
        saved.ContactPoints.ShouldHaveSingleItem().IsPrimary.ShouldBeTrue();
        (
            await context.Parties.CountAsync(found => EF.Functions.ILike(found.Search, "%2121112233%"), Cancellation)
        ).ShouldBe(1);
    }

    [Fact]
    public async Task ContactPointChanges_AreRecordedUnderTheParty()
    {
        var party = Party.CreateOrganization("Işık Ses", null, [PartyRole.Supplier], []);
        await SaveAsync(party);

        await using (AsyncServiceScope scope = fixture.Services.CreateAsyncScope())
        {
            PartiesDbContext context = scope.ServiceProvider.GetRequiredService<PartiesDbContext>();
            Party loaded = await context
                .Parties.Include(found => found.ContactPoints)
                .SingleAsync(found => found.Id == party.Id, Cancellation);
            loaded.Edit(
                "Işık Ses",
                null,
                null,
                null,
                [PartyRole.Supplier],
                [new ContactPointDetails(null, ContactPointKind.Email, "info@isikses.example", null, false)]
            );
            await context.SaveChangesAsync(Cancellation);
        }

        await using AsyncServiceScope readScope = fixture.Services.CreateAsyncScope();
        AuditEntry contactPointEntry = await readScope
            .ServiceProvider.GetRequiredService<AuditDbContext>()
            .Set<AuditEntry>()
            .SingleAsync(entry => entry.EntityType == nameof(ContactPoint), Cancellation);
        contactPointEntry.RootType.ShouldBe(nameof(Party));
        contactPointEntry.RootId.ShouldBe(party.Id.Value);
    }

    private async Task SaveAsync(Party party)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        PartiesDbContext context = scope.ServiceProvider.GetRequiredService<PartiesDbContext>();
        context.Parties.Add(party);
        await context.SaveChangesAsync(Cancellation);
    }
}
