using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

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
            [Authorize(/*"some:permission"*/)]
            (HttpContext httpContext) =>
            {
                return Results.Ok(new { Message = "This is a protected route.", Value = 1 });
            })
            .WithMetadata(new RequireWristbandAuthAttribute())
            .WithTags(Tags)
            .WithOpenApi();

        return app;
    }
}
