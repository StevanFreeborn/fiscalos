namespace FiscalOS.API.Institutions.GetAvailable;

internal static class Endpoint
{
  private const string Route = "/{id}/available";

  public static RouteHandlerBuilder MapGetAvailableEndpoint(this RouteGroupBuilder groupBuilder)
  {
    return groupBuilder.MapGet(Route, HandleAsync);
  }

  private static async Task<IResult> HandleAsync(
    HttpContext httpContext,
    [FromRoute] Guid id,
    [FromServices] AppDbContext appDbContext,
    [FromServices] PlaidService plaidService,
    [FromServices] IEncryptor encryptor,
    CancellationToken ct
  )
  {
    var userId = httpContext.GetUserId();

    var user = await appDbContext.Users
      .Include(u => u.Institutions.Where(i => i.Id == id))
      .ThenInclude(i => i.Metadata)
      .SingleOrDefaultAsync(u => u.Id == userId, ct);

    if (user is null)
    {
      return Results.Unauthorized();
    }

    if (user.Institutions.Any() is false)
    {
      return Results.NotFound();
    }

    var institution = user.Institutions.First();

    if (institution.Metadata is not PlaidMetadata plaidMetadata)
    {
      return Results.BadRequest();
    }

    var accessToken = await encryptor.DecryptAsyncFor(user, plaidMetadata.EncryptedAccessToken, ct);
    var accounts = await plaidService.GetAccountsAsync(accessToken);
    var accountsDtos = accounts.Select(a => AvailableAccountDto.From(plaidMetadata, a));

    return Results.Ok(Response.From(accountsDtos));
  }
}