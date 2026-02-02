namespace FiscalOS.Infra.Authentication;

public sealed record JwtOptions
{
  public string Issuer { get; init; } = string.Empty;
  public string Audience { get; init; } = string.Empty;
  public string Secret { get; init; } = string.Empty;
  public int ExpiryInMinutes { get; init; } = 5;
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