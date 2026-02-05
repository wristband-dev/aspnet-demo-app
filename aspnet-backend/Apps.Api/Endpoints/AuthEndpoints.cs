using System.Security.Claims;
using System.Text.Json;

using Wristband.AspNet.Auth;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this WebApplication app)
    {
        var authRoutes = app.MapGroup("/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        /****** LOGIN ENDPOINT ******/

        authRoutes.MapGet("/login", async (HttpContext ctx, IWristbandAuthService wristbandAuth) =>
        {
            try
            {
                /* WRISTBAND_TOUCHPOINT - AUTHENTICATION */
                var wristbandAuthorizeUrl = await wristbandAuth.Login(ctx, null);
                return Results.Redirect(wristbandAuthorizeUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex}");
                return Results.Problem(detail: $"Unexpected error: {ex.Message}", statusCode: 500);
            }
        });

        /****** CALLBACK ENDPOINT ******/

        authRoutes.MapGet("/callback", async (HttpContext ctx, IWristbandAuthService wristbandAuth) =>
        {
            try
            {
                /* WRISTBAND_TOUCHPOINT - AUTHENTICATION */
                var callbackResult = await wristbandAuth.Callback(ctx);

                // Some edge cases will attempt to redirect to a login URL to restart the flow.
                if (callbackResult.Type == CallbackResultType.RedirectRequired)
                {
                    return Results.Redirect(callbackResult.RedirectUrl);
                }

                // Extract additional fields from userinfo
                var userinfo = callbackResult.CallbackData.Userinfo;
                var email = userinfo.Email;
                var roles = userinfo.Roles;
                var customClaims = new List<Claim>();
                if (!string.IsNullOrEmpty(email))
                {
                    customClaims.Add(new Claim("email", email));
                }
                if (roles != null && roles.Count > 0)
                {
                    customClaims.Add(new Claim("roles", JsonSerializer.Serialize(roles)));
                }

                // Create session from callback data
                ctx.CreateSessionFromCallback(callbackResult.CallbackData, customClaims);

                return Results.Redirect(callbackResult.CallbackData.ReturnUrl ?? "http://localhost:6001");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex}");
                return Results.Problem(detail: $"Unexpected error: {ex.Message}", statusCode: 500);
            }
        });

        /****** LOGOUT ENDPOINT ******/

        authRoutes.MapGet("/logout", async (HttpContext ctx, IWristbandAuthService wristbandAuth) =>
        {
            var logoutConfig = new LogoutConfig
            {
                RefreshToken = ctx.GetRefreshToken(),
                TenantCustomDomain = ctx.GetTenantCustomDomain(),
                TenantName = ctx.GetTenantName(),
            };
    
            ctx.DestroySession();

            /* WRISTBAND_TOUCHPOINT - AUTHENTICATION */
            var wristbandLogoutUrl = await wristbandAuth.Logout(ctx, logoutConfig);
            return Results.Redirect(wristbandLogoutUrl);
        });

        /****** SESSION ENDPOINT ******/

        authRoutes.MapGet("/session", (HttpContext ctx) =>
        {
            // Session response with custom metadata (as expected by frontend SDK)
            var response = ctx.GetSessionResponse(metadata: new
            {
                tenantName = ctx.GetTenantName(),
                roles = ctx.GetRoles(),
                email = ctx.GetSessionClaim("email"),
            });
            return Results.Ok(response);
        })
        .RequireWristbandSession();  // WRISTBAND_TOUCHPOINT - AUTHENTICATION

        /****** TOKEN ENDPOINT ******/

        authRoutes.MapGet("/token", (HttpContext ctx) =>
        {
            var response = ctx.GetTokenResponse();
            return Results.Ok(response);
        })
        .RequireWristbandSession();  // WRISTBAND_TOUCHPOINT - AUTHENTICATION

        return authRoutes;
    }
}
