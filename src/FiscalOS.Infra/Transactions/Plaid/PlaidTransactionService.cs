namespace FiscalOS.Infra.Transactions.Plaid;

internal interface IPlaidTransactionService
{
  IAsyncEnumerable<TransactionsSyncResponse> SyncTransactionsForAccountAsync(
    string accessToken,
    string accountId,
    string? initialCursor = null
  );
}

internal sealed class PlaidTransactionService : IPlaidTransactionService
{
  private readonly PlaidClient _client;

  private PlaidTransactionService(PlaidClient client)
  {
    _client = client;
  }

  public static PlaidTransactionService From(IServiceProvider sp)
  {
    var client = sp.GetRequiredService<PlaidClient>();
    return new(client);
  }

  public async IAsyncEnumerable<TransactionsSyncResponse> SyncTransactionsForAccountAsync(
    string accessToken,
    string accountId,
    string? initialCursor = null
  )
  {
    TransactionsSyncResponse? response;
    var cursor = initialCursor;

    do
    {
      response = await _client.TransactionsSyncAsync(new()
      {
        AccessToken = accessToken,
        Cursor = cursor,
        Count = 500,
        Options = new()
        {
          AccountId = accountId,
        }
      }).ConfigureAwait(false);

      if (response.Error?.ErrorCode is "TRANSACTIONS_SYNC_MUTATION_DURING_PAGINATION")
      {
        continue;
      }

      cursor = response.NextCursor;
      yield return response;
    } while (response is not null && response.HasMore);
  }
}
