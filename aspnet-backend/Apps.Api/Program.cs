using DotNetEnv;
using Microsoft.AspNetCore.Authentication.Cookies;
using Wristband.AspNet.Auth;

// =============================================================================
// CONFIGURATION & ENVIRONMENT
// =============================================================================

// Load environment variables from ".env" file
Env.Load();

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Add Configuration from multiple sources: "appsettings.json" AND ".env".
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// =============================================================================
// CORE SERVICES
// =============================================================================

// API documentation & tooling
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HTTP services
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// JSON formatting
builder.Services.ConfigureHttpJsonOptions(json => json.SerializerOptions.WriteIndented = true);

// =============================================================================
// AUTHENTICATION & AUTHORIZATION CONFIGURATION
// Registers authentication schemes, authorization policies, and handlers.
// This defines HOW authentication/authorization works, not WHEN it runs.
// =============================================================================

/* WRISTBAND_TOUCHPOINT - Wristband Auth Service: Handles OAuth2 login/logout flows */
builder.Services.AddWristbandAuth(options =>
{
  options.ClientId = builder.Configuration["CLIENT_ID"];
  options.ClientSecret = builder.Configuration["CLIENT_SECRET"];
  options.WristbandApplicationVanityDomain = builder.Configuration["APPLICATION_VANITY_DOMAIN"];
  options.Scopes = ["openid", "offline_access", "email", "roles", "profile"];
  options.DangerouslyDisableSecureCookies = true; // IMPORTANT: Must be "false" in Production!!
});

// Optional: Enable in-memory key data protection for zero-infrastructure session encryption.
// Derives encryption keys from a secret (stored in env vars/K8s Secrets/Key Vault), eliminating
// the need for Redis, persistent volumes, or other key storage infrastructure.
// Only required for multi-server deployments - single-server apps work without this.
builder.Services.AddInMemoryKeyDataProtection(["your-secret-key-min-32-characters-long"]);

/* WRISTBAND_TOUCHPOINT - Authentication Schemes: Configure the ways users can authenticate requests */
builder.Services
    // Default to Cookie auth for unprotected endpoints (needed for logout, etc.)
    // Protected endpoints override this via their authorization policies
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    // Cookie scheme: Stores session data in encrypted browser cookie
    .AddCookie(options =>
    {
        options.UseWristbandSessionConfig(); /* WRISTBAND_TOUCHPOINT - AUTHENTICATION */
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // IMPORTANT: Must be "Always" in Production!!
        // options.ExpireTimeSpan = TimeSpan.FromHours(1); <-- Override this for a non-default session expiration time
    })
    // JWT Bearer scheme: Validates JWT tokens from Authorization header
    .AddJwtBearer(options => options.UseWristbandJwksValidation(
        wristbandApplicationVanityDomain: builder.Configuration["APPLICATION_VANITY_DOMAIN"]!
    ));

// WRISTBAND_TOUCHPOINT - CSRF: Protection with Synchronizer Token Pattern
builder.Services.AddWristbandCsrfProtection();

/* WRISTBAND_TOUCHPOINT - Authorization: Register handler and policies for protecting endpoints */
// Handler: Validates session cookies and JWT tokens; refreshes expired access tokens
builder.Services.AddWristbandAuthorizationHandler();
// Policies: Defines "WristbandSession" and "WristbandJwt" authorization policies
builder.Services.AddAuthorization(options => options.AddWristbandDefaultPolicies());

// =============================================================================
// BUILD APPLICATION
// =============================================================================

var app = builder.Build();

// =============================================================================
// GLOBAL MIDDLEWARE
// =============================================================================

// Error handling
app.UseUnexpectedErrorHandler();

// API documentation
app.UseSwagger();
app.UseSwaggerUI();

// =============================================================================
// AUTHENTICATION & AUTHORIZATION MIDDLEWARE - ORDER MATTERS!!!
// These middlewares execute the authentication/authorization configured above
// They run on EVERY request before reaching your endpoints
// =============================================================================

/* Authentication Middleware: Populates HttpContext.User from cookie/JWT */
app.UseAuthentication();

/* Authorization Middleware: Enforces authorization policies on endpoints */
app.UseAuthorization();

/* WRISTBAND_TOUCHPOINT - AUTHENTICATION: Persists updated session data to cookie after authorization */
app.UseWristbandSessionMiddleware();

// -----------------------------------------------------------------------------
// API ROUTES
// -----------------------------------------------------------------------------

app.MapDefaultEndpoints();
app.MapAuthEndpoints();
app.MapProtectedDemoEndpoints();

// =============================================================================
// RUN APPLICATION
// =============================================================================

app.Run();
