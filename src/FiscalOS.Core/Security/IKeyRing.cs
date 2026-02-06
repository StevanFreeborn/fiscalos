namespace FiscalOS.Core.Security;

public interface IKeyRing
{
  KeyRingEntry GetKey(string keyId);
  KeyRingEntry GetPrimaryKey();
  Task<KeyRingEntry> SaveKeyAsync(string key);
}