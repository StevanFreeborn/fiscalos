namespace FiscalOS.Core.Accounts;

public sealed class Balance : Entity
{
  public Guid AccountId { get; init; }
  public decimal Current { get; init; }
  public decimal Available { get; init; }
  public string CurrencyCode { get; init; } = string.Empty;

  private Balance()
  {
  }

  public static Balance From(
    decimal current,
    decimal available,
    string currencyCode
  )
  {
    return new()
    {
      Current = current,
      Available = available,
      CurrencyCode = currencyCode,
    };
  }
}