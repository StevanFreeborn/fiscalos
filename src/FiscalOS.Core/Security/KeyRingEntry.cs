namespace FiscalOS.Core.Security;

public sealed record KeyRingEntry
{
  public string KeyId { get; init; }
  public string Key { get; init; }

  private KeyRingEntry(string keyId, string key)
  {
    KeyId = keyId;
    Key = key;
  }

  public static KeyRingEntry From(string keyId, string key)
  {
    ArgumentNullException.ThrowIfNull(keyId);
    ArgumentNullException.ThrowIfNull(key);

    return new(keyId, key);
  }
}