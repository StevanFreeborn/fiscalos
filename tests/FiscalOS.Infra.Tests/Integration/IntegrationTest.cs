namespace FiscalOS.Infra.Tests.Integration;

public abstract class IntegrationTest : IAsyncLifetime
{
  private bool _isDisposed;
  private readonly FileSystem _fileSystem = new();
  protected AppDbContext AppDbContext { get; }

  protected IntegrationTest()
  {
    var options = Options.Create(new AppDbContextOptions()
    {
      DatabaseFilePath = $"{Guid.NewGuid()}.db",
    });

    AppDbContext = new(options, _fileSystem);
  }

  public async ValueTask InitializeAsync()
  {
    await AppDbContext.Database.MigrateAsync();
  }

  public async ValueTask DisposeAsync()
  {
    await DisposeAsync(true);
    GC.SuppressFinalize(this);
  }

  protected virtual async Task DisposeAsync(bool disposing)
  {
    if (_isDisposed)
    {
      return;
    }

    if (disposing)
    {
      await AppDbContext.DisposeAsync();
    }

    _isDisposed = true;
  }
}