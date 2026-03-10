namespace FiscalOS.API.Auth.Logout;

internal static class Endpoint
{
  private const string Route = "/logout";

  public static RouteHandlerBuilder MapLogoutEndpoint(this RouteGroupBuilder groupBuilder)
  {
    return groupBuilder.MapPost(Route, HandleAsync);
  }

  private static async Task<IResult> HandleAsync(
    HttpContext httpContext,
    [FromServices] AppDbContext appDbContext,
    CancellationToken ct
  )
  {
    var userId = httpContext.GetUserId();
    var refreshToken = httpContext.GetRefreshTokenFromCookie();

    var user = await appDbContext.Users
      .Include(u => u.RefreshTokens
        .Where(r => r.Token == refreshToken)
      )
      .Where(u => u.Id == userId)
      .SingleOrDefaultAsync(ct);

    if (user is null || user.RefreshTokens.Any() is false)
    {
      return Results.Unauthorized();
    }

    user.RefreshTokens.First().Revoke();
    await appDbContext.SaveChangesAsync(ct);

    httpContext.ExpireRefreshTokenCookie();

    return Results.NoContent();
  }
}