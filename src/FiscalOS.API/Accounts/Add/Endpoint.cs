namespace FiscalOS.API.Accounts.Add;

internal static class Endpoint
{
  private const string Route = "/";

  public static RouteHandlerBuilder MapAddEndpoint(this RouteGroupBuilder groupBuilder)
  {
    return groupBuilder.MapPost(Route, HandleAsync);
  }

  private static async Task<IResult> HandleAsync(
    HttpContext httpContext,
    [FromBody] Request request,
    [FromServices] AppDbContext appDbContext,
    CancellationToken ct
  )
  {
    var userId = httpContext.GetUserId();

    var user = await appDbContext.Users
      .Include(u => u.Accounts.Where(a => a.Metadata is PlaidAccountMetadata && ((PlaidAccountMetadata)a.Metadata).PlaidId == request.PlaidAccountId))
      .ThenInclude(a => a.Metadata)
      .SingleOrDefaultAsync(u => u.Id == userId, ct);

    if (user is null)
    {
      return Results.Unauthorized();
    }

    if (user.Accounts.Any())
    {
      return Results.Conflict();
    }

    // TODO: This is new account
    // so we should add to the database

    return Results.Ok();
  }
}