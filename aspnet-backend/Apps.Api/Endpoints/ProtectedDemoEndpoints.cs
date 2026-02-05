using Wristband.AspNet.Auth;

public static class ProtectedDemoEndpoints
{
    public static RouteGroupBuilder MapProtectedDemoEndpoints(this WebApplication app)
    {
        var protectedDemoRoutes = app.MapGroup("")
            .WithTags("Demo Resources")
            .WithOpenApi();

        /****** SESSION-PROTECTED ENDPOINTS ******/

        var sessionProtectedRoutes = protectedDemoRoutes.MapGroup("/session")
            .RequireWristbandSession(); // WRISTBAND_TOUCHPOINT - AUTHENTICATION
        
        sessionProtectedRoutes.MapGet("/protected", (HttpContext ctx) =>
        {
            // Access session data using extension methods for known Wrisband fields
            var userId = ctx.GetUserId();
            var tenantId = ctx.GetTenantId();

            // You can also add, update, or remove session data with extension methods
            var randomNumber = Random.Shared.Next(1, 10000);
            ctx.SetSessionClaim("randomNumber", randomNumber.ToString()); // <-- Set claim example
            // ctx.RemoveSessionClaim("randomNumber"); <-- Remove claim example

            return Results.Ok(new {
                Message = "This is a session-protected route.",
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                UserId = userId,
                TenantId = tenantId,
                RandomNumber = randomNumber,
            });
        });

        /****** JWT-PROTECTED ENDPOINTS ******/

        var jwtProtectedRoutes = protectedDemoRoutes.MapGroup("/jwt")
            .RequireWristbandJwt(); // WRISTBAND_TOUCHPOINT - AUTHENTICATION

        jwtProtectedRoutes.MapGet("/protected", (HttpContext ctx) =>
        {
            // Access JWT data using extension methods
            var token = ctx.GetJwt();
            var payload = ctx.GetJwtPayload();

            return Results.Ok(new 
            { 
                Message = "This is a JWT-protected route.",
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                UserId = payload.Sub,
                TenantId = payload.Claims?["tnt_id"],
                Token = token,
            });
        });

        return protectedDemoRoutes;
    }
}
