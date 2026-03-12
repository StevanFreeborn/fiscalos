using Account = FiscalOS.Core.Accounts.Account;
using Transaction = FiscalOS.Core.Transactions.Transaction;

namespace FiscalOS.Infra.Transactions.Plaid;

internal interface IPlaidAddedTransactionHandler
{
  Task<int> HandleAsync(
    Account account,
    IEnumerable<Going.Plaid.Entity.Transaction> added,
    CancellationToken ct
  );
}

internal sealed class PlaidAddedTransactionHandler : IPlaidAddedTransactionHandler
{
  private readonly ILogger<PlaidAddedTransactionHandler> _logger;
  private readonly AppDbContext _appDbContext;

  private PlaidAddedTransactionHandler(
    ILogger<PlaidAddedTransactionHandler> logger,
    AppDbContext appDbContext
  )
  {
    _logger = logger;
    _appDbContext = appDbContext;
  }

  public static PlaidAddedTransactionHandler From(
    ILogger<PlaidAddedTransactionHandler> logger,
    AppDbContext appDbContext
  )
  {
    return new(logger, appDbContext);
  }

  public static PlaidAddedTransactionHandler From(IServiceProvider serviceProvider)
  {
    return new(
      serviceProvider.GetRequiredService<ILogger<PlaidAddedTransactionHandler>>(),
      serviceProvider.GetRequiredService<AppDbContext>()
    );
  }

  public async Task<int> HandleAsync(Account account, IEnumerable<Going.Plaid.Entity.Transaction> added, CancellationToken ct)
  {
    var existingTransactionIds = added.Select(t => t.TransactionId);
    var existingTransactions = await _appDbContext.Transactions
      .Include(t => t.Metadata)
      .Where(t => t.Metadata is PlaidTransactionMetadata && existingTransactionIds.Contains(((PlaidTransactionMetadata)t.Metadata).PlaidId))
      .ToListAsync(ct)
      .ConfigureAwait(false);

    var addedCount = 0;

    foreach (var addedTransaction in added)
    {
      try
      {
        if (addedTransaction.Pending.GetValueOrDefault())
        {
          _logger.LogInformation(
            "Skipping pending transaction {TransactionId} for account {AccountId} as it has not been posted yet",
            addedTransaction.TransactionId,
            account.Id
          );
          addedCount++;
          continue;
        }

        var existingTransaction = existingTransactions.FirstOrDefault(
          t => t.Metadata is PlaidTransactionMetadata metadata && metadata.PlaidId == addedTransaction.TransactionId
        );

        if (existingTransaction is not null)
        {
          _logger.LogInformation(
            "Skipping added transaction {PlaidTransactionId} for account {AccountId} as it has already been added as transaction {TransactionId}",
            addedTransaction.TransactionId,
            account.Id,
            existingTransaction.Id
          );
          addedCount++;
          continue;
        }

        var transactionMetadata = PlaidTransactionMetadata.From(addedTransaction.TransactionId);
        var transaction = Transaction.From(
          account.UserId,
          account.Id,
          addedTransaction.Merchant,
          addedTransaction.Description,
          addedTransaction.CanonicalAmount,
          addedTransaction.PostedDate,
          transactionMetadata
        );
        account.AddTransaction(transaction);
        addedCount++;
      }
      catch (Exception ex)
      {
        _logger.LogError(
          ex,
          "Failed to added transaction {PlaidTransactionId} to account {AccountId}",
          account.Id,
          addedTransaction.TransactionId
        );
      }
    }

    _logger.LogInformation("Added {AddedCount} transactions to account {AccountId}", addedCount, account.Id);

    return addedCount;
  }
}