namespace FiscalOS.Core.Accounts;

public sealed class Account : Entity
{
  public Guid UserId { get; init; }
  public Guid InstitutionId { get; init; }
  public Institution Institution { get; init; } = null!;
  public string Name { get; init; } = string.Empty;
  public AccountMetadata Metadata { get; init; } = null!;

  public static Account From(Guid institutionId, string name, AccountMetadata accountMetadata)
  {
    return new()
    {
      InstitutionId = institutionId,
      Name = name,
      Metadata = accountMetadata,
    };
  }
}