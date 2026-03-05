namespace FiscalOS.Infra.Tests.Integration;

public abstract class IntegrationTest : IAsyncLifetime
{
  private bool _isDisposed;
  private readonly FileSystem _fileSystem = new();
  protected AppDbContext AppDbContext { get; }

  protected IntegrationTest()
  {
    var options = Options.Create(new AppDbContextOptions()
    {
      DatabaseFilePath = $"{Guid.NewGuid()}.db",
    });

    AppDbContext = new(options, _fileSystem);
  }

  protected async Task<User> CreateTestUserAsync()
  {
    var user = User.From(
      "username",
      "hashedPassword",
      EncryptedDataKey.From("keyUsed", "encryptedKey")
    );

    var institutionMetadata = PlaidInstitutionMetadata.From(
      "plaidId",
      "plaidName",
      "encryptedAccessToken",
      "itemId"
    );
    var institution = Institution.From("institutionName", institutionMetadata);
    user.AddInstitution(institution);

    var accountMetadata = PlaidAccountMetadata.From("plaidId", "plaidName");
    var account = Account.From("some account", accountMetadata);
    user.AddAccount(account);
    institution.AddAccount(account);

    var transactionMetadata = PlaidTransactionMetadata.From("plaidId");
    var transaction = Transaction.From(
      "merchantName",
      "description",
      100,
      DateTimeOffset.UtcNow,
      transactionMetadata
    );
    user.AddTransaction(transaction);
    account.AddTransaction(transaction);

    await AppDbContext.AddAsync(user, TestContext.Current.CancellationToken);
    await AppDbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    return user;
  }

  public async ValueTask InitializeAsync()
  {
    await AppDbContext.Database.MigrateAsync();
  }

  public async ValueTask DisposeAsync()
  {
    await DisposeAsync(true);
    GC.SuppressFinalize(this);
  }

  protected virtual async Task DisposeAsync(bool disposing)
  {
    if (_isDisposed)
    {
      return;
    }

    if (disposing)
    {
      await AppDbContext.DisposeAsync();
    }

    _isDisposed = true;
  }
}