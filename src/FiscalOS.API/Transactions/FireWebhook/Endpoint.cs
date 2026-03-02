using Going.Plaid;
using Going.Plaid.Entity;

namespace FiscalOS.API.Transactions.FireWebhook;

internal static class Endpoint
{
  private const string Route = "/fire-webhook";

  public static RouteHandlerBuilder MapFireWebhookEndpoint(this RouteGroupBuilder groupBuilder)
  {
    return groupBuilder.MapPost(Route, HandleAsync);
  }

  private static async Task<IResult> HandleAsync(
    HttpContext httpContext,
    [FromBody] Request request,
    [FromServices] AppDbContext appDbContext,
    [FromServices] IEncryptor encryptor,
    [FromServices] PlaidClient plaidClient,
    CancellationToken ct
  )
  {
    var userId = httpContext.GetUserId();

    var user = await appDbContext.Users
      .Include(u => u.Accounts.Where(a => a.Id.ToString() == request.AccountId))
      .ThenInclude(static a => a.Institution!)
      .ThenInclude(static i => i.Metadata)
      .FirstOrDefaultAsync(u => u.Id == userId, ct);

    if (user is null)
    {
      return Results.Unauthorized();
    }

    if (user.Accounts.Any() is false)
    {
      return Results.NotFound();
    }

    var plaidMetadata = (PlaidInstitutionMetadata?)user.Accounts.First().Institution?.Metadata;

    if (plaidMetadata is null)
    {
      return Results.InternalServerError("No metadata associated with account");
    }

    var accessToken = await encryptor.DecryptAsyncFor(user, plaidMetadata.EncryptedAccessToken, ct);

    var fireEventResponse = await plaidClient.SandboxItemFireWebhookAsync(new()
    {
      AccessToken = accessToken,
      WebhookType = SandboxItemFireWebhookRequestWebhookTypeEnum.Transactions,
      WebhookCode = SandboxItemFireWebhookRequestWebhookCodeEnum.SyncUpdatesAvailable,
    });

    if (fireEventResponse.IsSuccessStatusCode is false)
    {
      return Results.InternalServerError(fireEventResponse.Error?.ErrorMessage);
    }

    return Results.Ok(new { fireEventResponse.RequestId, fireEventResponse.WebhookFired });
  }
}