namespace FiscalOS.API.Tests.Infra;

internal sealed class JwtTokenBuilder
{
  private const string Issuer = "TestIssuer";
  private const string Audience = "TestAudience";
  private const int ExpiryInMinutes = 5;
  private static readonly string Secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
  private readonly List<Claim> _claims = [];
  private DateTimeOffset _issuedAt = DateTimeOffset.UtcNow;

  public static JwtOptions DefaultJwtOptions => new()
  {
    Issuer = Issuer,
    Audience = Audience,
    Secret = Secret,
    ExpiryInMinutes = ExpiryInMinutes,
  };

  public static JwtTokenBuilder New()
  {
    return new JwtTokenBuilder();
  }

  public JwtTokenBuilder WithClaim(string type, string value)
  {
    _claims.Add(new(type, value));
    return this;
  }

  public JwtTokenBuilder IssuedAt(DateTimeOffset issuedAt)
  {
    _issuedAt = issuedAt;
    return this;
  }

  public string Build()
  {
    var tokenHandler = new JwtSecurityTokenHandler();
    var expiresAt = _issuedAt.AddMinutes(ExpiryInMinutes);

    var descriptor = new SecurityTokenDescriptor()
    {
      Subject = new(_claims),
      IssuedAt = _issuedAt.UtcDateTime,
      Expires = expiresAt.UtcDateTime,
      Issuer = Issuer,
      Audience = Audience,
      SigningCredentials = new(
        DefaultJwtOptions.Key,
        SecurityAlgorithms.HmacSha256Signature
      ),
    };

    var securityToken = tokenHandler.CreateJwtSecurityToken(descriptor);
    var jwtToken = tokenHandler.WriteToken(securityToken);

    return jwtToken;
  }
}