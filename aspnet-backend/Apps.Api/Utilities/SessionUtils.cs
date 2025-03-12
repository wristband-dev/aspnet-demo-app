using System.Security.Claims;
using System.Text.Json;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

using Wristband.AspNet.Auth;

public static class SessionUtils
{
  static string? GetUserInfoValue(UserInfo userinfo, string key) =>
      userinfo.TryGetValue(key, out var obj) ? obj.GetString() : null;

  static Claim CreateClaim(string type, string? value) => new(type, value ?? string.Empty);

  public static async Task SetSessionClaims(HttpContext context, CallbackData callbackData, string csrfSecret)
  {
    // Extract fields from userinfo to stick into the session
    var userinfo = callbackData.Userinfo;
    var userId = GetUserInfoValue(userinfo, "sub");
    var email = GetUserInfoValue(userinfo, "email");
    var fullName = GetUserInfoValue(userinfo, "name");
    var idpName = GetUserInfoValue(userinfo, "idp_name");
    var tenantId = GetUserInfoValue(userinfo, "tnt_id");
    var roles = userinfo.TryGetValue("roles", out var rolesClaim) && rolesClaim.ValueKind == JsonValueKind.Array
        ? JsonSerializer.Deserialize<List<WristbandRole>>(rolesClaim.GetRawText()) ?? new List<WristbandRole>()
        : new List<WristbandRole>();

    // Prepare claims for session
    var claims = new List<Claim>
        {
            CreateClaim("isAuthenticated", string.IsNullOrEmpty(callbackData.AccessToken) ? "false" : "true"),
            CreateClaim("accessToken", callbackData.AccessToken),
            CreateClaim("refreshToken", callbackData.RefreshToken),
            // Convert expiration seconds to a Unix timestamp in milliseconds.
            CreateClaim("expiresAt", $"{DateTimeOffset.Now.ToUnixTimeMilliseconds() + (callbackData.ExpiresIn * 1000)}"),
            CreateClaim("email", email),
            CreateClaim("fullName", fullName),
            CreateClaim("idpName", idpName),
            CreateClaim("tenantId", tenantId),
            CreateClaim("userId", userId),
            CreateClaim("roles", JsonSerializer.Serialize(roles)),
            CreateClaim("tenantDomainName", callbackData.TenantDomainName),
            CreateClaim("tenantCustomDomain", callbackData.TenantCustomDomain),
            CreateClaim("csrfSecret", csrfSecret)
        };

    // Save user claims in the session
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), new AuthenticationProperties
    {
      IsPersistent = true,
    });
  }

  public static async Task UpdateTokenClaims(HttpContext context, TokenData? tokenData)
  {
    var claims = context.User.Claims;
    if (tokenData != null)
    {
      // Update token claims if refresh was necessary
      claims = claims
          .Where(c => c.Type is not ("isAuthenticated" or "accessToken" or "refreshToken" or "expiresAt"))
          .Append(new Claim("isAuthenticated", string.IsNullOrEmpty(tokenData.AccessToken) ? "false" : "true"))
          .Append(new Claim("accessToken", tokenData.AccessToken))
          .Append(new Claim("refreshToken", tokenData.RefreshToken ?? string.Empty))
          // Convert expiration seconds to a Unix timestamp in milliseconds.
          .Append(new Claim("expiresAt", $"{DateTimeOffset.Now.ToUnixTimeMilliseconds() + (tokenData.ExpiresIn * 1000)}"))
          .ToList();
    }

    // Save updated claims and touch the session to extend expiration window
    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties
    {
      IsPersistent = true, 
    });
  }

  public static async Task DestroySession(HttpContext context)
  {
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
  }

  public static string GetStringSessionClaim(HttpContext context, string claimName)
  {
    return context.User.FindFirst(claimName)?.Value ?? string.Empty;
  }

  public static List<WristbandRole> GetRoles(HttpContext context)
  {
    var claimValue = context.User.FindFirst("roles")?.Value;
    return !string.IsNullOrEmpty(claimValue)
        ? JsonSerializer.Deserialize<List<WristbandRole>>(claimValue) ?? []
        : [];
  }

  public static bool GetIsAuthenticated(HttpContext context)
  {
    return context.User.FindFirst("isAuthenticated")?.Value == "true";
  }

  public static long GetExpiresAt(HttpContext context)
  {
    return long.TryParse(context.User.FindFirst("expiresAt")?.Value, out var expiresAt) ? expiresAt : 0;
  }
}
