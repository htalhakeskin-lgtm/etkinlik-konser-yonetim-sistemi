using System.Net;

namespace FestOS.Testing;

/// <summary>
/// Sends test requests the way the front end's request wrapper does (testing §6): keeps the cookies, and
/// adds the antiforgery token and a new <c>Idempotency-Key</c> to requests that change data. A header the
/// test set itself is left alone, so tests of the missing or wrong headers set them explicitly.
/// </summary>
public sealed class BrowserLikeHandler : DelegatingHandler
{
    private const string TokenPath = "/api/v1/antiforgery";
    private const string TokenCookieSuffix = "festos_xsrf";

    private readonly CookieContainer _cookies = new();

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(request);
        Uri address = request.RequestUri ?? throw new InvalidOperationException("The request has no address.");

        if (ChangesData(request.Method))
        {
            if (!request.Headers.Contains("X-XSRF-TOKEN"))
            {
                request.Headers.Add("X-XSRF-TOKEN", await RequestTokenAsync(address, cancellationToken));
            }

            if (!request.Headers.Contains("Idempotency-Key"))
            {
                request.Headers.Add("Idempotency-Key", Guid.CreateVersion7().ToString());
            }
        }

        return await SendWithCookiesAsync(request, cancellationToken);
    }

    private static bool ChangesData(HttpMethod method) =>
        method == HttpMethod.Post
        || method == HttpMethod.Put
        || method == HttpMethod.Patch
        || method == HttpMethod.Delete;

    private async Task<string> RequestTokenAsync(Uri address, CancellationToken cancellationToken)
    {
        Cookie? token = Find(address);
        if (token is null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(address, TokenPath));
            using HttpResponseMessage response = await SendWithCookiesAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            token = Find(address) ?? throw new InvalidOperationException($"GET {TokenPath} issued no token.");
        }

        return token.Value;
    }

    private Cookie? Find(Uri address) =>
        _cookies
            .GetCookies(address)
            .FirstOrDefault(cookie => cookie.Name.EndsWith(TokenCookieSuffix, StringComparison.Ordinal));

    private async Task<HttpResponseMessage> SendWithCookiesAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        string cookies = _cookies.GetCookieHeader(request.RequestUri!);
        if (cookies.Length > 0 && !request.Headers.Contains("Cookie"))
        {
            request.Headers.Add("Cookie", cookies);
        }

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
        if (response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? setCookies))
        {
            foreach (string setCookie in setCookies)
            {
                _cookies.SetCookies(request.RequestUri!, setCookie);
            }
        }

        return response;
    }
}
