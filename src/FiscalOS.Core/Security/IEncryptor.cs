namespace FiscalOS.Core.Security;

public interface IEncryptor
{
  string GenerateKey();
  Task<EncryptedDataKey> GenerateEncryptedKeyAsync(CancellationToken ct);
  Task<string> EncryptAsync(string plainText, CancellationToken ct);
  Task<string> EncryptAsyncFor(User user, string plainText, CancellationToken ct);
  Task<string> DecryptAsync(string cipherText, CancellationToken ct);
  Task<string> DecryptAsyncFor(User user, string plainText, CancellationToken ct);
}