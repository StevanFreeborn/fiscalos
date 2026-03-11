namespace FiscalOS.Infra.Tests.Unit;

public class PlaidTransactionSyncerTests
{
  private readonly Mock<ILogger<PlaidTransactionSyncer>> _mockLogger = new();
  private readonly Mock<IPlaidTransactionService> _mockPlaidTransactionService = new();
  private readonly Mock<IPlaidTransactionProcessor> _mockPlaidTransactionProcessor = new();
  private readonly PlaidTransactionSyncer _sut;

  public PlaidTransactionSyncerTests()
  {
    _sut = PlaidTransactionSyncer.From(
      _mockLogger.Object,
      _mockPlaidTransactionService.Object,
      _mockPlaidTransactionProcessor.Object
    );
  }

  [Fact]
  public async Task SyncTransactionsForAccountAsync_WhenAccountHasNoPlaidMetadata_ItShouldNotCallService()
  {
    var account = AccountBuilder.Create()
      .WithName("no-plaid")
      .WithMetadata(new DummyMetadata())
      .Build();

    await _sut.SyncTransactionsForAccountAsync(account, "token", CancellationToken.None);

    _mockPlaidTransactionService.Verify(
      s => s.SyncTransactionsForAccountAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()),
      Times.Never()
    );

    _mockPlaidTransactionProcessor.Verify(
      p => p.ProcessAsync(It.IsAny<Account>(), It.IsAny<TransactionsSyncResponse>(), It.IsAny<CancellationToken>()),
      Times.Never()
    );
  }

  [Fact]
  public async Task SyncTransactionsForAccountAsync_WhenResponseIsUnsuccessful_ItShouldStopProcessing()
  {
    var account = AccountBuilder.Create()
      .WithName("acct")
      .WithMetadata(amb =>
      {
        amb.WithPlaidId("plaid-1")
          .WithPlaidName("Plaid One");
      })
      .Build();

    var response = new TransactionsSyncResponse()
    {
      RequestId = "req-1",
      Error = new() { ErrorMessage = "boom" }
    };

    SetStatusCode(response, HttpStatusCode.InternalServerError);

    _mockPlaidTransactionService
      .Setup(s => s.SyncTransactionsForAccountAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
      .Returns(AsyncResponses(response));

    await _sut.SyncTransactionsForAccountAsync(account, "token", CancellationToken.None);

    _mockPlaidTransactionProcessor.Verify(
      p => p.ProcessAsync(It.IsAny<Account>(), It.IsAny<TransactionsSyncResponse>(), It.IsAny<CancellationToken>()),
      Times.Never
    );
  }

  [Fact]
  public async Task SyncTransactionsForAccountAsync_WhenResponseHasNoAccounts_ItShouldStopProcessing()
  {
    var account = AccountBuilder.Create()
      .WithName("acct")
      .WithMetadata(amb =>
      {
        amb.WithPlaidId("plaid-2")
          .WithPlaidName("Plaid Two");
      })
      .Build();

    var response = new TransactionsSyncResponse()
    {
      RequestId = "req-2",
      Accounts = []
    };
    SetStatusCode(response, HttpStatusCode.OK);

    _mockPlaidTransactionService
      .Setup(s => s.SyncTransactionsForAccountAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
      .Returns(AsyncResponses(response));

    await _sut.SyncTransactionsForAccountAsync(account, "token", CancellationToken.None);

    _mockPlaidTransactionProcessor.Verify(
      p => p.ProcessAsync(It.IsAny<Account>(), It.IsAny<TransactionsSyncResponse>(), It.IsAny<CancellationToken>()),
      Times.Never
    );
  }

  [Fact]
  public async Task SyncTransactionsForAccountAsync_WhenResponseMissingMatchingAccount_ItShouldStopProcessing()
  {
    var account = AccountBuilder.Create()
      .WithName("acct")
      .WithMetadata(amb =>
      {
        amb.WithPlaidId("plaid-3")
          .WithPlaidName("Plaid Three");
      })
      .Build();

    var response = new TransactionsSyncResponse()
    {
      RequestId = "req-3",
      Accounts = [new() { AccountId = "other" }]
    };
    SetStatusCode(response, HttpStatusCode.OK);

    _mockPlaidTransactionService
      .Setup(s => s.SyncTransactionsForAccountAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
      .Returns(AsyncResponses(response));

    await _sut.SyncTransactionsForAccountAsync(account, "token", CancellationToken.None);

    _mockPlaidTransactionProcessor.Verify(
      p => p.ProcessAsync(It.IsAny<Account>(), It.IsAny<TransactionsSyncResponse>(), It.IsAny<CancellationToken>()),
      Times.Never
    );
  }

  [Fact]
  public async Task SyncTransactionsForAccountAsync_WhenProcessorReturnsFalse_ItShouldLeaveCursorUnchangedButAddsBalance()
  {
    var account = AccountBuilder.Create()
      .WithName("acct")
      .WithMetadata(amb =>
      {
        amb.WithPlaidId("plaid-4")
          .WithPlaidName("Plaid Four");
      })
      .Build();

    var response = new TransactionsSyncResponse()
    {
      RequestId = "req-4",
      Accounts = [
        new()
        {
          AccountId = "plaid-4",
          Balances = new() { Current = 123.45m, Available = 100.00m, IsoCurrencyCode = "USD" }
        }
      ],
      NextCursor = "next-1"
    };
    SetStatusCode(response, HttpStatusCode.OK);

    _mockPlaidTransactionService
      .Setup(s => s.SyncTransactionsForAccountAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
      .Returns(AsyncResponses(response));

    _mockPlaidTransactionProcessor
      .Setup(p => p.ProcessAsync(It.IsAny<Account>(), It.IsAny<TransactionsSyncResponse>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(false);

    await _sut.SyncTransactionsForAccountAsync(account, "token", CancellationToken.None);

    account.Balances.Should().HaveCount(1);
    ((PlaidAccountMetadata)account.Metadata!).Cursor.Should().BeNull();
  }

  [Fact]
  public async Task SyncTransactionsForAccountAsync_WhenProcessorSucceeds_ItShouldUpdateCursorAndAddsBalance()
  {
    var account = AccountBuilder.Create()
      .WithName("acct")
      .WithMetadata(amb =>
      {
        amb.WithPlaidId("plaid-5")
          .WithPlaidName("Plaid Five");
      })
      .Build();

    var response = new TransactionsSyncResponse()
    {
      RequestId = "req-5",
      Accounts = [
        new()
        {
          AccountId = "plaid-5",
          Balances = new() { Current = 200m, Available = 150m, IsoCurrencyCode = "EUR" }
        }
      ],
      NextCursor = "cursor-5"
    };
    SetStatusCode(response, HttpStatusCode.OK);

    _mockPlaidTransactionService
      .Setup(s => s.SyncTransactionsForAccountAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
      .Returns(AsyncResponses(response));

    _mockPlaidTransactionProcessor
      .Setup(p => p.ProcessAsync(It.IsAny<Account>(), It.IsAny<TransactionsSyncResponse>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(true);

    await _sut.SyncTransactionsForAccountAsync(account, "token", CancellationToken.None);

    account.Balances.Should().HaveCount(1);
    var balance = account.Balances.Should().ContainSingle().Which;
    balance.Current.Should().Be(200m);
    balance.Available.Should().Be(150m);
    balance.CurrencyCode.Should().Be("EUR");
    ((PlaidAccountMetadata)account.Metadata!).Cursor.Should().Be("cursor-5");
  }

  [Fact]
  public async Task SyncTransactionsForAccountAsync_WhenMultipleSuccessfulResponses_ItShouldProcessAllAndUseLastCursor()
  {
    var account = AccountBuilder.Create()
      .WithName("acct")
      .WithMetadata(amb =>
      {
        amb.WithPlaidId("plaid-6")
          .WithPlaidName("Plaid Six");
      })
      .Build();

    var r1 = new TransactionsSyncResponse()
    {
      RequestId = "req-6-1",
      Accounts = [
        new()
        {
          AccountId = "plaid-6",
          Balances = new() { Current = 10m, Available = 10m, IsoCurrencyCode = "USD" }
        }
      ],
      NextCursor = "cursor-6-1"
    };
    SetStatusCode(r1, HttpStatusCode.OK);

    var r2 = new TransactionsSyncResponse()
    {
      RequestId = "req-6-2",
      Accounts = [
        new()
        {
          AccountId = "plaid-6",
          Balances = new() { Current = 20m, Available = 20m, IsoCurrencyCode = "USD" }
        }
      ],
      NextCursor = "cursor-6-2"
    };
    SetStatusCode(r2, HttpStatusCode.OK);

    _mockPlaidTransactionService
      .Setup(s => s.SyncTransactionsForAccountAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
      .Returns(AsyncResponses(r1, r2));

    _mockPlaidTransactionProcessor
      .Setup(p => p.ProcessAsync(It.IsAny<Account>(), It.IsAny<TransactionsSyncResponse>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(true);

    await _sut.SyncTransactionsForAccountAsync(account, "token", CancellationToken.None);

    account.Balances.Should().HaveCount(2);
    ((PlaidAccountMetadata)account.Metadata!).Cursor.Should().Be("cursor-6-2");
    _mockPlaidTransactionProcessor.Verify(p => p.ProcessAsync(account, It.IsAny<TransactionsSyncResponse>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
  }

  private static void SetStatusCode(ResponseBase response, HttpStatusCode statusCode) =>
    typeof(ResponseBase)
      .GetProperty(nameof(ResponseBase.StatusCode))!
      .GetSetMethod(nonPublic: true)!
      .Invoke(response, [statusCode]);

  private static async IAsyncEnumerable<TransactionsSyncResponse> AsyncResponses(params TransactionsSyncResponse[] responses)
  {
    foreach (var r in responses)
    {
      yield return r;
    }
  }

  private sealed class DummyMetadata : AccountMetadata
  {
    public DummyMetadata() : base("dummy")
    {
    }
  }
}