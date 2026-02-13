namespace FiscalOS.Core.Identity;

public sealed class User : Entity
{
  public string Username { get; init; } = string.Empty;
  public string HashedPassword { get; init; } = string.Empty;
  public string EncryptionKeyId { get; init; } = string.Empty;
  public string EncryptedDataKey { get; init; } = string.Empty;

  private readonly List<RefreshToken> _refreshTokens = [];
  public IEnumerable<RefreshToken> RefreshTokens => _refreshTokens;

  private readonly List<Institution> _institutions = [];
  public IEnumerable<Institution> Institutions => _institutions;

  private readonly List<Account> _accounts = [];
  public IEnumerable<Account> Accounts => _accounts;

  private User()
  {
  }

  public static User New()
  {
    return new();
  }

  public static User From(string username, string hashedPassword, EncryptedDataKey encryptedDataKey)
  {
    ArgumentNullException.ThrowIfNull(username, nameof(username));
    ArgumentNullException.ThrowIfNull(hashedPassword, nameof(hashedPassword));
    ArgumentNullException.ThrowIfNull(encryptedDataKey, nameof(encryptedDataKey));

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
    ArgumentNullException.ThrowIfNull(refreshToken, nameof(refreshToken));

    _refreshTokens.Add(refreshToken);
  }

  public void AddInstitution(Institution institution)
  {
    ArgumentNullException.ThrowIfNull(institution, nameof(institution));

    _institutions.Add(institution);
  }

  public void AddAccount(Account account)
  {
    ArgumentNullException.ThrowIfNull(account, nameof(account));

    _accounts.Add(account);
  }
}