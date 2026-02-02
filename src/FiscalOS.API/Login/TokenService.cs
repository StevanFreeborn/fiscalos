using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using FiscalOS.Core.Identity;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FiscalOS.API.Login;

internal sealed class TokenService(
  TimeProvider timeProvider,
  IOptions<JwtOptions> jwtOptions
)
{
  private const int RefreshTokenExpiryInHours = 12;
  private readonly JwtOptions _jwtOptions = jwtOptions.Value;
  private readonly TimeProvider _timeProvider = timeProvider;

  public string GenerateAccessToken(User user)
  {
    var tokenHandler = new JwtSecurityTokenHandler();
    var secretKeyBytes = Encoding.UTF8.GetBytes(_jwtOptions.Secret);
    var issuedAt = _timeProvider.GetUtcNow();
    var expiresAt = issuedAt.AddMinutes(_jwtOptions.ExpiryInMinutes);
    List<Claim> claims = [
      new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
      new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
    ];

    var descriptor = new SecurityTokenDescriptor()
    {
      Subject = new(claims),
      IssuedAt = issuedAt.UtcDateTime,
      Expires = expiresAt.UtcDateTime,
      Issuer = _jwtOptions.Issuer,
      Audience = _jwtOptions.Audience,
      SigningCredentials = new(
        new SymmetricSecurityKey(secretKeyBytes),
        SecurityAlgorithms.HmacSha256Signature
      ),
    };

    var securityToken = tokenHandler.CreateJwtSecurityToken(descriptor);
    var jwtToken = tokenHandler.WriteToken(securityToken);

    return jwtToken;
  }

  public RefreshToken GenerateRefreshToken(User user)
  {
    var expiresAt = _timeProvider
      .GetUtcNow()
      .AddHours(RefreshTokenExpiryInHours);

    return RefreshToken.From(user.Id, GenerateToken(), expiresAt);
  }

  private static string GenerateToken()
  {
    var randomBytes = RandomNumberGenerator.GetBytes(32);
    var token = Convert.ToBase64String(randomBytes);
    return token;
  }
}
