namespace FiscalOS.Core.Accounts;

public sealed class Account : Entity
{
  public Guid UserId { get; init; }
  public Guid InstitutionId { get; init; }
  public Institution? Institution { get; init; }
  public string Name { get; init; } = string.Empty;
  public AccountMetadata? Metadata { get; init; }

  private readonly List<Balance> _balances = [];
  public IEnumerable<Balance> Balances => _balances;

  private readonly List<Transaction> _transactions = [];
  public IEnumerable<Transaction> Transactions => _transactions;

  private Account()
  {
  }

  public static Account From(
    string name,
    AccountMetadata accountMetadata
  )
  {
    return new()
    {
      Name = name,
      Metadata = accountMetadata,
    };
  }

  public void AddBalance(Balance balance)
  {
    ArgumentNullException.ThrowIfNull(balance, nameof(balance));

    _balances.Add(balance);
  }

  public void AddTransaction(Transaction transaction)
  {
    ArgumentNullException.ThrowIfNull(transaction, nameof(transaction));

    _transactions.Add(transaction);
  }
}