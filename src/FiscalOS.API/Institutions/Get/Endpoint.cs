namespace FiscalOS.API.Institutions.Get;

internal static class Endpoint
{
  private const string Route = "/";

  public static RouteHandlerBuilder MapGetEndpoint(this RouteGroupBuilder groupBuilder)
  {
    return groupBuilder.MapGet(Route, HandleAsync);
  }

  private static async Task<IResult> HandleAsync(
    HttpContext httpContext,
    [FromServices] AppDbContext appDbContext
  )
  {
    var userId = httpContext.GetUserId();

    var user = await appDbContext.Users
      .Include(u => u.Institutions)
      .ThenInclude(i => i.Accounts)
      .FirstOrDefaultAsync(u => u.Id == userId);

    if (user is null)
    {
      return Results.Unauthorized();
    }

    var institutionDtos = user.Institutions
      .Select(InstitutionDto.From)
      .OrderBy(dto => dto.Name);

    return Results.Ok(Response.From(institutionDtos));
  }
}