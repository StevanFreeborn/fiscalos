
using FiscalOS.Core.Identity;

namespace FiscalOS.API.Login;

internal static class Endpoint
{
  private const string Route = "/login";

  public static RouteHandlerBuilder MapLoginEndpoint(this WebApplication app)
  {
    return app.MapPost(Route, HandleAsync);
  }

  private static async Task<IResult> HandleAsync(
    [FromBody] LoginRequest loginRequest,
    [FromServices] HttpContext httpContext,
    [FromServices] AppDbContext appDbContext,
    [FromServices] IPasswordHasher passwordHasher,
    [FromServices] TokenService tokenService
  )
  {
    var user = await appDbContext.Users.SingleOrDefaultAsync(u => u.Username == loginRequest.Username);

    if (user is null)
    {
      return Results.Unauthorized();
    }

    var isCorrectPassword = passwordHasher.Verify(loginRequest.Password, user.HashedPassword);

    if (isCorrectPassword is false)
    {
      return Results.Unauthorized();
    }

    var accessToken = tokenService.GenerateAccessToken(user);

    var refreshToken = tokenService.GenerateRefreshToken(user);
    await appDbContext.RefreshTokens.AddAsync(refreshToken);
    await appDbContext.SaveChangesAsync();

    httpContext.Response.Cookies.Append(
      "fiscalos_refresh_cookie",
      refreshToken.Token,
      new CookieOptions
      {
        HttpOnly = true,
        Expires = refreshToken.ExpiresAt,
        // TODO: Revisit when decided
        // on hosting setup
        SameSite = SameSiteMode.None,
        Secure = true
      }
    );

    return Results.Ok(new Response(accessToken));
  }
}