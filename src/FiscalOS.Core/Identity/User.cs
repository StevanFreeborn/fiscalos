namespace FiscalOS.Core.Identity;

public sealed class User : Entity
{
  private readonly List<RefreshToken> _refreshTokens = [];

  public string Username { get; init; } = string.Empty;
  public string HashedPassword { get; init; } = string.Empty;
  public string EncryptionKeyId { get; init; } = string.Empty;
  public string EncryptedDataKey { get; init; } = string.Empty;

  public IEnumerable<RefreshToken> RefreshTokens => _refreshTokens;

  private User()
  {
  }

  public static User New()
  {
    return new();
  }

  public static User From(string username, string hashedPassword, EncryptedDataKey encryptedDataKey)
  {
    ArgumentNullException.ThrowIfNull(username);
    ArgumentNullException.ThrowIfNull(hashedPassword);
    ArgumentNullException.ThrowIfNull(encryptedDataKey);

    return new()
    {
      Username = username,
      HashedPassword = hashedPassword,
      EncryptedDataKey = encryptedDataKey.EncryptedKey,
      EncryptionKeyId = encryptedDataKey.KeyIdUsed,
    };
  }

  public void AddRefreshToken(RefreshToken refreshToken)
  {
    ArgumentNullException.ThrowIfNull(refreshToken);

    _refreshTokens.Add(refreshToken);
  }
}