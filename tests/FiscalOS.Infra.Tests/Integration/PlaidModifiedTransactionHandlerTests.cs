namespace FiscalOS.Infra.Tests.Integration;

public class PlaidModifiedTransactionHandlerTests : IntegrationTest
{
  private readonly Mock<ILogger<PlaidModifiedTransactionHandler>> _mockLogger = new();
  private readonly PlaidModifiedTransactionHandler _sut;

  public PlaidModifiedTransactionHandlerTests()
  {
    _sut = PlaidModifiedTransactionHandler.From(_mockLogger.Object, AppDbContext);
  }

  [Fact]
  public async Task HandleAsync_WhenCalledWithExistingTransaction_ItShouldUpdateTheTransaction()
  {
    var user = await CreateTestUserAsync();
    var account = user.Accounts.First();
    var existingTransaction = account.Transactions.First();

    var transaction = new Going.Plaid.Entity.Transaction()
    {
      TransactionId = ((PlaidTransactionMetadata)existingTransaction.Metadata!).PlaidId,
      MerchantName = Guid.NewGuid().ToString(),
      OriginalDescription = "Test transaction",
      Amount = 100,
      Datetime = DateTimeOffset.UtcNow,
      Pending = false,
    };

    var result = await _sut.HandleAsync(account, [transaction], TestContext.Current.CancellationToken);

    result.Should().Be(1);
    existingTransaction.MerchantName.Should().Be(transaction.MerchantName);
    existingTransaction.Amount.Should().Be(-transaction.Amount);
  }
}