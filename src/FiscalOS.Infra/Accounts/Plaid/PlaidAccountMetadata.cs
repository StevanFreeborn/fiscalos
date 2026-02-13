namespace FiscalOS.Infra.Accounts.Plaid;

public sealed class PlaidAccountMetadata : AccountMetadata
{
  public const string TypeValue = Providers.Plaid;
  public string PlaidId { get; init; } = string.Empty;
  public string PlaidName { get; init; } = string.Empty;

  private PlaidAccountMetadata() : base(TypeValue)
  {
  }

  public static PlaidAccountMetadata From(string plaidId, string plaidName)
  {
    return new()
    {
      PlaidId = plaidId,
      PlaidName = plaidName,
    };
  }
}