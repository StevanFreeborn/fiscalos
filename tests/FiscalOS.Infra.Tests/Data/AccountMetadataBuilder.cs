namespace FiscalOS.Infra.Tests.Data;

internal sealed class AccountMetadataBuilder
{
  private string _plaidId = "plaidId";
  private string _plaidName = "plaidName";

  private AccountMetadataBuilder()
  {
  }

  public static AccountMetadataBuilder Create()
  {
    return new();
  }

  public AccountMetadataBuilder WithPlaidId(string plaidId)
  {
    _plaidId = plaidId;
    return this;
  }

  public AccountMetadataBuilder WithPlaidName(string plaidName)
  {
    _plaidName = plaidName;
    return this;
  }

  public PlaidAccountMetadata Build()
  {
    return PlaidAccountMetadata.From(_plaidId, _plaidName);
  }
}