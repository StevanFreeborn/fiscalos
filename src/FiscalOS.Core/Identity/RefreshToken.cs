namespace FiscalOS.Core.Identity;

public sealed class RefreshToken : Entity
{
  public string Token { get; init; } = string.Empty;
  public DateTimeOffset ExpiresAt { get; init; }
  public Guid UserId { get; init; }
  public User User { get; init; } = User.New();

  private RefreshToken()
  {
  }

  public static RefreshToken From(Guid userId, string token, DateTimeOffset expiresAt)
  {
    return new()
    {
      UserId = userId,
      Token = token,
      ExpiresAt = expiresAt,
    };
  }
}