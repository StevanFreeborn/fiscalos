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
    [FromServices] IEncryptor encryptor,
    [FromServices] IPlaidAccountService plaidAccountService,
    [FromServices] IAsyncQueue<SyncUpdatesQueueItem> queue,
    CancellationToken ct
  )
  {
    var userId = httpContext.GetUserId();

    var user = await appDbContext.Users
      .Include(u => u.Institutions.Where(i => i.Metadata is PlaidInstitutionMetadata && ((PlaidInstitutionMetadata)i.Metadata).PlaidId == request.PlaidInstitutionId))
      .ThenInclude(i => i.Metadata)
      .Include(u => u.Accounts.Where(a => a.Metadata is PlaidAccountMetadata && ((PlaidAccountMetadata)a.Metadata).PlaidId == request.PlaidAccountId))
      .ThenInclude(a => a.Metadata)
      .AsSplitQuery()
      .SingleOrDefaultAsync(u => u.Id == userId, ct);

    if (user is null)
    {
      return Results.Unauthorized();
    }

    if (user.Institutions.Any() is false)
    {
      return Results.ValidationProblem(new Dictionary<string, string[]>
      {
        ["PlaidInstitutionId"] = ["The PlaidInstitutionId field is invalid. No institution connected with the given PlaidInstitutionId was found for the user."],
      });
    }

    var plaidInstitutionMetadata = (PlaidInstitutionMetadata?)user.Institutions.First().Metadata;

    if (plaidInstitutionMetadata is null)
    {
      return Results.ValidationProblem(new Dictionary<string, string[]>
      {
        ["PlaidInstitutionId"] = ["The PlaidInstitutionId field is invalid. The connected institution has no plaid metadata"],
      });
    }

    if (user.Accounts.Any())
    {
      return Results.Conflict();
    }

    var decryptedAccessToken = await encryptor.DecryptAsyncFor(user, plaidInstitutionMetadata.EncryptedAccessToken, ct);
    var accountMetadata = PlaidAccountMetadata.From(request.PlaidAccountId, request.PlaidAccountName);
    var account = Account.From(user.Institutions.First().Id, request.PlaidAccountName, accountMetadata);
    user.AddAccount(account);

    await appDbContext.SaveChangesAsync(ct);
    await queue.EnqueueAsync(new(plaidInstitutionMetadata.ItemId), ct);

    return Results.Ok();
  }
}