namespace FiscalOS.API.Tests.Infra;

internal sealed class JwtTokenBuilder
{
  private const string Issuer = "TestIssuer";
  private const string Audience = "TestAudience";
  private const int ExpiryInMinutes = 5;
  private static readonly string Secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
  private readonly List<Claim> _claims = [];
  private DateTimeOffset _expiresAt = DateTimeOffset.UtcNow.AddMinutes(ExpiryInMinutes);

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

  public JwtTokenBuilder WithExpiresAt(DateTimeOffset expiresAt)
  {
    _expiresAt = expiresAt;
    return this;
  }

  public string Build()
  {
    var tokenHandler = new JwtSecurityTokenHandler();
    var issuedAt = _expiresAt.AddMinutes(-ExpiryInMinutes);

    var descriptor = new SecurityTokenDescriptor()
    {
      Subject = new(_claims),
      NotBefore = issuedAt.UtcDateTime,
      IssuedAt = issuedAt.UtcDateTime,
      Expires = _expiresAt.UtcDateTime,
      Issuer = Issuer,
      Audience = Audience,
      SigningCredentials = new(
        DefaultJwtOptions.Key,
        SecurityAlgorithms.HmacSha256Signature
      ),
    };

    var securityToken = tokenHandler.CreateToken(descriptor);
    var jwtToken = tokenHandler.WriteToken(securityToken);

    return jwtToken;
  }
}