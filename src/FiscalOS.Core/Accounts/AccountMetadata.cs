namespace FiscalOS.Core.Accounts;

public abstract class AccountMetadata : Entity
{
  private readonly string _type;

  public string Type => _type;

  public Guid AccountId { get; init; }

  protected AccountMetadata(string type)
  {
    _type = type;
  }
}