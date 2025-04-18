using System.Security.Claims;

using Wristband.AspNet.Auth;

public static class AuthRoutes
{
    private const string Tags = "Authentication";

    public static WebApplication MapAuthEndpoints(this WebApplication app)
    {
        // ////////////////////////////////////
        //   LOGIN ENDPOINT
        // ////////////////////////////////////
        app.MapGet("/auth/login", async (HttpContext httpContext, IWristbandAuthService wristbandAuth) =>
        {
            try
            {
                /* WRISTBAND_TOUCHPOINT - AUTHENTICATION */
                var wristbandLoginUrl = await wristbandAuth.Login(httpContext, null);
                return Results.Redirect(wristbandLoginUrl);
            } catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex}");
                return Results.Problem(detail: $"Unexpected error: {ex.Message}", statusCode: 500);
            }
        })
        .WithTags(Tags)
        .WithOpenApi();

        // ////////////////////////////////////
        //   CALLBACK ENDPOINT
        // ////////////////////////////////////
        app.MapGet("/auth/callback", async (HttpContext httpContext, IWristbandAuthService wristbandAuth) =>
        {
            try
            {
                /* WRISTBAND_TOUCHPOINT - AUTHENTICATION */
                var callbackResult = await wristbandAuth.Callback(httpContext);

                // Some edge cases will attempt to redirect to a login URL to restart the flow.
                if (callbackResult.Result == CallbackResultType.REDIRECT_REQUIRED)
                {
                    return Results.Redirect(callbackResult.RedirectUrl);
                }

                // Generate the CSRF secret.
                var csrfSecret = CsrfUtils.GenerateCsrfSecret();

                // Initialize the auth session cookie.
                await SessionUtils.SetSessionClaims(httpContext, callbackResult.CallbackData, csrfSecret);

                // Generate the CSRF token cookie.
                CsrfUtils.UpdateCsrfTokenCookie(httpContext, csrfSecret);

                var tenantPostLoginRedirectUrl = Environment.GetEnvironmentVariable("DOMAIN_FORMAT") == "VANITY_DOMAIN"
                        ? $"http://{callbackResult.CallbackData.TenantDomainName}.business.invotastic.com:6001"
                        : "http://localhost:6001";
                return Results.Redirect(tenantPostLoginRedirectUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex}");
                return Results.Problem(detail: $"Unexpected error: {ex.Message}", statusCode: 500);
            }
        })
        .WithTags(Tags)
        .WithOpenApi();

        // ////////////////////////////////////
        //   LOGOUT ENDPOINT
        // ////////////////////////////////////
        app.MapGet("/auth/logout", async (HttpContext httpContext, IWristbandAuthService wristbandAuth) =>
        {
            httpContext.Response.Cookies.Delete("XSRF-TOKEN");

            var refreshToken = SessionUtils.GetStringSessionClaim(httpContext, "refreshToken");
            var tenantCustomDomain = SessionUtils.GetStringSessionClaim(httpContext, "tenantCustomDomain");
            var tenantDomainName = SessionUtils.GetStringSessionClaim(httpContext, "tenantDomainName");
            
            await SessionUtils.DestroySession(httpContext);

            /* WRISTBAND_TOUCHPOINT - AUTHENTICATION */
            var wristbandLogoutUrl = await wristbandAuth.Logout(httpContext, new LogoutConfig
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
        app.MapGet("/session", (HttpContext httpContext) =>
        {

            //
            // NOTE: You can optionally make API requests for additional data that you might want
            // to return to the frontend in your Session Response.
            //

            return Results.Ok(new
            {
                userId = SessionUtils.GetStringSessionClaim(httpContext, "userId"),
                tenantId = SessionUtils.GetStringSessionClaim(httpContext, "tenantId"),
                // NOTE: If you want to avoid stale data, you should load any metadata values from your backend datastore
                // instead of relying on values in your cookie session.
                metadata = new
                {
                    email = SessionUtils.GetStringSessionClaim(httpContext, "email"),
                    fullName = SessionUtils.GetStringSessionClaim(httpContext, "fullName"),
                    roles = SessionUtils.GetRoles(httpContext),
                    tenantDomainName = SessionUtils.GetStringSessionClaim(httpContext, "tenantDomainName")
                }
            });
        })
        /* WRISTBAND_TOUCHPOINT - AUTHENTICATION */
        .WithMetadata(new RequireWristbandAuth())
        .WithTags(Tags)
        .WithOpenApi();

        return app;
    }
}
