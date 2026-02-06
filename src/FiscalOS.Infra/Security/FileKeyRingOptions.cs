namespace FiscalOS.Infra.Security;

public sealed record FileKeyRingOptions
{
  public string KeysDirectoryPath { get; init; } = string.Empty;
  public string PrimaryKeyId { get; init; } = string.Empty;
}

public sealed record class FileKeyRingOptionsSetup : IConfigureOptions<FileKeyRingOptions>
{
  private const string SectionName = nameof(FileKeyRingOptions);
  private readonly IConfiguration _configuration;

  public FileKeyRingOptionsSetup(IConfiguration configuration)
  {
    _configuration = configuration;
  }

  public void Configure(FileKeyRingOptions options)
  {
    _configuration.GetSection(SectionName).Bind(options);
  }
}