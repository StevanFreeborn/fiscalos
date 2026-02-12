namespace FiscalOS.API.Login;

internal static class Endpoint
{
  private const string Route = "/login";

  public static RouteHandlerBuilder MapLoginEndpoint(this WebApplication app)
  {
    return app.MapPost(Route, HandleAsync);
  }

  private static async Task<IResult> HandleAsync(
    HttpContext httpContext,
    [FromBody] Request loginRequest,
    [FromServices] AppDbContext appDbContext,
    [FromServices] IPasswordHasher passwordHasher,
    [FromServices] ITokenGenerator tokenService
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

    user.AddRefreshToken(refreshToken);

    await appDbContext.SaveChangesAsync();

    httpContext.SetRefreshTokenCookie(refreshToken);

    return Results.Ok(Response.From(accessToken));
  }
}