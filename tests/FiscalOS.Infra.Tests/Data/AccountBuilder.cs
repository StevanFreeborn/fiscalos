namespace FiscalOS.Infra.Tests.Data;

internal sealed class AccountBuilder
{
  private string _name = "accountName";
  private AccountMetadata? _metadata;

  private AccountBuilder()
  {
  }

  public static AccountBuilder Create()
  {
    return new();
  }

  public AccountBuilder WithName(string name)
  {
    _name = name;
    return this;
  }

  public AccountBuilder WithMetadata(Action<AccountMetadataBuilder>? action = null)
  {
    var imb = AccountMetadataBuilder.Create();
    action?.Invoke(imb);
    _metadata = imb.Build();
    return this;
  }

  public Account Build()
  {
    if (_metadata is null)
    {
      throw new InvalidOperationException($"You must call {nameof(WithMetadata)} prior to building");
    }

    return Account.From(_name, _metadata);
  }
}