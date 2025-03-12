using Microsoft.AspNetCore.Diagnostics;

public static class ErrorHandlerExtensions
{
    public static WebApplication UseUnexpectedErrorHandler(this WebApplication app)
    {
        app.UseExceptionHandler(new ExceptionHandlerOptions
        {
            ExceptionHandler = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var error = new
                {
                    message = app.Environment.IsDevelopment() 
                        ? context.Features.Get<IExceptionHandlerFeature>()?.Error?.ToString()
                        : "An unexpected error occurred"
                };

                await context.Response.WriteAsJsonAsync(error);
            }
        });

        return app;
    }
}
