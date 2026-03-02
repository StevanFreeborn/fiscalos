namespace FiscalOS.Infra.Transactions.Plaid;

internal interface IPlaidRemovedTransactionHandler
{
  Task HandleAsync(IEnumerable<RemovedTransaction> removed, CancellationToken cancellationToken);
}

internal sealed class PlaidRemovedTransactionHandler : IPlaidRemovedTransactionHandler
{
  private readonly AppDbContext _appDbContext;

  private PlaidRemovedTransactionHandler(AppDbContext appDbContext)
  {
    _appDbContext = appDbContext;
  }

  public static PlaidRemovedTransactionHandler From(IServiceProvider serviceProvider)
  {
    return new(
      serviceProvider.GetRequiredService<AppDbContext>()
    );
  }

  public async Task HandleAsync(IEnumerable<RemovedTransaction> removed, CancellationToken cancellationToken)
  {
    var removedIdsList = removed.Select(t => t.TransactionId);

    await _appDbContext.Transactions
      .Where(t => t.Metadata is PlaidTransactionMetadata && removedIdsList.Contains(((PlaidTransactionMetadata)t.Metadata).PlaidId))
      .ExecuteDeleteAsync(cancellationToken)
      .ConfigureAwait(false);
  }
}