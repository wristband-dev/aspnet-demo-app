using Microsoft.AspNetCore.Http;

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
        if (endpoint?.Metadata.GetMetadata<RequireWristbandAuthAttribute>() == null)
        {
            await _next(context);
            return;
        }

        // Validate the CSRF secret from the session
        var csrfSecret = SessionHandler.GetCsrfSecret(context);

        /* CSRF_TOUCHPOINT */
        if (string.IsNullOrEmpty(csrfSecret) || !CsrfHandler.IsCsrfTokenValid(context, csrfSecret))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Forbidden access: invalid CSRF token");
            return;
        }

        // Update the CSRF token cookie
        CsrfHandler.UpdateCsrfTokenCookie(context, csrfSecret);

        await _next(context);
    }
}
