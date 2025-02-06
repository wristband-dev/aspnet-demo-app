using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

// Import the Wristband Auth SDK
using Wristband;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

var services = builder.Services;
services.AddDistributedMemoryCache();
services.AddHttpContextAccessor();
services.AddHttpClient();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
services.ConfigureHttpJsonOptions(json =>
{
  json.SerializerOptions.WriteIndented = true;
});
services.AddAuthorization();

// Configure the Wristband SDK for usage in the server
services.Configure<AuthConfig>(builder.Configuration.GetSection("Wristband:AuthConfig"));
services.AddScoped<IWristbandAuthService>(serviceProvider =>
{
  var authConfig = serviceProvider.GetRequiredService<IOptions<AuthConfig>>().Value;
  return new WristbandAuthService(authConfig);
});

// Add cookie session for authenticated users
services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        // Instead of doing an immediate redirect to some page, simply return the error code and let the
        // client decide what to do from there.
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

// Enable CSRF Protection with cookies
services.AddAntiforgery(options =>
{
  options.HeaderName = "X-XSRF-TOKEN"; // The header where the token will be submitted from the client

  // The token will be stored in a cookie that the client sends back with each request
  options.Cookie.Name = "XSRF-TOKEN"; // Name of the CSRF cookie
  options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Use same as the request for secure cookies
  options.Cookie.SameSite = SameSiteMode.Strict; // Apply Strict SameSite policy
  options.Cookie.HttpOnly = false; // Make the cookie accessible by JavaScript
  options.Cookie.Path = "/"; // Apply the cookie to the entire site
});

var app = builder.Build();

if (!builder.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Error");
}

app.UseHsts();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseRouting();

// We use Cookies Authentication to store a client-side Cookies Session.
app.UseAuthentication();
app.UseAuthorization();

// Auth + CSRF Middlewares
app.UseMiddleware<AuthMiddleware>();
app.UseMiddleware<CsrfMiddleware>();

// Endpoints
app.MapGet("/", () => "Please visit /api/swagger for API documentation").ExcludeFromDescription();
app.MapDefaultEndpoints();
app.MapAuthEndpoints();
app.MapDemoResourceEndpoints();

app.Run();
