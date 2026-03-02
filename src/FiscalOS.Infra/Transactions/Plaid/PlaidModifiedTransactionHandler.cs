using Account = FiscalOS.Core.Accounts.Account;
using Transaction = FiscalOS.Core.Transactions.Transaction;

namespace FiscalOS.Infra.Transactions.Plaid;

internal interface IPlaidModifiedTransactionHandler
{
  void Handle(Account account, IEnumerable<Going.Plaid.Entity.Transaction> modified);
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

  public static PlaidModifiedTransactionHandler From(IServiceProvider serviceProvider)
  {
    return new(
      serviceProvider.GetRequiredService<ILogger<PlaidModifiedTransactionHandler>>(),
      serviceProvider.GetRequiredService<AppDbContext>()
    );
  }

  public void Handle(Account account, IEnumerable<Going.Plaid.Entity.Transaction> modified)
  {
    var modifiedTransactionIds = modified.Select(t => t.TransactionId);
    var existingModifiedTransactions = _appDbContext.Transactions
      .Where(t => t.Metadata is PlaidTransactionMetadata && modifiedTransactionIds.Contains(((PlaidTransactionMetadata)t.Metadata).PlaidId))
      .ToList();

    foreach (var existing in existingModifiedTransactions)
    {
      if (existing.Metadata is not PlaidTransactionMetadata plaidMetadata)
      {
        _logger.LogWarning(
          "Existing transaction {TransactionId} has non-Plaid metadata. Skipping update for this transaction.",
          existing.Id
        );
        continue;
      }

      var plaidModifiedTransaction = modified.FirstOrDefault(t => t.TransactionId == plaidMetadata.PlaidId);

      if (plaidModifiedTransaction is null)
      {
        _logger.LogWarning(
          "No corresponding modified transaction found in Plaid response for existing transaction {TransactionId}. Skipping update for this transaction.",
          existing.Id
        );
        continue;
      }

      var newTransactionData = Transaction.From(
        existing.UserId,
        existing.AccountId,
        plaidModifiedTransaction.MerchantName,
        plaidModifiedTransaction.Description,
        plaidModifiedTransaction.Amount,
        plaidModifiedTransaction.PostedDate,
        plaidMetadata
      );

      _appDbContext.Entry(existing).CurrentValues.SetValues(newTransactionData);
    }
  }
}