using Microsoft.AspNetCore.Authentication.Cookies;

using DotNetEnv;
using Wristband.AspNet.Auth;

// Load environment variables from ".env" file
Env.Load();

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Add Configuration from multiple sources: "appsettings.json" AND ".env".
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// API-related services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// JSON Configuration
builder.Services.ConfigureHttpJsonOptions(json =>
{
  json.SerializerOptions.WriteIndented = true;
});

/* WRISTBAND_TOUCHPOINT - AUTHENTICATION */
// Add the Wristband Auth SDK
builder.Services.AddWristbandAuth(options =>
{
    options.ClientId = builder.Configuration["CLIENT_ID"];
    options.ClientSecret = builder.Configuration["CLIENT_SECRET"];
    options.LoginUrl = $"http://localhost:6001/api/auth/login";
    options.LoginStateSecret = builder.Configuration["LOGIN_STATE_SECRET"] ?? 
        "dummyval-7GO1imaU48udQnXZqAe3EpmFhNGvQ7Qc3";
    options.RedirectUri = $"http://localhost:6001/api/auth/callback";
    options.Scopes = ["openid", "offline_access", "email", "roles", "profile"];
    options.WristbandApplicationVanityDomain = builder.Configuration["APPLICATION_VANITY_DOMAIN"];
});

// Add cookie session for authenticated users
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // NOTE: Must be "Always" in Production
        options.Cookie.SameSite = SameSiteMode.Strict; // If dealing with CORS, you may need to use "Lax" mode.
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        /* WRISTBAND_TOUCHPOINT - AUTHENTICATION */
        options.UseWristbandApiStatusCodes(); // Return 401 errors codes to client instead of redirecting.
    });

var app = builder.Build();

// Error handling
app.UseUnexpectedErrorHandler();

// API documentation
app.UseSwagger();
app.UseSwaggerUI();

// Auth + CSRF Middlewares
app.UseAuthentication();
app.UseMiddleware<AuthMiddleware>();

// API Routes
app.MapDefaultEndpoints();
app.MapAuthEndpoints();
app.MapDemoResourceEndpoints();

app.Run();
