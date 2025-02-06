using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Antiforgery;

// Import Wristband Auth SDK
using Wristband;

public static class AuthRoutes
{
    private const string Tags = "Authentication";

    public static WebApplication MapAuthEndpoints(this WebApplication app)
    {
        // ////////////////////////////////////
        //   LOGIN ENDPOINT
        // ////////////////////////////////////
        app.MapGet("/auth/login", async (HttpContext httpContext, IWristbandAuthService authService) =>
        {
            try {
                var wristbandLoginUrl = await authService.Login(httpContext, null);
                return Results.Redirect(wristbandLoginUrl);
            } catch (Exception ex) {
                Console.WriteLine($"Unexpected error: {ex}");
                return Results.Problem(detail: $"Unexpected error: {ex.Message}", statusCode: 500);
            }
        })
        .WithTags(Tags)
        .WithOpenApi();

        // ////////////////////////////////////
        //   CALLBACK ENDPOINT
        // ////////////////////////////////////
        app.MapGet("/auth/callback", async (HttpContext httpContext, IWristbandAuthService authService) =>
        {
            try {
                // Call the Wristband Callback() function.
                var callbackResult = await authService.Callback(httpContext);

                if (callbackResult.Result == CallbackResultType.REDIRECT_REQUIRED)
                {
                    return Results.Redirect(callbackResult.RedirectUrl);
                }

                // Generate the CSRF secret
                var csrfSecret = CsrfHandler.GenerateCsrfSecret();

                // Initialize the auth session cookie
                await SessionHandler.SetSessionClaims(httpContext, callbackResult.CallbackData, csrfSecret);

                // Generate the CSRF token cookie
                CsrfHandler.UpdateCsrfTokenCookie(httpContext, csrfSecret);

                var tenantPostLoginRedirectUrl = $"http://{callbackResult.CallbackData.TenantDomainName}.business.invotastic.com:6001/";
                return Results.Redirect(tenantPostLoginRedirectUrl);
            } catch (Exception ex) {
                Console.WriteLine($"Unexpected error: {ex}");
                return Results.Problem(detail: $"Unexpected error: {ex.Message}", statusCode: 500);
            }
        })
        .WithTags(Tags)
        .WithOpenApi();

        // ////////////////////////////////////
        //   LOGOUT ENDPOINT
        // ////////////////////////////////////
        app.MapGet("/auth/logout", async (HttpContext httpContext, IWristbandAuthService authService) =>
        {
            httpContext.Response.Cookies.Delete("XSRF-TOKEN");

            var refreshToken = SessionHandler.GetRefreshToken(httpContext);
            var tenantCustomDomain = SessionHandler.GetTenantCustomDomain(httpContext);
            var tenantDomainName = SessionHandler.GetTenantDomainName(httpContext);
            
            await SessionHandler.RemoveWristbandSessionKeys(httpContext);

            var wristbandLogoutUrl = await authService.Logout(httpContext, new LogoutConfig
            {
                RedirectUrl = null,
                RefreshToken = refreshToken ?? null,
                TenantCustomDomain = tenantCustomDomain ?? null,
                TenantDomainName = tenantDomainName ?? null,
            });

            return Results.Redirect(wristbandLogoutUrl);
        })
        .WithTags(Tags)
        .WithOpenApi();

        // ////////////////////////////////////
        //   SESSION ENDPOINT
        // ////////////////////////////////////
        app.MapGet("/session", (HttpContext httpContext, IWristbandAuthService authService) =>
        {
            var isAuthenticated = SessionHandler.GetIsAuthenticated(httpContext);
            var user = httpContext.User;

            if (!isAuthenticated || user?.Identity == null)
            {
                return Results.Ok(new { IsAuthenticated = false, Name = "", Email = "" });
            }

            return Results.Ok(new 
            { 
                IsAuthenticated = isAuthenticated,
                Name = user.Identity.Name ?? "",
                Email = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "",
            });
        })
        .WithMetadata(new RequireWristbandAuthAttribute())
        .WithTags(Tags)
        .WithOpenApi();

        return app;
    }
}
