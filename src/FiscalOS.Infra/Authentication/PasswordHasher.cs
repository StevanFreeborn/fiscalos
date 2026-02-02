namespace FiscalOS.Infra.Authentication;

public sealed class PasswordHasher : IPasswordHasher
{
  private const int SaltSize = 16;
  private const int HashSize = 32;
  private const int Iterations = 100_000;

  private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA512;

  private PasswordHasher()
  {
  }

  public static PasswordHasher New()
  {
    return new();
  }

  public static PasswordHasher From(IServiceProvider serviceProvider)
  {
    return new();
  }

  public string Hash(string password)
  {
    var salt = RandomNumberGenerator.GetBytes(SaltSize);
    var hash = Rfc2898DeriveBytes.Pbkdf2(
      password,
      salt,
      Iterations,
      HashAlgorithm,
      HashSize
    );

    var hashBytes = new byte[SaltSize + HashSize];
    Array.Copy(salt, 0, hashBytes, 0, SaltSize);
    Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

    return Convert.ToBase64String(hashBytes);
  }

  public bool Verify(string providedPassword, string hashedPassword)
  {
    var hashBytes = Convert.FromBase64String(hashedPassword);

    var salt = new byte[SaltSize];
    Array.Copy(hashBytes, 0, salt, 0, SaltSize);

    var storedHash = new byte[HashSize];
    Array.Copy(hashBytes, SaltSize, storedHash, 0, HashSize);

    var computedHash = Rfc2898DeriveBytes.Pbkdf2(
      providedPassword,
      salt,
      Iterations,
      HashAlgorithm,
      HashSize
    );

    return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
  }
}