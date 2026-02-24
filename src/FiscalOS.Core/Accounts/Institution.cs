namespace FiscalOS.Core.Accounts;

public sealed class Institution : Entity
{
  public Guid UserId { get; init; }
  public string Name { get; init; } = string.Empty;
  public InstitutionMetadata? Metadata { get; init; }

  private readonly List<Account> _accounts = [];
  public IEnumerable<Account> Accounts => _accounts;

  private Institution()
  {
  }

  public static Institution From(string? name, InstitutionMetadata metadata)
  {
    ArgumentNullException.ThrowIfNull(name, nameof(name));
    ArgumentNullException.ThrowIfNull(metadata, nameof(metadata));

    return new Institution()
    {
      Name = name,
      Metadata = metadata
    };
  }
}