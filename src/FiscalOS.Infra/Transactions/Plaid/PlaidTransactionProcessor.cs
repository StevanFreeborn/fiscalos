using Account = FiscalOS.Core.Accounts.Account;

namespace FiscalOS.Infra.Transactions.Plaid;

internal interface IPlaidTransactionProcessor
{
  Task ProcessAsync(
    Account account,
    TransactionsSyncResponse syncResponse,
    CancellationToken cancellationToken
  );
}

internal sealed class PlaidTransactionProcessor : IPlaidTransactionProcessor
{
  private readonly IPlaidAddedTransactionHandler _addedHandler;
  private readonly IPlaidModifiedTransactionHandler _modifiedHandler;
  private readonly IPlaidRemovedTransactionHandler _removedHandler;

  private PlaidTransactionProcessor(
    IPlaidAddedTransactionHandler addedHandler,
    IPlaidModifiedTransactionHandler modifiedHandler,
    IPlaidRemovedTransactionHandler removedHandler
  )
  {
    _addedHandler = addedHandler;
    _modifiedHandler = modifiedHandler;
    _removedHandler = removedHandler;
  }

  internal static PlaidTransactionProcessor From(IServiceProvider provider)
  {
    return new(
      provider.GetRequiredService<IPlaidAddedTransactionHandler>(),
      provider.GetRequiredService<IPlaidModifiedTransactionHandler>(),
      provider.GetRequiredService<IPlaidRemovedTransactionHandler>()
    );
  }

  public async Task ProcessAsync(Account account, TransactionsSyncResponse syncResponse, CancellationToken cancellationToken)
  {
    _addedHandler.Handle(account, syncResponse.Added);
    _modifiedHandler.Handle(account, syncResponse.Modified);
    await _removedHandler.HandleAsync(syncResponse.Removed, cancellationToken).ConfigureAwait(false);
  }
}