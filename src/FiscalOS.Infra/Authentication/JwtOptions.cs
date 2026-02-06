namespace FiscalOS.Infra.Authentication;

public sealed record JwtOptions
{
  public string Issuer { get; init; } = string.Empty;
  public string Audience { get; init; } = string.Empty;
  public string Secret { get; init; } = string.Empty;
  public int ExpiryInMinutes { get; init; } = 5;

  public SymmetricSecurityKey Key => new(Encoding.UTF8.GetBytes(Secret));
}

public sealed record JwtOptionsSetup : IConfigureOptions<JwtOptions>
{
  private const string SectionName = nameof(JwtOptions);
  private readonly IConfiguration _configuration;

  public JwtOptionsSetup(IConfiguration configuration)
  {
    _configuration = configuration;
  }

  public void Configure(JwtOptions options)
  {
    _configuration.GetSection(SectionName).Bind(options);
  }
}

public sealed class JwtBearerOptionsSetup(IOptions<JwtOptions> jwtOptions) : IConfigureNamedOptions<JwtBearerOptions>
{
  private readonly JwtOptions _jwtOptions = jwtOptions.Value;

  public void Configure(string? name, JwtBearerOptions options)
  {
    options.TokenValidationParameters = new()
    {
      ValidIssuer = _jwtOptions.Issuer,
      ValidAudience = _jwtOptions.Audience,
      IssuerSigningKey = _jwtOptions.Key,
      ValidateIssuer = true,
      ValidateAudience = true,
      ValidateIssuerSigningKey = true,
      ValidateLifetime = true,
    };

    if (name is Schemes.AllowExpiredTokens)
    {
      options.TokenValidationParameters.ValidateLifetime = false;
    }
  }

  public void Configure(JwtBearerOptions options)
  {
    Configure(Options.DefaultName, options);
  }
}