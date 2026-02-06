namespace FiscalOS.Core.Tests.Unit;

public class UserTests
{
  [Fact]
  public void New_WhenCalled_ItShouldReturnNewUserInstance()
  {
    var user = User.New();

    user.Should().NotBeNull();
    user.Username.Should().BeEmpty();
    user.HashedPassword.Should().BeEmpty();
    user.EncryptionKeyId.Should().BeEmpty();
    user.EncryptedDataKey.Should().BeEmpty();
    user.RefreshTokens.Should().BeEmpty();
  }

  [Fact]
  public void From_WhenCalledWithValidParameters_ItShouldReturnUserInstance()
  {
    var encryptedDataKey = EncryptedDataKey.From("keyId", "encryptedKey");
    var user = User.From("testuser", "hashedpassword", encryptedDataKey);

    user.Should().NotBeNull();
    user.Username.Should().Be("testuser");
    user.HashedPassword.Should().Be("hashedpassword");
    user.EncryptionKeyId.Should().Be("keyId");
    user.EncryptedDataKey.Should().Be("encryptedKey");
    user.RefreshTokens.Should().BeEmpty();
  }

  [Fact]
  public void From_WhenCalledWithNullParameters_ItShouldThrowArgumentNullException()
  {
    var encryptedDataKey = EncryptedDataKey.From("keyId", "encryptedKey");

    var creatingUserWithNullUsername = () => User.From(null!, "hashedpassword", encryptedDataKey);
    creatingUserWithNullUsername.Should().Throw<ArgumentNullException>();

    var creatingUserWithNullHashedPassword = () => User.From("testuser", null!, encryptedDataKey);
    creatingUserWithNullHashedPassword.Should().Throw<ArgumentNullException>();

    var creatingUserWithNullDataKey = () => User.From("testuser", "hashedpassword", null!);
    creatingUserWithNullDataKey.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void AddRefreshToken_WhenCalled_ItShouldAddRefreshTokenToUser()
  {
    var encryptedDataKey = EncryptedDataKey.From("keyId", "encryptedKey");
    var user = User.From("testuser", "hashedpassword", encryptedDataKey);
    var refreshToken = RefreshToken.From(user.Id, "tokenvalue", DateTimeOffset.UtcNow.AddDays(7));

    user.AddRefreshToken(refreshToken);

    user.RefreshTokens.Should().ContainSingle().Which.Should().Be(refreshToken);
  }

  [Fact]
  public void AddRefreshToken_WhenCalledWithNull_ItShouldThrowArgumentNullException()
  {
    var user = User.New();

    var addingNullRefreshToken = () => user.AddRefreshToken(null!);

    addingNullRefreshToken.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void SetCreatedAt_WhenCalled_ItShouldSetCreatedAtProperty()
  {
    var user = User.New();
    var createdAt = DateTimeOffset.UtcNow;

    user.SetCreatedAt(createdAt);

    user.CreatedAt.Should().Be(createdAt);
  }

  [Fact]
  public void SetUpdatedAt_WhenCalled_ItShouldSetUpdatedAtProperty()
  {
    var user = User.New();
    var updatedAt = DateTimeOffset.UtcNow;

    user.SetUpdatedAt(updatedAt);

    user.UpdatedAt.Should().Be(updatedAt);
  }
}