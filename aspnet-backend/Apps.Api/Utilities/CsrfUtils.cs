using System.Security.Cryptography;

public static class CsrfUtils
{
    public static string CreateCsrfToken()
    {
      var secretBytes = RandomNumberGenerator.GetBytes(32);
      return Convert.ToHexString(secretBytes).ToLower();
    }

    public static void UpdateCsrfCookie(HttpContext httpContext, string csrfToken)
    {
        httpContext.Response.Cookies.Append("CSRF-TOKEN", csrfToken, new CookieOptions
        {
            HttpOnly = false,
            Secure = false, // IMPORTANT: Secure must be "true" in Production!!
            SameSite = SameSiteMode.Strict, // If dealing with CORS, you may need to use "Lax" mode.
            Path = "/",
            // Match the session cookie expiration
            Expires = DateTimeOffset.UtcNow.AddMinutes(30),
            MaxAge = TimeSpan.FromMinutes(30)
        });
    }
}
