namespace FiscalOS.Core.Tests.Unit;

public class EncryptedDataKeyTests
{
  [Fact]
  public void From_WhenCalled_ItShouldReturnAnInstance()
  {
    var keyId = "key-id";
    var encryptedKey = "encrypted-key";

    var encryptedDataKey = EncryptedDataKey.From(keyId, encryptedKey);

    encryptedDataKey.KeyIdUsed.Should().Be(keyId);
    encryptedDataKey.EncryptedKey.Should().Be(encryptedKey);
  }

  [Fact]
  public void From_WhenCalledWithNullKeyId_ItShouldThrowArgumentNullException()
  {
    var encryptedKey = "encrypted-key";

    var creatingDataKeyWithNullKeyId = () => EncryptedDataKey.From(null!, encryptedKey);

    creatingDataKeyWithNullKeyId.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void From_WhenCalledWithNullEncryptedKey_ItShouldThrowArgumentNullException()
  {
    var keyId = "key-id";

    var creatingDataKeyWithNullEncryptedKey = () => EncryptedDataKey.From(keyId, null!);

    creatingDataKeyWithNullEncryptedKey.Should().Throw<ArgumentNullException>();
  }
}