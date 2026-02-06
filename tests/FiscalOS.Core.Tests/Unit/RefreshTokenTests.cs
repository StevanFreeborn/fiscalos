namespace FiscalOS.Core.Tests.Unit;

public class RefreshTokenTests
{
  [Fact]
  public void From_WhenCalledWithUserId_ItShouldReturnARefreshTokenInstance()
  {
    var userId = Guid.NewGuid();
    var token = "token";
    var expiresAt = DateTime.UtcNow.AddDays(7);

    var refreshToken = RefreshToken.From(userId, token, expiresAt);

    refreshToken.UserId.Should().Be(userId);
    refreshToken.User.Should().BeNull();
    refreshToken.Token.Should().Be(token);
    refreshToken.ExpiresAt.Should().Be(expiresAt);
    refreshToken.Revoked.Should().BeFalse();
  }

  [Fact]
  public void From_WhenCalledWithUser_ItShouldReturnARefreshTokenInstance()
  {
    var user = User.From(
      "testuser",
      "hashedpassword",
      EncryptedDataKey.From("keyId", "encryptedKey")
    );

    var token = "token";
    var expiresAt = DateTime.UtcNow.AddDays(7);

    var refreshToken = RefreshToken.From(user, token, expiresAt);

    refreshToken.UserId.Should().Be(user.Id);
    refreshToken.User.Should().Be(user);
    refreshToken.Token.Should().Be(token);
    refreshToken.ExpiresAt.Should().Be(expiresAt);
    refreshToken.Revoked.Should().BeFalse();
  }

  [Fact]
  public void From_WhenCalledWithNullUser_ItShouldThrowArgumentNullException()
  {
    var token = "token";
    var expiresAt = DateTime.UtcNow.AddDays(7);

    var createRefreshTokenWithNullUser = () => RefreshToken.From(null!, token, expiresAt);

    createRefreshTokenWithNullUser.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void Revoke_WhenCalled_ItShouldSetRevokedToTrue()
  {
    var refreshToken = RefreshToken.From(Guid.NewGuid(), "token", DateTime.UtcNow.AddDays(7));

    refreshToken.Revoke();

    refreshToken.Revoked.Should().BeTrue();
  }

  [Fact]
  public void IsExpired_WhenCalledOnExpiredToken_ItShouldReturnTrue()
  {
    var refreshToken = RefreshToken.From(Guid.NewGuid(), "token", DateTime.UtcNow.AddDays(-1));

    var isExpired = refreshToken.IsExpired(DateTimeOffset.UtcNow);

    isExpired.Should().BeTrue();
  }

  [Fact]
  public void IsExpired_WhenCalledOnNonExpiredToken_ItShouldReturnFalse()
  {
    var refreshToken = RefreshToken.From(Guid.NewGuid(), "token", DateTime.UtcNow.AddDays(1));

    var isExpired = refreshToken.IsExpired(DateTimeOffset.UtcNow);

    isExpired.Should().BeFalse();
  }
}