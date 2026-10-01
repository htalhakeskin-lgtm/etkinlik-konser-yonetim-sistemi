using System.Security.Claims;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.Modules.Identity.Application.Authentication;
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
    }

    private static async Task<Ok<SignedInUserDetails>> LoginAsync(
        LoginRequest request,
        ICommandHandler<SignInCommand, SignedInUserDetails> signIn,
        HttpContext context,
        CancellationToken cancellationToken
    )
    {
        SignedInUserDetails user = await signIn.HandleAsync(new(request.Email, request.Password), cancellationToken);
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
