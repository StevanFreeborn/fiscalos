namespace FiscalOS.Tests.Common.Data;

public sealed class AccountBuilder
{
  private string _name = "accountName";
  private AccountMetadata? _metadata;
  private readonly List<Transaction> _transactions = [];

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

  public AccountBuilder WithMetadata(AccountMetadata metadata)
  {
    _metadata = metadata;
    return this;
  }

  public AccountBuilder WithTransaction(Action<TransactionBuilder>? action = null)
  {
    var tb = TransactionBuilder.Create();
    action?.Invoke(tb);
    _transactions.Add(tb.Build());
    return this;
  }

  public Account Build()
  {
    if (_metadata is null)
    {
      throw new InvalidOperationException($"You must call {nameof(WithMetadata)} prior to building");
    }

    var account = Account.From(_name, _metadata);

    foreach (var transaction in _transactions)
    {
      account.AddTransaction(transaction);
    }

    return account;
  }
}