using Wristband.AspNet.Auth;

public static class DemoResourceRoutes
{
    private const string Tags = "Demo Resources";

    public static WebApplication MapDemoResourceEndpoints(this WebApplication app)
    {
        // ////////////////////////////////////
        //   UNPROTECTED RESOURCE ENDPOINT
        // ////////////////////////////////////
        app.MapGet("/unprotected",
            () =>
            {
                return Results.Ok(new { Message = "This is an unprotected route.", Value = 1 });
            })
            .WithTags(Tags)
            .WithOpenApi();

        // ////////////////////////////////////
        //   PROTECTED RESOURCE ENDPOINT
        // ////////////////////////////////////
        app.MapGet("/protected",
            (HttpContext httpContext) =>
            {
                return Results.Ok(new { Message = "This is a protected route.", Value = 1 });
            })
            /* WRISTBAND_TOUCHPOINT - AUTHENTICATION */
            .WithMetadata(new RequireWristbandAuth())
            .WithTags(Tags)
            .WithOpenApi();

        return app;
    }
}
