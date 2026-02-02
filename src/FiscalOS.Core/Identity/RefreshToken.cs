namespace FiscalOS.Core.Identity;

public sealed class RefreshToken : Entity
{
  public Guid UserId { get; init; }
  public User User { get; init; } = new();
}