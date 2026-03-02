using FiscalOS.Infra.Common;

namespace FiscalOS.Infra.Accounts.Plaid;

public sealed class PlaidInstitutionMetadata : InstitutionMetadata
{
  public const string TypeValue = Providers.Plaid;
  public string PlaidId { get; init; } = string.Empty;
  public string PlaidName { get; init; } = string.Empty;
  public string EncryptedAccessToken { get; init; } = string.Empty;
  public string ItemId { get; init; } = string.Empty;

  private PlaidInstitutionMetadata() : base(TypeValue)
  {
  }

  public static PlaidInstitutionMetadata From(
    string? plaidId,
    string? plaidName,
    string encryptedAccessToken,
    string itemId
  )
  {
    ArgumentNullException.ThrowIfNull(plaidId, nameof(plaidId));
    ArgumentNullException.ThrowIfNull(plaidName, nameof(plaidName));
    ArgumentNullException.ThrowIfNull(encryptedAccessToken, nameof(encryptedAccessToken));
    ArgumentNullException.ThrowIfNull(itemId, nameof(itemId));

    return new()
    {
      PlaidId = plaidId,
      PlaidName = plaidName,
      EncryptedAccessToken = encryptedAccessToken,
      ItemId = itemId,
    };
  }
}