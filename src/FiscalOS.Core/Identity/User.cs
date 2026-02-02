namespace FiscalOS.Core.Identity;

public sealed class User : Entity
{
  public string Username { get; init; } = string.Empty;
  public string HashedPassword { get; init; } = string.Empty;

  public ICollection<RefreshToken> RefreshTokens { get; init; } = [];

  private User()
  {
  }

  public static User New()
  {
    return new();
  }

  public static User From(string username, string hashedPassword)
  {
    return new()
    {
      Username = username,
      HashedPassword = hashedPassword,
    };
  }
}