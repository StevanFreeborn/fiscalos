using Account = FiscalOS.Core.Accounts.Account;

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
      .Include(u => u.Institutions.Where(i => i.Metadata is PlaidInstitutionMetadata && ((PlaidInstitutionMetadata)i.Metadata).PlaidId == request.ProviderInstitutionId))
      .ThenInclude(i => i.Metadata)
      .Include(u => u.Accounts.Where(a => a.Metadata is PlaidAccountMetadata && ((PlaidAccountMetadata)a.Metadata).PlaidId == request.ProviderAccountId))
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
        [nameof(Request.ProviderInstitutionId)] = [$"The {nameof(Request.ProviderInstitutionId)} field is invalid."],
      });
    }

    var plaidInstitutionMetadata = (PlaidInstitutionMetadata?)user.Institutions.First().Metadata;

    if (plaidInstitutionMetadata is null)
    {
      return Results.ValidationProblem(new Dictionary<string, string[]>
      {
        [nameof(Request.ProviderInstitutionId)] = [$"The {nameof(Request.ProviderInstitutionId)} field is invalid."],
      });
    }

    if (user.Accounts.Any())
    {
      return Results.Conflict();
    }

    var decryptedAccessToken = await encryptor.DecryptAsyncFor(user, plaidInstitutionMetadata.EncryptedAccessToken, ct);
    var accountMetadata = PlaidAccountMetadata.From(request.ProviderAccountId, request.ProviderAccountName);
    var account = Account.From(request.ProviderAccountName, accountMetadata);
    user.AddAccount(account);
    user.Institutions.First().AddAccount(account);

    await appDbContext.SaveChangesAsync(ct);
    await queue.EnqueueAsync(new(plaidInstitutionMetadata.ItemId), ct);

    return Results.Ok();
  }
}