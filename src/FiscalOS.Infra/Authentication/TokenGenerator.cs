namespace FiscalOS.Infra.Authentication;

public sealed class TokenGenerator : ITokenGenerator
{
  private const int RefreshTokenExpiryInHours = 12;
  private readonly IOptions<JwtOptions> _jwtOptions;
  private readonly TimeProvider _timeProvider;

  private TokenGenerator(
    TimeProvider timeProvider,
    IOptions<JwtOptions> jwtOptions
  )
  {
    _timeProvider = timeProvider;
    _jwtOptions = jwtOptions;
  }

  public static TokenGenerator From(IServiceProvider serviceProvider)
  {
    var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();
    var jwtOptions = serviceProvider.GetRequiredService<IOptions<JwtOptions>>();
    return new(timeProvider, jwtOptions);
  }

  public static TokenGenerator From(
    TimeProvider timeProvider,
    IOptions<JwtOptions> jwtOptions
  )
  {
    return new(timeProvider, jwtOptions);
  }

  public string GenerateAccessToken(User user)
  {
    var tokenHandler = new JwtSecurityTokenHandler();
    var issuedAt = _timeProvider.GetUtcNow();
    var expiresAt = issuedAt.AddMinutes(_jwtOptions.Value.ExpiryInMinutes);
    List<Claim> claims = [
      new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
      new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
    ];

    var descriptor = new SecurityTokenDescriptor()
    {
      Subject = new(claims),
      IssuedAt = issuedAt.UtcDateTime,
      Expires = expiresAt.UtcDateTime,
      Issuer = _jwtOptions.Value.Issuer,
      Audience = _jwtOptions.Value.Audience,
      SigningCredentials = new(
        _jwtOptions.Value.Key,
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