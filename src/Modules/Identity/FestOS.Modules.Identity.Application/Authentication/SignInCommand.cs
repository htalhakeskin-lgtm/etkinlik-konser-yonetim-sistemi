using FestOS.BuildingBlocks.Application.Messaging;

namespace FestOS.Modules.Identity.Application.Authentication;

/// <summary>Checks an email and password (US-SYS-010); the endpoint then opens the session.</summary>
public sealed record SignInCommand(string Email, [property: Sensitive] string Password) : ICommand<SignInResult>;
