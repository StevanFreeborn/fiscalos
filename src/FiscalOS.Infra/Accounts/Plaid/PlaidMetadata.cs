namespace FiscalOS.Infra.Accounts.Plaid;

public sealed class PlaidMetadata : InstitutionMetadata
{
  public const string TypeValue = "Plaid";
  public string PlaidId { get; init; } = string.Empty;
  public string PlaidName { get; init; } = string.Empty;
  public string EncryptedAccessToken { get; init; } = string.Empty;

  private PlaidMetadata() : base(TypeValue)
  {
  }

  public static PlaidMetadata From(
    string? plaidId,
    string? plaidName,
    string encryptedAccessToken
  )
  {
    ArgumentNullException.ThrowIfNull(plaidId, nameof(plaidId));
    ArgumentNullException.ThrowIfNull(plaidName, nameof(plaidName));
    ArgumentNullException.ThrowIfNull(encryptedAccessToken, nameof(encryptedAccessToken));

    return new PlaidMetadata
    {
      PlaidId = plaidId,
      PlaidName = plaidName,
      EncryptedAccessToken = encryptedAccessToken
    };
  }
}