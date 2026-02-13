namespace FiscalOS.Core.Security;

public sealed record EncryptedDataKey
{
  public string KeyIdUsed { get; init; }
  public string EncryptedKey { get; init; }

  private EncryptedDataKey(string keyIdUsed, string encryptedKey)
  {
    KeyIdUsed = keyIdUsed;
    EncryptedKey = encryptedKey;
  }

  public static EncryptedDataKey From(string keyIdUsed, string encryptedKey)
  {
    ArgumentNullException.ThrowIfNull(keyIdUsed, nameof(keyIdUsed));
    ArgumentNullException.ThrowIfNull(encryptedKey, nameof(encryptedKey));

    return new(keyIdUsed, encryptedKey);
  }
}