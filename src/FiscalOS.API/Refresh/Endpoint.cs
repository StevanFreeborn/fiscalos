namespace FiscalOS.API.Refresh;

internal static class Endpoint
{
  private const string Route = "/refresh";

  public static RouteHandlerBuilder MapRefreshEndpoint(this WebApplication app)
  {
    return app.MapPost(Route, HandleAsync);
  }

  private static async Task<IResult> HandleAsync(
    HttpContext httpContext,
    [FromServices] AppDbContext appDbContext,
    [FromServices] TimeProvider timeProvider,
    [FromServices] ITokenGenerator tokenGenerator
  )
  {
    var requestUserId = httpContext.GetUserId();
    var token = httpContext.GetRefreshTokenFromCookie();

    var refreshToken = await appDbContext.RefreshTokens
      .Include(t => t.User)
      .SingleOrDefaultAsync(t => t.Token == token);

    if (refreshToken is null || refreshToken.User is null)
    {
      return Results.BadRequest();
    }

    if (refreshToken.UserId != requestUserId)
    {
      await appDbContext.RefreshTokens
        .Where(t => t.UserId == refreshToken.UserId)
        .ExecuteUpdateAsync(t => t.SetProperty(t => t.Revoked, true));

      await appDbContext.SaveChangesAsync();

      return Results.Forbid();
    }

    var now = timeProvider.GetUtcNow();

    if (refreshToken.Revoked || refreshToken.IsExpired(now))
    {
      return Results.BadRequest();
    }

    var accessToken = tokenGenerator.GenerateAccessToken(refreshToken.User);
    var newRefreshToken = tokenGenerator.GenerateRefreshToken(refreshToken.User);

    refreshToken.Revoke();
    await appDbContext.RefreshTokens.AddAsync(newRefreshToken);
    await appDbContext.SaveChangesAsync();

    httpContext.SetRefreshTokenCookie(newRefreshToken);

    return Results.Ok(Response.From(accessToken));
  }
}