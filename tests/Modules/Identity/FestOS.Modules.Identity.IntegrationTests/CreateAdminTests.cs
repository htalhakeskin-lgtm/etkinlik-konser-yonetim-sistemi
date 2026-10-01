using FestOS.Modules.Identity.Application.Passwords;
using FestOS.Modules.Identity.Domain.Users;
using FestOS.Modules.Identity.Infrastructure;
using FestOS.Modules.Identity.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.Modules.Identity.IntegrationTests;

/// <summary>The Host's create-admin command (identity ID-07).</summary>
public sealed class CreateAdminTests(IdentityFixture fixture) : IAsyncLifetime
{
    public async ValueTask InitializeAsync() => await fixture.ResetAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    [Trait("Rule", "BR-SYS-006")]
    public async Task CreateAdmin_MakesTheFirstAdministratorWithATemporaryPasswordShownOnce()
    {
        (int exitCode, string output) = await RunAsync("Ayşe Kaya", "Ayse@Example.com");

        exitCode.ShouldBe(0);
        string temporaryPassword = output.Trim().Split(' ')[^1];
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        User admin = await scope
            .ServiceProvider.GetRequiredService<IdentityDbContext>()
            .Users.SingleAsync(user => user.Email == "ayse@example.com", TestContext.Current.CancellationToken);
        admin.Roles.ShouldBe([Role.SystemAdministrator]);
        admin.MustChangePassword.ShouldBeTrue();
        scope
            .ServiceProvider.GetRequiredService<IPasswordHasher>()
            .Verify(admin.PasswordHash, temporaryPassword, out _)
            .ShouldBeTrue();
    }

    [Fact]
    public async Task CreateAdmin_WhenAnActiveAdministratorExists_CreatesNothing()
    {
        await RunAsync("Ayşe Kaya", "ayse@example.com");

        (int exitCode, string output) = await RunAsync("Mehmet Demir", "mehmet@example.com");

        exitCode.ShouldBe(1);
        output.ShouldContain("already exists");
    }

    [Fact]
    public async Task CreateAdmin_WithoutTheArguments_ExplainsTheUsage()
    {
        (int exitCode, string output) = await RunAsync(name: null, email: null);

        exitCode.ShouldBe(2);
        output.ShouldContain("Usage: create-admin");
    }

    private async Task<(int ExitCode, string Output)> RunAsync(string? name, string? email)
    {
        IConfiguration arguments = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>(StringComparer.Ordinal) { ["name"] = name, ["email"] = email }
            )
            .Build();
        await using var output = new StringWriter();
        int exitCode = await CreateAdminCommandLine.RunAsync(fixture.Services, arguments, output);
        return (exitCode, output.ToString());
    }
}
