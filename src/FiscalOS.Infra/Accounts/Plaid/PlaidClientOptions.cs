namespace FiscalOS.Infra.Accounts.Plaid;

public sealed record PlaidClientOptions
{
  public string ClientId { get; init; } = string.Empty;
  public string Secret { get; init; } = string.Empty;

  public IOptions<PlaidOptions> ToPlaidOptions()
  {
    return Options.Create<PlaidOptions>(new()
    {
      ClientId = ClientId,
      Secret = Secret
    });
  }
}

public sealed record PlaidClientOptionsSetup : IConfigureOptions<PlaidClientOptions>
{
  private const string SectionName = nameof(PlaidClientOptions);
  private readonly IConfiguration _configuration;

  public PlaidClientOptionsSetup(IConfiguration configuration)
  {
    _configuration = configuration;
  }

  public void Configure(PlaidClientOptions options)
  {
    _configuration.GetSection(SectionName).Bind(options);
  }
}