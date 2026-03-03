namespace FiscalOS.Infra.Transactions.Plaid;

public sealed class PlaidTransactionMetadata : TransactionMetadata
{
  public const string TypeValue = Providers.Plaid;
  public string PlaidId { get; init; } = string.Empty;

  private PlaidTransactionMetadata() : base(TypeValue)
  {
  }

  public static PlaidTransactionMetadata From(string? plaidId)
  {
    ArgumentNullException.ThrowIfNull(plaidId, nameof(plaidId));

    return new()
    {
      PlaidId = plaidId,
    };
  }
}