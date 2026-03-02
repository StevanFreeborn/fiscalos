namespace FiscalOS.Infra.Transactions.Plaid;

internal sealed class SyncUpdatesProcessor : IAsyncQueueProcessor<SyncUpdatesQueueItem>
{
  private readonly ILogger<SyncUpdatesProcessor> _logger;
  private readonly AppDbContext _appDbContext;
  private readonly IEncryptor _encryptor;
  private readonly IPlaidTransactionSyncer _plaidTransactionSyncer;

  private SyncUpdatesProcessor(
    ILogger<SyncUpdatesProcessor> logger,
    AppDbContext appDbContext,
    IEncryptor encryptor,
    IPlaidTransactionSyncer plaidTransactionSyncer
  )
  {
    _logger = logger;
    _appDbContext = appDbContext;
    _encryptor = encryptor;
    _plaidTransactionSyncer = plaidTransactionSyncer;
  }

  public static SyncUpdatesProcessor From(IServiceProvider serviceProvider)
  {
    return new(
      serviceProvider.GetRequiredService<ILogger<SyncUpdatesProcessor>>(),
      serviceProvider.GetRequiredService<AppDbContext>(),
      serviceProvider.GetRequiredService<IEncryptor>(),
      serviceProvider.GetRequiredService<IPlaidTransactionSyncer>()
    );
  }

  public async Task ProcessAsync(SyncUpdatesQueueItem item, CancellationToken cancellationToken = default)
  {
    _logger.LogInformation("Processing sync updates for item {ItemId}", item.InstitutionItemId);

    var institution = await _appDbContext.Institutions
      .Include(i => i.User)
      .Include(i => i.Accounts)
      .ThenInclude(a => a.Metadata)
      .Include(i => i.Metadata)
      .Where(i => i.Metadata is PlaidInstitutionMetadata && ((PlaidInstitutionMetadata)i.Metadata).ItemId == item.InstitutionItemId)
      .AsSplitQuery()
      .FirstOrDefaultAsync(cancellationToken)
      .ConfigureAwait(false);

    if (institution is null)
    {
      _logger.LogWarning(
        "Unable to process sync updates for item {ItemId} as no institution with matching Plaid item ID was found.",
        item.InstitutionItemId
      );
      return;
    }

    if (institution.User is null)
    {
      _logger.LogWarning(
        "Unable to process sync updates for item {ItemId} as the associated institution {InstitutionId} does not have a user.",
        item.InstitutionItemId,
        institution.Id
      );
      return;
    }

    if (institution.Metadata is not PlaidInstitutionMetadata plaidMetadata)
    {
      _logger.LogWarning(
        "Unable to process sync updates for item {ItemId} as the associated institution {InstitutionId} does not have Plaid metadata.",
        item.InstitutionItemId,
        institution.Id
      );
      return;
    }

    var decryptedAccessToken = await _encryptor.DecryptAsyncFor(
        institution.User!,
        plaidMetadata.EncryptedAccessToken,
        cancellationToken
      )
      .ConfigureAwait(false);

    foreach (var account in institution.Accounts)
    {
      await _plaidTransactionSyncer.SyncTransactionsForAccountAsync(account, decryptedAccessToken, cancellationToken)
        .ConfigureAwait(false);
    }

    await _appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    _logger.LogInformation("Successfully processed sync updates for item {ItemId}", item.InstitutionItemId);
  }
}