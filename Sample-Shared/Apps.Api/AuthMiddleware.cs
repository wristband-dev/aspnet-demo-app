using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

using Wristband;

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
        if (endpoint?.Metadata.GetMetadata<RequireWristbandAuthAttribute>() == null)
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

        var isAutheticated = SessionHandler.GetIsAuthenticated(context);
        if (!isAutheticated)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized access: user not authenticated");
            return;
        }

        try
        {
            var refreshToken = SessionHandler.GetRefreshToken(context);
            var expiresAt = SessionHandler.GetExpiresAt(context);

            /* WRISTBAND_TOUCHPOINT - AUTHENTICATION */
            var tokenData = await wristbandAuth.RefreshTokenIfExpired(refreshToken, expiresAt);

            if (tokenData != null) {
                await SessionHandler.UpdateTokenClaims(context, tokenData);
            }

            await _next(context);
        }
        catch (Exception error)
        {
            await Console.Error.WriteLineAsync($"Failed to refresh token due to: {error}");
            context.Response.StatusCode = 401;
        }
    }
}
