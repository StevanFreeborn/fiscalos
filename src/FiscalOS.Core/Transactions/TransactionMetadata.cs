namespace FiscalOS.Core.Transactions;

public abstract class TransactionMetadata : Entity
{
  private readonly string _type;

  public string Type => _type;

  public Guid TransactionId { get; init; }

  protected TransactionMetadata(string type)
  {
    _type = type;
  }
}