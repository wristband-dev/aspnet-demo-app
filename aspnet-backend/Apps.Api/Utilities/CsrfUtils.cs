using System.Security.Cryptography;
using System.Text;

public static class CsrfUtils
{
    private const string _xsrfCookieName = "XSRF-TOKEN";
    private const string _xsrfTokenHeaderName = "X-XSRF-TOKEN";

    public static string GenerateCsrfSecret()
    {
      var secretBytes = RandomNumberGenerator.GetBytes(32);
      return Convert.ToBase64String(secretBytes);
    }

    public static void UpdateCsrfTokenCookie(HttpContext httpContext, string csrfSecret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(csrfSecret));
        var tokenBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(csrfSecret));
        var csrfToken = Convert.ToBase64String(tokenBytes);

        httpContext.Response.Cookies.Append(_xsrfCookieName, csrfToken, new CookieOptions
        {
            HttpOnly = false,
            Secure = false, // NOTE: Must be "true" in Production
            SameSite = SameSiteMode.Strict, // If dealing with CORS, you may need to use "Lax" mode.
            Path = "/",
            // Ideally should match the session cookie expiration
            Expires = DateTimeOffset.UtcNow.AddMinutes(30),
            MaxAge = TimeSpan.FromMinutes(30)
        });
    }

    public static bool IsCsrfTokenValid(HttpContext httpContext, string csrfSecret)
    {
        var csrfToken = string.Empty;
        if (httpContext.Request.Headers.TryGetValue(_xsrfTokenHeaderName, out var token))
        {
            csrfToken = token.FirstOrDefault()?.ToString() ?? string.Empty;
        }

        if (string.IsNullOrEmpty(csrfSecret) || string.IsNullOrEmpty(csrfToken))
        {
            return false;
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(csrfSecret));
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(csrfSecret));
        var computedToken = Convert.ToBase64String(computedHash);
        return csrfToken == computedToken;
    }
}
