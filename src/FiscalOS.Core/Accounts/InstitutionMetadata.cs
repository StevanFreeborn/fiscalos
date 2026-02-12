namespace FiscalOS.Core.Accounts;

public abstract class InstitutionMetadata : Entity
{
  private readonly string _type;

  public string Type => _type;

  public Guid InstitutionId { get; init; }

  protected InstitutionMetadata(string type)
  {
    _type = type;
  }
}