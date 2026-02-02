namespace FiscalOS.Infra.Data;

public sealed record AppDbContextOptions
{
  public string DatabaseFilePath { get; init; } = string.Empty;
  public string GetFullyQualifiedDatabasePath()
  {
    return Path.GetFullPath(DatabaseFilePath, AppContext.BaseDirectory);
  }
}

public sealed record AppDbContextOptionsSetup : IConfigureOptions<AppDbContextOptions>
{
  private const string SectionName = nameof(AppDbContextOptions);
  private readonly IConfiguration _configuration;

  public AppDbContextOptionsSetup(IConfiguration configuration)
  {
    _configuration = configuration;
  }

  public void Configure(AppDbContextOptions options)
  {
    _configuration.GetSection(SectionName).Bind(options);
  }
}