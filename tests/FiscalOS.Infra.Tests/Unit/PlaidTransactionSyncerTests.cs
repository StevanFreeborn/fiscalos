namespace FiscalOS.Infra.Tests.Unit;

public class PlaidTransactionSyncerTests
{
  private readonly Mock<ILogger<PlaidTransactionSyncer>> _mockSyncer = new();
  private 
  private readonly PlaidTransactionSyncer _sut;

  public PlaidTransactionSyncer()
  {
    _sut = PlaidTransactionSyncer.From();
  }
}
