namespace FiscalOS.Tests.Common.Data;

public sealed class TransactionMetadataBuilder
{
  private string _plaidId = "plaidId";

  private TransactionMetadataBuilder()
  {
  }

  public static TransactionMetadataBuilder Create()
  {
    return new();
  }

  public TransactionMetadataBuilder WithPlaidId(string plaidId)
  {
    _plaidId = plaidId;
    return this;
  }

  public PlaidTransactionMetadata Build()
  {
    return PlaidTransactionMetadata.From(_plaidId);
  }
}