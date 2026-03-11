namespace FiscalOS.Tests.Common.Data;

public sealed class TransactionBuilder
{
  private string _merchantName = "merchantName";
  private string _description = "description";
  private decimal _amount = 100;
  private DateTimeOffset _date = DateTimeOffset.UtcNow;
  private TransactionMetadata? _metadata;

  private TransactionBuilder()
  {
  }

  public static TransactionBuilder Create()
  {
    return new();
  }

  public TransactionBuilder WithMerchantName(string merchantName)
  {
    _merchantName = merchantName;
    return this;
  }

  public TransactionBuilder WithDescription(string description)
  {
    _description = description;
    return this;
  }

  public TransactionBuilder WithAmount(decimal amount)
  {
    _amount = amount;
    return this;
  }

  public TransactionBuilder WithDate(DateTimeOffset date)
  {
    _date = date;
    return this;
  }

  public TransactionBuilder WithMetadata(Action<TransactionMetadataBuilder>? action = null)
  {
    var tmb = TransactionMetadataBuilder.Create();
    action?.Invoke(tmb);
    _metadata = tmb.Build();
    return this;
  }

  public Transaction Build()
  {
    if (_metadata is null)
    {
      throw new InvalidOperationException($"You must call {nameof(WithMetadata)} prior to building");
    }

    return Transaction.From(
      _merchantName,
      _description,
      _amount,
      _date,
      _metadata
    );
  }
}