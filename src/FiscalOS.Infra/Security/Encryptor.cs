
namespace FiscalOS.Infra.Security;

public sealed class Encryptor : IEncryptor
{
  private readonly IKeyRing _keyRing;

  private Encryptor(IKeyRing keyRing)
  {
    _keyRing = keyRing;
  }

  public static Encryptor From(IServiceProvider serviceProvider)
  {
    var keyRing = serviceProvider.GetRequiredService<IKeyRing>();
    return new Encryptor(keyRing);
  }

  public static Encryptor From(IKeyRing keyRing)
  {
    return new Encryptor(keyRing);
  }

  private KeyRingEntry PrimaryKey => _keyRing.GetPrimaryKey();

  private static async Task<string> DecryptCoreAsync(string key, string cipherText, CancellationToken ct)
  {
    var cipherTextBytes = Convert.FromBase64String(cipherText);

    using var aes = Aes.Create();
    aes.Key = Convert.FromBase64String(key);

    var iv = new byte[16];
    Array.Copy(cipherTextBytes, 0, iv, 0, 16);
    aes.IV = iv;

    var decryptor = aes.CreateDecryptor();

    using var memoryStream = new MemoryStream(cipherTextBytes, 16, cipherTextBytes.Length - 16);
    using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
    using var streamReader = new StreamReader(cryptoStream);

    return await streamReader.ReadToEndAsync(ct).ConfigureAwait(false);
  }

  public async Task<string> DecryptAsync(string cipherText, CancellationToken ct)
  {
    return await DecryptCoreAsync(PrimaryKey.Key, cipherText, ct).ConfigureAwait(false);
  }

  public async Task<string> DecryptAsyncFor(User user, string plainText, CancellationToken ct)
  {
    var decryptedKey = await DecryptCoreAsync(PrimaryKey.Key, user.EncryptedDataKey, ct).ConfigureAwait(false);
    return await DecryptCoreAsync(decryptedKey, plainText, ct).ConfigureAwait(false);
  }

  private static async Task<string> EncryptCoreAsync(string key, string plainText, CancellationToken ct)
  {
    using var aes = Aes.Create();
    aes.Key = Convert.FromBase64String(key);

    using var memoryStream = new MemoryStream();
    aes.GenerateIV();
    await memoryStream.WriteAsync(aes.IV, ct).ConfigureAwait(false);

    var encryptor = aes.CreateEncryptor();

    using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
    using var streamWriter = new StreamWriter(cryptoStream);

    await streamWriter.WriteAsync(plainText.ToCharArray(), ct).ConfigureAwait(false);
    await streamWriter.FlushAsync(ct).ConfigureAwait(false);
    streamWriter.Close();

    return Convert.ToBase64String(memoryStream.ToArray());
  }

  public async Task<string> EncryptAsync(string plainText, CancellationToken ct)
  {
    return await EncryptCoreAsync(PrimaryKey.Key, plainText, ct).ConfigureAwait(false);
  }

  public async Task<string> EncryptAsyncFor(User user, string plainText, CancellationToken ct)
  {
    var decryptedKey = await DecryptCoreAsync(PrimaryKey.Key, user.EncryptedDataKey, ct).ConfigureAwait(false);
    return await EncryptCoreAsync(decryptedKey, plainText, ct).ConfigureAwait(false);
  }

  public string GenerateKey()
  {
    using var aes = Aes.Create();
    aes.GenerateKey();

    return Convert.ToBase64String(aes.Key);
  }

  public async Task<EncryptedDataKey> GenerateEncryptedKeyAsync(CancellationToken ct)
  {
    var key = GenerateKey();
    var keyUsed = PrimaryKey;
    var encryptedKey = await EncryptCoreAsync(keyUsed.Key, key, ct).ConfigureAwait(false);
    return EncryptedDataKey.From(keyUsed.KeyId, encryptedKey);
  }

}