using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

// Import Wristband Auth SDK
using Wristband;

public static class SessionHandler
{
    public static async Task SetSessionClaims(HttpContext context, CallbackData callbackData, string csrfSecret)
    {
        // Extract fields from userinfo to stick into session claims
        var userinfo = callbackData.Userinfo;
        var userId = userinfo.TryGetValue("sub", out var userIdObj) ? userIdObj.GetString() : null;
        var email = userinfo.TryGetValue("email", out var emailObj) ? emailObj.GetString() : null;
        var name = userinfo.TryGetValue("name", out var nameObj) ? nameObj.GetString() : null;
        var idpName = userinfo.TryGetValue("idp_name", out var idpObj) ? idpObj.GetString() : null;
        var tenantId = userinfo.TryGetValue("tnt_id", out var tntObj) ? tntObj.GetString() : null;

        // Parse the roles for the session claims
        var roles = new List<WristbandRole>();
        if (userinfo.TryGetValue("roles", out var rolesObj))
        {
            var jsonElement = rolesObj is JsonElement obj ? obj : default;
            if (jsonElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in jsonElement.EnumerateArray())
                {
                    var role = JsonSerializer.Deserialize<WristbandRole>(element.GetRawText());
                    if (role != null)
                    {
                        roles.Add(role);
                    }
                }
            }
        }

        // Prepare claims for session
        var claims = new List<Claim>
        {
            new Claim("wristband:isAuthenticated", string.IsNullOrEmpty(callbackData.AccessToken) ? "false" : "true"),
            new Claim("wristband:accessToken", callbackData.AccessToken),
            new Claim("wristband:refreshToken", callbackData.RefreshToken ?? string.Empty),
            new Claim("wristband:expiresAt", $"{DateTimeOffset.Now.ToUnixTimeMilliseconds() + (callbackData.ExpiresIn * 1000)}"),

            new Claim("wristband:email", email ?? string.Empty),
            new Claim("wristband:name", name ?? string.Empty),
            new Claim("wristband:idpName", idpName ?? string.Empty),
            new Claim("wristband:tenantId", tenantId ?? string.Empty),
            new Claim("wristband:userId", userId ?? string.Empty),
            new Claim("wristband:roles", JsonSerializer.Serialize(roles)),
            new Claim("wristband:tenantDomainName", callbackData.TenantDomainName),

            new Claim("csrf_secret", csrfSecret)
        };

        // Create claims identity
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        // Sign in the user with the claims
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties
        {
            IsPersistent = true,
        });
    }

    public static async Task UpdateTokenClaims(HttpContext context, TokenData tokenData)
    {
        // Update claims and refresh expiration
        if (tokenData == null) {
            var updatedClaimsIdentity = new ClaimsIdentity(context.User.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(updatedClaimsIdentity), new AuthenticationProperties
            {
                IsPersistent = true, // Touch the session to extend expiration window
            });

            return;
        }

        // Retrieve existing claims
        var existingClaims = context.User.Claims.ToList() ?? new List<Claim>();

        // Update or add claims
        var claimsToRemove = new[] { "wristband:isAuthenticated", "wristband:accessToken", "wristband:refreshToken", "wristband:expiresAt" };
        var updatedClaims = existingClaims.Where(c => !claimsToRemove.Contains(c.Type)).ToList();
        updatedClaims.Add(new Claim("wristband:isAuthenticated", string.IsNullOrEmpty(tokenData.AccessToken) ? "false" : "true"));
        updatedClaims.Add(new Claim("wristband:accessToken", tokenData.AccessToken));
        updatedClaims.Add(new Claim("wristband:refreshToken", tokenData.RefreshToken ?? string.Empty));
        updatedClaims.Add(new Claim("wristband:expiresAt", $"{DateTimeOffset.Now.ToUnixTimeMilliseconds() + (tokenData.ExpiresIn * 1000)}"));

        // Save updated claims
        var claimsIdentity = new ClaimsIdentity(updatedClaims, CookieAuthenticationDefaults.AuthenticationScheme);
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties
        {
            IsPersistent = true,
        });
    }

    public static async Task RemoveWristbandSessionKeys(HttpContext context)
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    // ////////////////////////////////////////////////////////////
    //   CLAIM METHODS
    // ////////////////////////////////////////////////////////////

    public static string GetTenantDomainName(HttpContext context)
    {
        var tenantDomainNameClaim = context.User.FindFirst("wristband:tenantDomainName");
        return tenantDomainNameClaim?.Value ?? string.Empty;
    }

    public static string GetTenantCustomDomain(HttpContext context)
    {
        var tenantCustomDomainClaim = context.User.FindFirst("wristband:tenantCustomDomain");
        return tenantCustomDomainClaim?.Value ?? string.Empty;
    }

    public static bool GetIsAuthenticated(HttpContext context)
    {
        var isAuthenticatedClaim = context.User.FindFirst("wristband:isAuthenticated");
        return isAuthenticatedClaim != null && isAuthenticatedClaim.Value == "true";
    }

    public static string GetAccessToken(HttpContext context)
    {
        var accessTokenClaim = context.User.FindFirst("wristband:accessToken");
        return accessTokenClaim?.Value ?? string.Empty;
    }

    public static string GetRefreshToken(HttpContext context)
    {
        var refreshTokenClaim = context.User.FindFirst("wristband:refreshToken");
        return refreshTokenClaim?.Value ?? string.Empty;
    }

    public static long GetExpiresAt(HttpContext context)
    {
        var expiresAtClaim = context.User.FindFirst("wristband:expiresAt");
        return expiresAtClaim != null && long.TryParse(expiresAtClaim.Value, out var expiresAt) ? expiresAt : 0;
    }

    public static string? GetCsrfSecret(HttpContext context)
    {
        return context.User.Claims.FirstOrDefault(c => c.Type == "csrf_secret")?.Value;
    }
}
