using FestOS.BuildingBlocks.Application.Messaging;

namespace FestOS.Modules.Identity.Application.Authentication;

/// <summary>The details of the user of the current session.</summary>
public sealed record GetSignedInUserQuery : IQuery<SignedInUserDetails>;
