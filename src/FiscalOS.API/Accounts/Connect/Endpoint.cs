namespace FiscalOS.API.Accounts.Connect;

internal static class Endpoint
{
  private const string Route = "/connect";

  public static RouteHandlerBuilder MapConnectEndpoint(this RouteGroupBuilder groupBuilder)
  {
    return groupBuilder.MapPost(Route, HandleAsync);
  }

  private static async Task<IResult> HandleAsync(
    HttpContext httpContext,
    [FromBody] Request request,
    [FromServices] AppDbContext appDbContext
  )
  {
    var userId = httpContext.GetUserId();

    var user = await appDbContext.Users
      .Include(u => u.Institutions
        .Where(
          i => i.Metadata is PlaidMetadata &&
            ((PlaidMetadata)i.Metadata).PlaidId == request.PlaidInstitutionId
        )
      )
      .ThenInclude(i => i.Metadata)
      .SingleOrDefaultAsync(u => u.Id == userId);

    if (user is null)
    {
      return Results.Problem(
        statusCode: StatusCodes.Status401Unauthorized,
        title: "Unauthorized",
        detail: "You are not authorized to connect an institution. Please log in and try again."
      );
    }

    if (user.Institutions.Any())
    {
      return Results.Problem(
        statusCode: StatusCodes.Status409Conflict,
        title: "Institution already connected",
        detail: "The user has already connected an institution with the provided Plaid Institution ID."
      );
    }

    return Results.Ok();
  }
}