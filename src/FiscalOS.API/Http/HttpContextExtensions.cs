namespace FiscalOS.API.Http;

internal static class HttpContextExtensions
{
  private const string RefreshTokenCookieName = "fiscalos_refresh_cookie";

  public static Guid GetUserId(this HttpContext context)
  {
    var id = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    return Guid.TryParse(id, out var userId) ? userId : Guid.Empty;
  }

  public static void SetRefreshTokenCookie(this HttpContext context, RefreshToken token)
  {
    context.Response.Cookies.Append(
      RefreshTokenCookieName,
      token.Token,
      new CookieOptions
      {
        HttpOnly = true,
        Expires = token.ExpiresAt,
        SameSite = SameSiteMode.Strict,
        Secure = true
      }
    );
  }

  public static string GetRefreshTokenFromCookie(this HttpContext context)
  {
    return context.Request.Cookies[RefreshTokenCookieName] ?? string.Empty;
  }
}