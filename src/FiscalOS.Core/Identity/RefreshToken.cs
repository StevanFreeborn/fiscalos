namespace FiscalOS.Core.Identity;

public sealed class RefreshToken : Entity
{
  public string Token { get; init; } = string.Empty;
  public DateTimeOffset ExpiresAt { get; init; }
  public Guid UserId { get; init; }
  public User? User { get; init; }
  public bool Revoked { get; set; }

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

  public static RefreshToken From(User user, string token, DateTimeOffset expiresAt)
  {
    ArgumentNullException.ThrowIfNull(user, nameof(user));

    return new()
    {
      User = user,
      UserId = user.Id,
      Token = token,
      ExpiresAt = expiresAt,
    };
  }

  public void Revoke()
  {
    Revoked = true;
  }

  public bool IsExpired(DateTimeOffset now)
  {
    return now >= ExpiresAt;
  }
}