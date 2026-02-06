
namespace FiscalOS.API.Tests.Infra;

internal sealed class TestKeyRing : IKeyRing
{
  private string _key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

  private TestKeyRing()
  {
  }

  public static TestKeyRing From(IServiceProvider serviceProvider)
  {
    return new();
  }

  public KeyRingEntry GetKey(string keyId)
  {
    return KeyRingEntry.From(keyId, _key);
  }

  public KeyRingEntry GetPrimaryKey()
  {
    return KeyRingEntry.From("primary-key-id", _key);
  }

  public Task<KeyRingEntry> SaveKeyAsync(string key)
  {
    _key = key;
    var entry = KeyRingEntry.From(Guid.NewGuid().ToString(), key);
    return Task.FromResult(entry);
  }
}