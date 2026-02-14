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
      .Include(u => u.Institutions.Where(i => i.Metadata is PlaidMetadata && ((PlaidMetadata)i.Metadata).PlaidId == request.PlaidInstitutionId))
      .Include(u => u.Accounts.Where(a => a.Metadata is PlaidAccountMetadata && ((PlaidAccountMetadata)a.Metadata).PlaidId == request.PlaidAccountId))
      .ThenInclude(a => a.Metadata)
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

    if (user.Accounts.Any())
    {
      return Results.Conflict();
    }

    var accountMetadata = PlaidAccountMetadata.From(request.PlaidAccountId, request.PlaidAccountName);
    var account = Account.From(user.Institutions.First().Id, request.PlaidAccountName, accountMetadata);
    var balance = Balance.From(request.AccountCurrentBalance, request.AccountAvailableBalance, request.AccountCurrencyCode);

    account.AddBalance(balance);
    user.AddAccount(account);

    await appDbContext.SaveChangesAsync(ct);

    return Results.Ok();
  }
}