using Account = FiscalOS.Core.Accounts.Account;

namespace FiscalOS.Infra.Transactions.Plaid;

internal interface IPlaidTransactionProcessor
{
  Task<bool> ProcessAsync(
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

  internal static PlaidTransactionProcessor From(
    IPlaidAddedTransactionHandler addedHandler,
    IPlaidModifiedTransactionHandler modifiedHandler,
    IPlaidRemovedTransactionHandler removedHandler
  )
  {
    return new(addedHandler, modifiedHandler, removedHandler);
  }

  internal static PlaidTransactionProcessor From(IServiceProvider provider)
  {
    return new(
      provider.GetRequiredService<IPlaidAddedTransactionHandler>(),
      provider.GetRequiredService<IPlaidModifiedTransactionHandler>(),
      provider.GetRequiredService<IPlaidRemovedTransactionHandler>()
    );
  }

  public async Task<bool> ProcessAsync(
    Account account,
    TransactionsSyncResponse syncResponse,
    CancellationToken cancellationToken
  )
  {
    var numAdded = await _addedHandler.HandleAsync(
      account,
      syncResponse.Added,
      cancellationToken
    ).ConfigureAwait(false);

    var numModified = await _modifiedHandler.HandleAsync(
      account,
      syncResponse.Modified,
      cancellationToken
    ).ConfigureAwait(false);

    var numRemoved = await _removedHandler.HandleAsync(
      syncResponse.Removed,
      cancellationToken
    ).ConfigureAwait(false);

    return numAdded == syncResponse.Added.Count &&
      numModified == syncResponse.Modified.Count &&
      numRemoved == syncResponse.Removed.Count;
  }
}