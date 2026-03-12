using Account = FiscalOS.Core.Accounts.Account;
using Transaction = FiscalOS.Core.Transactions.Transaction;

namespace FiscalOS.Infra.Transactions.Plaid;

internal interface IPlaidModifiedTransactionHandler
{
  Task<int> HandleAsync(
    Account account,
    IEnumerable<Going.Plaid.Entity.Transaction> modified,
    CancellationToken ct
  );
}

internal sealed class PlaidModifiedTransactionHandler : IPlaidModifiedTransactionHandler
{
  private readonly ILogger<PlaidModifiedTransactionHandler> _logger;
  private readonly AppDbContext _appDbContext;

  private PlaidModifiedTransactionHandler(
    ILogger<PlaidModifiedTransactionHandler> logger,
    AppDbContext appDbContext
  )
  {
    _logger = logger;
    _appDbContext = appDbContext;
  }

  public static PlaidModifiedTransactionHandler From(
    ILogger<PlaidModifiedTransactionHandler> logger,
    AppDbContext appDbContext
  )
  {
    return new(logger, appDbContext);
  }

  public static PlaidModifiedTransactionHandler From(IServiceProvider serviceProvider)
  {
    return new(
      serviceProvider.GetRequiredService<ILogger<PlaidModifiedTransactionHandler>>(),
      serviceProvider.GetRequiredService<AppDbContext>()
    );
  }

  public async Task<int> HandleAsync(Account account, IEnumerable<Going.Plaid.Entity.Transaction> modified, CancellationToken ct)
  {
    var modifiedTransactionIds = modified.Select(t => t.TransactionId);
    var existingModifiedTransactions = await _appDbContext.Transactions
      .Where(t => t.Metadata is PlaidTransactionMetadata && modifiedTransactionIds.Contains(((PlaidTransactionMetadata)t.Metadata).PlaidId))
      .ToListAsync(ct)
      .ConfigureAwait(false);

    var modifiedCount = 0;

    foreach (var existing in existingModifiedTransactions)
    {
      try
      {
        if (existing.Metadata is not PlaidTransactionMetadata plaidMetadata)
        {
          _logger.LogWarning(
            "Existing transaction {TransactionId} has non-Plaid metadata. Skipping update for this transaction.",
            existing.Id
          );
          modifiedCount++;
          continue;
        }

        var plaidModifiedTransaction = modified.FirstOrDefault(t => t.TransactionId == plaidMetadata.PlaidId);

        if (plaidModifiedTransaction is null)
        {
          _logger.LogWarning(
            "No corresponding modified transaction found in Plaid response for existing transaction {TransactionId}. Skipping update for this transaction.",
            existing.Id
          );
          modifiedCount++;
          continue;
        }

        var newTransactionData = Transaction.From(
          existing.Id,
          existing.UserId,
          existing.AccountId,
          plaidModifiedTransaction.Merchant,
          plaidModifiedTransaction.Description,
          plaidModifiedTransaction.CanonicalAmount,
          plaidModifiedTransaction.PostedDate,
          plaidMetadata
        );

        _appDbContext.Entry(existing).CurrentValues.SetValues(newTransactionData);
        modifiedCount++;
      }
      catch (Exception ex)
      {
        _logger.LogError(
          ex,
          "Failed to update transaction {TransactionId} for account {AccountId}",
          account.Id,
          existing.Id
        );
      }
    }

    _logger.LogInformation("Modified {ModifiedCount} transactions for account {AccountId}", modifiedCount, account.Id);

    return modifiedCount;
  }
}