namespace FiscalOS.API.Identity;

internal sealed class RefreshToken
{
  public Guid Id { get; init; }

  public Guid UserId { get; init; }
  public User User { get; init; } = new();
}