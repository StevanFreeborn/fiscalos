using Account = FiscalOS.Core.Accounts.Account;
using Transaction = FiscalOS.Core.Transactions.Transaction;

namespace FiscalOS.Infra.Transactions.Plaid;

internal interface IPlaidAddedTransactionHandler
{
  void Handle(Account account, IEnumerable<Going.Plaid.Entity.Transaction> added);
}

internal sealed class PlaidAddedTransactionHandler : IPlaidAddedTransactionHandler
{
  private readonly ILogger<PlaidAddedTransactionHandler> _logger;

  private PlaidAddedTransactionHandler(ILogger<PlaidAddedTransactionHandler> logger)
  {
    _logger = logger;
  }

  public static PlaidAddedTransactionHandler From(IServiceProvider serviceProvider)
  {
    return new(
      serviceProvider.GetRequiredService<ILogger<PlaidAddedTransactionHandler>>()
    );
  }

  public void Handle(Account account, IEnumerable<Going.Plaid.Entity.Transaction> added)
  {
    foreach (var addedTransaction in added)
    {
      if (addedTransaction.Pending.GetValueOrDefault())
      {
        _logger.LogInformation(
          "Skipping pending transaction {TransactionId} for account {AccountId} as it has not been posted yet",
          addedTransaction.TransactionId,
          account.Id
        );
        continue;
      }

      var transactionMetadata = PlaidTransactionMetadata.From(addedTransaction.TransactionId);
      var transaction = Transaction.From(
        account.UserId,
        account.Id,
        addedTransaction.MerchantName,
        addedTransaction.Description,
        addedTransaction.Amount,
        addedTransaction.PostedDate,
        transactionMetadata
      );
      account.AddTransaction(transaction);
    }
  }
}