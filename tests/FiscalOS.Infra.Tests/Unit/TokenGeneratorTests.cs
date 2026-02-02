using System.Globalization;
using System.Security.Cryptography;

namespace FiscalOS.Infra.Tests.Unit;

public class TokenGeneratorTests
{
  private readonly Mock<IOptions<JwtOptions>> _mockJwtOptions = new();
  private readonly Mock<TimeProvider> _mockTimeProvider = new();
  private readonly TokenGenerator _sut;

  public TokenGeneratorTests()
  {
    _sut = TokenGenerator.From(
      _mockTimeProvider.Object,
      _mockJwtOptions.Object
    );
  }

  [Fact]
  public void GenerateAccessToken_WhenCalled_ItShouldReturnValidJwtToken()
  {
    var user = User.From("testuser", "hashedpassword");

    var madeUp256BitSecret = RandomNumberGenerator.GetBytes(32);

    var jwtOptions = new JwtOptions
    {
      Secret = Convert.ToBase64String(madeUp256BitSecret),
      Issuer = "TestIssuer",
      Audience = "TestAudience",
      ExpiryInMinutes = 60,
    };

    _mockJwtOptions
      .SetupGet(static x => x.Value)
      .Returns(jwtOptions);

    var now = DateTimeOffset.UtcNow;

    _mockTimeProvider
      .Setup(static x => x.GetUtcNow())
      .Returns(now);

    var token = _sut.GenerateAccessToken(user);

    token.Should().HaveClaimWithValue(JwtRegisteredClaimNames.Sub, user.Id.ToString());
    token.Should().HaveClaim(JwtRegisteredClaimNames.Jti);
    token.Should().HaveClaimWithValue(JwtRegisteredClaimNames.Iss, jwtOptions.Issuer);
    token.Should().HaveClaimWithValue(JwtRegisteredClaimNames.Aud, jwtOptions.Audience);
    token.Should().HaveClaimWithValue(
      JwtRegisteredClaimNames.Exp,
      new DateTimeOffset(
        now.AddMinutes(jwtOptions.ExpiryInMinutes).UtcDateTime
      )
      .ToUnixTimeSeconds()
      .ToString(CultureInfo.InvariantCulture)
    );
  }

  [Fact]
  public void GenerateRefreshToken_WhenCalled_ItShouldReturnRefreshTokenWithCorrectProperties()
  {
    var user = User.From("testuser", "hashedpassword");

    var now = DateTimeOffset.UtcNow;

    _mockTimeProvider
      .Setup(static x => x.GetUtcNow())
      .Returns(now);

    var refreshToken = _sut.GenerateRefreshToken(user);

    refreshToken.UserId.Should().Be(user.Id);
    refreshToken.ExpiresAt.Should().Be(now.AddHours(12));
    refreshToken.Token.Should().NotBeNullOrEmpty();
  }
}