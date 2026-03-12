namespace FiscalOS.Infra.Tests.Integration;

public class PlaidAddedTransactionHandlerTests : IntegrationTest
{
  private readonly Mock<ILogger<PlaidAddedTransactionHandler>> _mockLogger = new();
  private readonly PlaidAddedTransactionHandler _sut;

  public PlaidAddedTransactionHandlerTests()
  {
    _sut = PlaidAddedTransactionHandler.From(_mockLogger.Object, AppDbContext);
  }

  [Fact]
  public async Task HandleAsync_WhenCalledWithNewTransaction_ItShouldAddTheTransaction()
  {
    var user = await CreateTestUserAsync();
    var account = user.Accounts.First();

    var transaction = new Going.Plaid.Entity.Transaction()
    {
      TransactionId = Guid.NewGuid().ToString(),
      MerchantName = "Test Merchant",
      OriginalDescription = "Test transaction",
      Amount = 100,
      Datetime = DateTimeOffset.UtcNow,
      Pending = false,
    };

    var result = await _sut.HandleAsync(account, [transaction], TestContext.Current.CancellationToken);

    result.Should().Be(1);
    account.Transactions.Should().HaveCount(2);
    account.Transactions
      .Single(t => t.Metadata is PlaidTransactionMetadata m && m.PlaidId == transaction.TransactionId)
      .Amount.Should().Be(-transaction.Amount);
  }

  [Fact]
  public async Task HandleAsync_WhenCalledWithPendingTransaction_ItShouldSkipTheTransaction()
  {
    var user = await CreateTestUserAsync();
    var account = user.Accounts.First();

    var transaction = new Going.Plaid.Entity.Transaction()
    {
      TransactionId = Guid.NewGuid().ToString(),
      MerchantName = "Test Merchant",
      OriginalDescription = "Test transaction",
      Amount = 100,
      Datetime = DateTimeOffset.UtcNow,
      Pending = true,
    };

    var result = await _sut.HandleAsync(account, [transaction], TestContext.Current.CancellationToken);

    result.Should().Be(1);
    account.Transactions.Should().HaveCount(1);
  }

  [Fact]
  public async Task HandleAsync_WhenCalledWithExistingTransaction_ItShouldNotAddTheTransactionToTheDatabase()
  {
    var user = await CreateTestUserAsync();
    var account = user.Accounts.First();
    var existingTransaction = user.Transactions.First();

    var transaction = new Going.Plaid.Entity.Transaction()
    {
      TransactionId = ((PlaidTransactionMetadata)existingTransaction.Metadata!).PlaidId,
      MerchantName = "Test Merchant",
      OriginalDescription = "Test transaction",
      Amount = 100,
      Datetime = DateTimeOffset.UtcNow,
      Pending = false,
    };

    var result = await _sut.HandleAsync(account, [transaction], TestContext.Current.CancellationToken);

    result.Should().Be(1);
    account.Transactions.Should().HaveCount(1);
  }
}