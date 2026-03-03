namespace FiscalOS.Infra.Tests.Integration;

public class PlaidRemovedTransactionHandlerTests : IntegrationTest
{
  private readonly Mock<ILogger<PlaidRemovedTransactionHandler>> _mockLogger = new();
  private readonly PlaidRemovedTransactionHandler _sut;

  public PlaidRemovedTransactionHandlerTests()
  {
    _sut = PlaidRemovedTransactionHandler.From(AppDbContext, _mockLogger.Object);
  }

  [Fact]
  public async Task HandleAsync_WhenCalled_ItShouldRemoveTheCorrectTransactions()
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

    var transactionToRemove = new Going.Plaid.Entity.RemovedTransaction()
    {
      TransactionId = transactionMetadata.PlaidId,
    };

    var result = await _sut.HandleAsync([transactionToRemove], TestContext.Current.CancellationToken);

    result.Should().Be(1);
  }
}