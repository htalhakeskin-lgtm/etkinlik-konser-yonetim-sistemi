using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Domain.Rules;
using FestOS.Modules.Identity.Application.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.Modules.Identity.Infrastructure.Users;

/// <summary>
/// The Host's <c>create-admin --email … --name …</c> command (identity ID-07): makes the first system
/// administrator and writes the temporary password once, to the console only, never to the logs.
/// </summary>
public static class CreateAdminCommandLine
{
    /// <summary>Runs the command; returns the process exit code.</summary>
    public static async Task<int> RunAsync(IServiceProvider services, IConfiguration arguments, TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(output);

        await using AsyncServiceScope scope = services.CreateAsyncScope();
        try
        {
            string? temporaryPassword = await scope
                .ServiceProvider.GetRequiredService<ICommandHandler<CreateFirstAdministratorCommand, string?>>()
                .HandleAsync(
                    new CreateFirstAdministratorCommand(arguments["name"] ?? "", arguments["email"] ?? ""),
                    CancellationToken.None
                );
            if (temporaryPassword is null)
            {
                await output.WriteLineAsync("An active system administrator already exists; nothing was created.");
                return 1;
            }

            await output.WriteLineAsync($"Created. Temporary password (shown once): {temporaryPassword}");
            return 0;
        }
        catch (ValidationFailedException exception)
        {
            await output.WriteLineAsync("Usage: create-admin --email <email> --name <full name>");
            foreach (ValidationError error in exception.Errors)
            {
                await output.WriteLineAsync($"  {error.PropertyPath}: {error.Code}");
            }

            return 2;
        }
        catch (BusinessRuleViolationException exception)
        {
            await output.WriteLineAsync($"Refused ({exception.RuleCode}): {exception.Message}");
            return 1;
        }
    }
}
