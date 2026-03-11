namespace FiscalOS.Tests.Common.Data;

public sealed class InstitutionMetadataBuilder
{
  private string _plaidId = "plaidId";
  private string _plaidName = "plaidName";
  private string _accessToken = "encryptedAccessToken";
  private string _itemId = "itemId";

  private InstitutionMetadataBuilder()
  {
  }

  public static InstitutionMetadataBuilder Create()
  {
    return new();
  }

  public InstitutionMetadataBuilder WithPlaidId(string plaidId)
  {
    _plaidId = plaidId;
    return this;
  }

  public InstitutionMetadataBuilder WithPlaidName(string plaidName)
  {
    _plaidName = plaidName;
    return this;
  }

  public InstitutionMetadataBuilder WithAccessToken(string accessToken)
  {
    _accessToken = accessToken;
    return this;
  }

  public InstitutionMetadataBuilder WithItemId(string itemId)
  {
    _itemId = itemId;
    return this;
  }

  public PlaidInstitutionMetadata Build()
  {
    return PlaidInstitutionMetadata.From(
      _plaidId,
      _plaidName,
      _accessToken,
      _itemId
    );
  }
}