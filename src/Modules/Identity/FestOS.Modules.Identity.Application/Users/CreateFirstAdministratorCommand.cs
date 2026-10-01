using FestOS.BuildingBlocks.Application.Messaging;

namespace FestOS.Modules.Identity.Application.Users;

/// <summary>
/// Creates the first system administrator of a new installation (identity ID-07). The result is the
/// temporary password, shown once, or <see langword="null"/> when an active administrator already exists.
/// </summary>
public sealed record CreateFirstAdministratorCommand(string FullName, string Email) : ICommand<string?>;
