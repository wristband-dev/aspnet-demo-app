using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public static class CsrfHandler
{
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

        httpContext.Response.Cookies.Append("XSRF-TOKEN", csrfToken, new CookieOptions
        {
            HttpOnly = false, // This makes the cookie accessible from JavaScript on the client-side
            Secure = false, // Use only over HTTPS
            SameSite = SameSiteMode.Strict, // Apply strict SameSite policy
            Path = "/", // The cookie will be sent for all requests to the same domain
            Expires = DateTimeOffset.UtcNow.AddMinutes(30), // Set initial expiration time
            MaxAge = TimeSpan.FromMinutes(30)
        });
    }

    public static bool IsCsrfTokenValid(HttpContext httpContext, string csrfSecret)
    {
        var csrfToken = httpContext.Request.Headers["X-XSRF-TOKEN"].ToString();

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
