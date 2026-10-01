using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Application.Users;

/// <summary>One user, for the edit dialog.</summary>
public sealed record GetUserQuery(UserId Id) : IQuery<UserDetails>;
