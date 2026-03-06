namespace FiscalOS.Core.Accounts;

public sealed class Institution : Entity
{
  public Guid UserId { get; init; }
  public User? User { get; init; }
  public string Name { get; init; } = string.Empty;
  public InstitutionMetadata? Metadata { get; init; }

  private readonly List<Account> _accounts = [];
  public IEnumerable<Account> Accounts => _accounts;

  private Institution()
  {
  }

  public static Institution From(string? name, InstitutionMetadata metadata)
  {
    return From(null, name, metadata);
  }

  public static Institution From(User? user, string? name, InstitutionMetadata metadata)
  {
    ArgumentNullException.ThrowIfNull(name, nameof(name));
    ArgumentNullException.ThrowIfNull(metadata, nameof(metadata));

    return new Institution()
    {
      UserId = user?.Id ?? Guid.Empty,
      User = user,
      Name = name,
      Metadata = metadata
    };
  }

  public void AddAccount(Account account)
  {
    ArgumentNullException.ThrowIfNull(account, nameof(account));

    _accounts.Add(account);
  }
}