using Microsoft.AspNetCore.Http;

using Wristband.AspNet.Auth;

public class CsrfMiddleware
{
    private readonly RequestDelegate _next;

    public CsrfMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip the middleware for unprotected endpoints
        var endpoint = context.GetEndpoint();
        /* WRISTBAND_TOUCHPOINT - AUTHENTICATION */
        if (endpoint?.Metadata.GetMetadata<RequireWristbandAuth>() == null)
        {
            await _next(context);
            return;
        }

        // Validate the CSRF secret from the session
        var csrfSecret = SessionUtils.GetStringSessionClaim(context, "csrfSecret");

        /* CSRF_TOUCHPOINT */
        if (string.IsNullOrEmpty(csrfSecret) || !CsrfUtils.IsCsrfTokenValid(context, csrfSecret))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Forbidden access: invalid CSRF token");
            return;
        }

        // Update the CSRF token cookie
        CsrfUtils.UpdateCsrfTokenCookie(context, csrfSecret);

        await _next(context);
    }
}
