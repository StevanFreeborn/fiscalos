namespace FiscalOS.Infra.Tests.Unit;

public class PlaidTransactionProcessorTests
{
  private readonly Mock<IPlaidAddedTransactionHandler> _mockAddedHandler = new();
  private readonly Mock<IPlaidModifiedTransactionHandler> _mockModifiedHandler = new();
  private readonly Mock<IPlaidRemovedTransactionHandler> _mockRemovedHandler = new();
  private readonly Mock<ILogger<PlaidTransactionProcessor>> _mockLogger = new();
  private readonly PlaidTransactionProcessor _sut;

  public PlaidTransactionProcessorTests()
  {
    _sut = PlaidTransactionProcessor.From(
      _mockAddedHandler.Object,
      _mockModifiedHandler.Object,
      _mockRemovedHandler.Object,
      _mockLogger.Object
    );
  }

  [Theory]
  [InlineData(0, 0, 0, false)]
  [InlineData(1, 1, 1, true)]
  [InlineData(0, 1, 1, false)]
  [InlineData(1, 0, 1, false)]
  [InlineData(1, 1, 0, true)]
  public async Task ProcessAsync_WhenCalledAndTransactionsAreHandled_ItShouldReturnCorrectResult(
    int numAdded,
    int numModified,
    int numRemoved,
    bool expectedResult
  )
  {
    _mockAddedHandler
      .Setup(
        static m => m.HandleAsync(
          It.IsAny<Account>(),
          It.IsAny<IEnumerable<Going.Plaid.Entity.Transaction>>(),
          It.IsAny<CancellationToken>()
        )
      )
      .ReturnsAsync(numAdded);

    _mockModifiedHandler
      .Setup(
        static m => m.HandleAsync(
          It.IsAny<Account>(),
          It.IsAny<IEnumerable<Going.Plaid.Entity.Transaction>>(),
          It.IsAny<CancellationToken>()
        )
      )
      .ReturnsAsync(numModified);

    _mockRemovedHandler
      .Setup(
        static m => m.HandleAsync(
          It.IsAny<IEnumerable<Going.Plaid.Entity.RemovedTransaction>>(),
          It.IsAny<CancellationToken>()
        )
      )
      .ReturnsAsync(numRemoved);

    var metadata = PlaidAccountMetadata.From("plaidId", "plaidName");
    var account = Account.From("name", metadata);
    var syncResponse = new TransactionsSyncResponse()
    {
      Added = [new()],
      Modified = [new()],
      Removed = [new()],
    };

    var result = await _sut.ProcessAsync(account, syncResponse, TestContext.Current.CancellationToken);

    result.Should().Be(expectedResult);
  }
}