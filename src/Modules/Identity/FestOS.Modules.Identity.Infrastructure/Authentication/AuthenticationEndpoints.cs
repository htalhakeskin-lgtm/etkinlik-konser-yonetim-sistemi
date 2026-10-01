using System.Security.Claims;
using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.Modules.Identity.Application.Authentication;
using FestOS.Modules.Identity.Application.Passwords;
using FestOS.Modules.Identity.Domain;
using FestOS.Modules.Identity.Domain.Users;
using FestOS.Modules.Identity.Infrastructure.Sessions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace FestOS.Modules.Identity.Infrastructure.Authentication;

/// <summary>Signing in and out, and the signed-in user (identity §7).</summary>
internal static class AuthenticationEndpoints
{
    /// <summary>The code of a wrong email or password; the answer does not tell which (US-SYS-010).</summary>
    public const string InvalidCredentials = "invalidCredentials";

    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapPost("/auth/login", LoginAsync)
            .AllowAnonymous()
            .WithoutIdempotencyKey()
            .WithName("Login")
            .WithSummary("Signs in with an email and password.");
        endpoints
            .MapPost("/auth/logout", LogoutAsync)
            .RequireAuthorization()
            .WithName("Logout")
            .WithSummary("Signs out.");
        endpoints
            .MapGet("/me", GetMeAsync)
            .RequireAuthorization()
            .WithName("GetMe")
            .WithSummary("Gets the signed-in user.");
        endpoints
            .MapPost("/me/password", ChangeMyPasswordAsync)
            .RequireAuthorization()
            .WithName("ChangeMyPassword")
            .WithSummary("Sets a new password for the signed-in user; the user's other sessions end.");
    }

    private static async Task<Ok<SignedInUserDetails>> LoginAsync(
        LoginRequest request,
        ICommandHandler<SignInCommand, SignInResult> signIn,
        LoginRateLimiter rateLimiter,
        HttpContext context,
        CancellationToken cancellationToken
    )
    {
        string email = EmailAddress.Normalize(request.Email ?? string.Empty);
        if (!rateLimiter.TryAcquire(context.Connection.RemoteIpAddress, email, out TimeSpan retryAfter))
        {
            throw new RateLimitedException(retryAfter);
        }

        SignInResult result = await signIn.HandleAsync(new(request.Email!, request.Password), cancellationToken);
        if (result.User is not { } user)
        {
            throw result.LockedUntil is { } lockedUntil
                ? new AuthenticationFailedException(
                    IdentityRuleCodes.AccountLocked,
                    "The account is locked for a while after too many wrong passwords.",
                    new Dictionary<string, object?>(StringComparer.Ordinal) { ["lockedUntil"] = lockedUntil }
                )
                : new AuthenticationFailedException(InvalidCredentials, "The email or the password is wrong.");
        }

        ClaimsPrincipal principal = IdentityClaims.PrincipalFor(user);
        await context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true }
        );

        // The antiforgery token is bound to the user, so the one issued before signing in no longer fits.
        context.User = principal;
        context.IssueAntiforgeryToken();
        return TypedResults.Ok(user);
    }

    private static async Task<NoContent> LogoutAsync(HttpContext context, CancellationToken cancellationToken)
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<SignedInUserDetails>> ChangeMyPasswordAsync(
        ChangeMyPasswordRequest request,
        ICommandHandler<ChangeMyPasswordCommand, SignedInUserDetails> changePassword,
        CancellationToken cancellationToken
    ) =>
        TypedResults.Ok(
            await changePassword.HandleAsync(new(request.CurrentPassword, request.NewPassword), cancellationToken)
        );

    private static async Task<Ok<SignedInUserDetails>> GetMeAsync(
        IQueryHandler<GetSignedInUserQuery, SignedInUserDetails> getUser,
        HttpContext context,
        CancellationToken cancellationToken
    )
    {
        SignedInUserDetails user = await getUser.HandleAsync(new GetSignedInUserQuery(), cancellationToken);
        context.IssueAntiforgeryToken();
        return TypedResults.Ok(user);
    }
}
