using Account = FiscalOS.Core.Accounts.Account;

namespace FiscalOS.Infra.Transactions.Plaid;

internal interface IPlaidTransactionSyncer
{
  Task SyncTransactionsForAccountAsync(
    Account account,
    string decryptedAccessToken,
    CancellationToken cancellationToken
  );
}

internal sealed class PlaidTransactionSyncer : IPlaidTransactionSyncer
{
  private readonly ILogger<PlaidTransactionSyncer> _logger;
  private readonly IPlaidTransactionService _plaidTransactionService;
  private readonly IPlaidTransactionProcessor _transactionProcessor;

  private PlaidTransactionSyncer(
    ILogger<PlaidTransactionSyncer> logger,
    IPlaidTransactionService plaidTransactionService,
    IPlaidTransactionProcessor plaidTransactionProcessor
  )
  {
    _logger = logger;
    _plaidTransactionService = plaidTransactionService;
    _transactionProcessor = plaidTransactionProcessor;
  }

  public static PlaidTransactionSyncer From(IServiceProvider serviceProvider)
  {
    return new(
      serviceProvider.GetRequiredService<ILogger<PlaidTransactionSyncer>>(),
      serviceProvider.GetRequiredService<IPlaidTransactionService>(),
      serviceProvider.GetRequiredService<IPlaidTransactionProcessor>()
    );
  }

  public async Task SyncTransactionsForAccountAsync(Account account, string decryptedAccessToken, CancellationToken cancellationToken)
  {
    if (account.Metadata is not PlaidAccountMetadata plaidAccountMetadata)
    {
      _logger.LogWarning("Unable to sync transactions for account {AccountId} as it has no Plaid metadata", account.Id);
      return;
    }

    var responses = _plaidTransactionService.SyncTransactionsForAccountAsync(
      decryptedAccessToken,
      plaidAccountMetadata.PlaidId,
      plaidAccountMetadata.Cursor
    ).ConfigureAwait(false);

    await foreach (var response in responses)
    {
      if (response.IsSuccessStatusCode is false)
      {
        _logger.LogWarning(
          "Failed to sync transactions for request {RequestId} for account {AccountId}: {ErrorMessage}",
          response.RequestId,
          account.Id,
          response.Error?.ErrorMessage
        );
        break;
      }

      if (response.Accounts.Any() is false)
      {
        _logger.LogInformation("No transactions to sync for request {RequestId} for account {AccountId}", response.RequestId, account.Id);
        break;
      }

      var plaidAccount = response.Accounts
        .FirstOrDefault(pa => pa.AccountId == plaidAccountMetadata.PlaidId);

      if (plaidAccount is null)
      {
        _logger.LogWarning(
          "Failed to sync transactions for request {RequestId} for account {AccountId}: No corresponding account received in Plaid response",
          response.RequestId,
          account.Id
        );
        break;
      }

      var balance = Balance.From(plaidAccount.Current, plaidAccount.Available, plaidAccount.CurrencyCode);
      account.AddBalance(balance);

      var isProcessedSuccessfully = await _transactionProcessor.ProcessAsync(account, response, cancellationToken).ConfigureAwait(false);

      if (isProcessedSuccessfully is false)
      {
        _logger.LogWarning(
          "Unable to process transactions for request {RequestId} for account {AccountId} successfully",
          response.RequestId,
          account.Id
        );
        break;
      }

      plaidAccountMetadata.SetCursor(response.NextCursor);
    }
  }
}