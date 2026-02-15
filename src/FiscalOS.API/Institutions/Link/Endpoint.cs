namespace FiscalOS.API.Institutions.Link;

internal static class Endpoint
{
  private const string Route = "/link";

  public static RouteHandlerBuilder MapLinkEndpoint(this RouteGroupBuilder groupBuilder)
  {
    return groupBuilder.MapPost(Route, HandleAsync);
  }

  private static async Task<IResult> HandleAsync(
    HttpContext httpContext,
    [FromServices] AppDbContext appDbContext,
    [FromServices] PlaidService plaidService
  )
  {
    var userId = httpContext.GetUserId();

    var user = await appDbContext.Users.SingleOrDefaultAsync(u => u.Id == userId);

    if (user is null)
    {
      return Results.Unauthorized();
    }

    var linkToken = await plaidService.CreateLinkTokenAsync(user.Id.ToString());

    return Results.Ok(Response.From(linkToken));
  }
}