using FiscalOS.Core.Accounts;

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
    [FromServices] AppDbContext appDbContext,
    [FromServices] PlaidService plaidService,
    [FromServices] IEncryptor encryptor,
    CancellationToken ct
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
      .SingleOrDefaultAsync(u => u.Id == userId, ct);

    if (user is null)
    {
      return Results.Unauthorized();
    }

    if (user.Institutions.Any())
    {
      return Results.Conflict();
    }

    var (itemId, accessToken) = await plaidService.ExchangeTokenAsync(request.PublicToken);
    var item = await plaidService.GetItemAsync(accessToken);

    var encryptedAccessToken = await encryptor.EncryptAsyncFor(user, accessToken, ct);

    var plaidMetadata = PlaidMetadata.From(item.InstitutionId, item.InstitutionName, encryptedAccessToken);
    var institution = Institution.From(item.InstitutionName, plaidMetadata);

    user.AddInstitution(institution);

    await appDbContext.SaveChangesAsync(ct);

    return Results.Ok();
  }
}