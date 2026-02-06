namespace FiscalOS.Infra.Tests.Unit;

public class EncryptorTests
{
  private readonly Mock<IKeyRing> _mockKeyRing = new();
  private readonly Encryptor _sut;

  public EncryptorTests()
  {
    var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    _mockKeyRing.Setup(static kr => kr.GetPrimaryKey()).Returns(KeyRingEntry.From("primary-key-id", key));
    _sut = Encryptor.From(_mockKeyRing.Object);
  }

  [Fact]
  public void GenerateKey_WhenCalled_ItShouldReturnKey()
  {
    var key = _sut.GenerateKey();
    key.Should().NotBeNull();
  }

  [Fact]
  public async Task GenerateEncryptedKeyAsync_WhenCalled_ItShouldReturnEncryptedKey()
  {
    var encryptedKey = await _sut.GenerateEncryptedKeyAsync(TestContext.Current.CancellationToken);
    encryptedKey.Should().NotBeNull();
  }

  [Fact]
  public async Task Encrypt_WhenCalledWithPlainText_ItShouldReturnCipherText()
  {
    var plainText = "Hello, World";

    var cipherText = await _sut.EncryptAsync(plainText, TestContext.Current.CancellationToken);

    cipherText.Should().NotBe(plainText);
  }

  [Fact]
  public async Task Decrypt_WhenCalledWithCipherText_ItShouldReturnPlainText()
  {
    var plainText = "Hello, World";
    var cipherText = await _sut.EncryptAsync(plainText, TestContext.Current.CancellationToken);

    var decryptedData = await _sut.DecryptAsync(cipherText, TestContext.Current.CancellationToken);

    decryptedData.Should().Be(plainText);
  }

  [Fact]
  public async Task EncryptForUser_WhenCalledWithPlainText_ItShouldReturnCipherText()
  {
    var plainText = "Hello, World";
    var userEncryptionKey = await _sut.GenerateEncryptedKeyAsync(TestContext.Current.CancellationToken);
    var user = User.From("Stevan", "HashedPassword", userEncryptionKey);

    var cipherText = await _sut.EncryptAsyncFor(user, plainText, TestContext.Current.CancellationToken);

    cipherText.Should().NotBe(plainText);
  }

  [Fact]
  public async Task DecryptForUser_WhenCalledWithCipherText_ItShouldReturnPlainText()
  {
    var plainText = "Hello, World";
    var encryptedDataKey = await _sut.GenerateEncryptedKeyAsync(TestContext.Current.CancellationToken);
    var user = User.From("Stevan", "HashedPassword", encryptedDataKey);
    var cipherText = await _sut.EncryptAsyncFor(user, plainText, TestContext.Current.CancellationToken);

    var decryptedData = await _sut.DecryptAsyncFor(user, cipherText, TestContext.Current.CancellationToken);

    decryptedData.Should().Be(plainText);
  }
}