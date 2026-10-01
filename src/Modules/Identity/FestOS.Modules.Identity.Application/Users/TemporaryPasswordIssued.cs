using FestOS.BuildingBlocks.Application.Messaging;

namespace FestOS.Modules.Identity.Application.Users;

/// <summary>The temporary password to hand over; shown once and never stored (BR-SYS-006).</summary>
public sealed record TemporaryPasswordIssued([property: Sensitive] string TemporaryPassword);
