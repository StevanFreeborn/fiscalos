namespace FiscalOS.Infra.Transactions.Plaid;

internal interface IPlaidRemovedTransactionHandler
{
  Task<int> HandleAsync(IEnumerable<RemovedTransaction> removed, CancellationToken cancellationToken);
}

internal sealed class PlaidRemovedTransactionHandler : IPlaidRemovedTransactionHandler
{
  private readonly AppDbContext _appDbContext;
  private readonly ILogger<PlaidRemovedTransactionHandler> _logger;

  private PlaidRemovedTransactionHandler(
    AppDbContext appDbContext,
    ILogger<PlaidRemovedTransactionHandler> logger
  )
  {
    _appDbContext = appDbContext;
    _logger = logger;
  }

  public static PlaidRemovedTransactionHandler From(
    AppDbContext appDbContext,
    ILogger<PlaidRemovedTransactionHandler> logger
  )
  {
    return new(appDbContext, logger);
  }

  public static PlaidRemovedTransactionHandler From(IServiceProvider serviceProvider)
  {
    return new(
      serviceProvider.GetRequiredService<AppDbContext>(),
      serviceProvider.GetRequiredService<ILogger<PlaidRemovedTransactionHandler>>()
    );
  }

  public async Task<int> HandleAsync(IEnumerable<RemovedTransaction> removed, CancellationToken cancellationToken)
  {
    var removedCount = 0;
    var removedIdsList = removed.Select(t => t.TransactionId);

    try
    {
      removedCount = await _appDbContext.Transactions
        .Include(t => t.Metadata)
        .Where(t => t.Metadata is PlaidTransactionMetadata && removedIdsList.Contains(((PlaidTransactionMetadata)t.Metadata).PlaidId))
        .ExecuteDeleteAsync(cancellationToken)
        .ConfigureAwait(false);
    }
    catch (Exception ex)
    {
      _logger.LogError(
        ex,
        "Failed to removed transactions {TransactionIds}",
        removedIdsList
      );
    }

    _logger.LogInformation("Removed {RemovedCount} transactions", removedCount);

    return removedCount;
  }
}