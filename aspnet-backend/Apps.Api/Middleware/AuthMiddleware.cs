using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

using Wristband.AspNet.Auth;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;

    public AuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IWristbandAuthService wristbandAuth)
    {
        // Skip the middleware for unprotected endpoints
        var endpoint = context.GetEndpoint();
        /* WRISTBAND_TOUCHPOINT - AUTHENTICATION */
        if (endpoint?.Metadata.GetMetadata<RequireWristbandAuth>() == null)
        {
            await _next(context);
            return;
        }

        // Check if the authenticated session is present
        var authResult = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!authResult.Succeeded || authResult.Principal == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized access: authentication failed");
            return;
        }

        try
        {
            var refreshToken = SessionUtils.GetStringSessionClaim(context, "refreshToken");
            var expiresAt = SessionUtils.GetExpiresAt(context);

            /* WRISTBAND_TOUCHPOINT - AUTHENTICATION */
            var tokenData = await wristbandAuth.RefreshTokenIfExpired(refreshToken, expiresAt);
            await SessionUtils.UpdateTokenClaims(context, tokenData);
            await _next(context);
        }
        catch (Exception error)
        {
            await Console.Error.WriteLineAsync($"Failed to refresh token due to: {error}");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
    }
}
