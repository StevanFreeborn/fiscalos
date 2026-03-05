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
    var user = await CreateTestUserAsync();
    var testTransaction = user.Transactions.First();

    var transactionToRemove = new Going.Plaid.Entity.RemovedTransaction()
    {
      TransactionId = ((PlaidTransactionMetadata)testTransaction.Metadata!).PlaidId,
    };

    var result = await _sut.HandleAsync([transactionToRemove], TestContext.Current.CancellationToken);

    result.Should().Be(1);
  }
}