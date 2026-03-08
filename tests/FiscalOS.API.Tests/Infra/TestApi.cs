namespace FiscalOS.API.Tests.Infra;

public class TestApi : WebApplicationFactory<Program>
{
  private readonly List<Action<IWebHostBuilder>> _additionalConfigs = [];

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    base.ConfigureWebHost(builder);

    builder.ConfigureLogging(static c => c.ClearProviders());

    builder.ConfigureAppConfiguration(static c =>
    {
      var testConfigPath = Path.Combine(AppContext.BaseDirectory, "appsettings.Test.json");
      c.AddJsonFile(testConfigPath, optional: false);
    });

    builder.ConfigureTestServices(static c =>
    {
      c.AddSingleton(Options.Create(new AppDbContextOptions()
      {
        DatabaseFilePath = $"{Guid.NewGuid()}.db",
      }));

      c.AddSingleton(Options.Create(JwtTokenBuilder.DefaultJwtOptions));

      c.AddSingleton<IKeyRing>(TestKeyRing.From);
    });

    foreach (var config in _additionalConfigs)
    {
      config(builder);
    }
  }

  public TestApi WithAdditionalConfig(Action<IWebHostBuilder> configuration)
  {
    var newApi = new TestApi();

    newApi._additionalConfigs.AddRange(_additionalConfigs);
    newApi._additionalConfigs.Add(configuration);

    return newApi;
  }

  public async Task ExecuteAsync(Func<DbContext, CancellationToken, Task> action, CancellationToken ct)
  {
    await using var scope = Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await action(context, ct);
  }

  public async Task ExecuteAsync(Func<DbContext, CancellationToken, IServiceProvider, Task> action, CancellationToken ct)
  {
    await using var scope = Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await action(context, ct, scope.ServiceProvider);
  }

  public async Task<T> ExecuteAsync<T>(Func<DbContext, CancellationToken, Task<T>> action, CancellationToken ct)
  {
    await using var scope = Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    return await action(context, ct);
  }

  public async Task<T> ExecuteAsync<T>(Func<DbContext, CancellationToken, IServiceProvider, Task<T>> action, CancellationToken ct)
  {
    await using var scope = Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    return await action(context, ct, scope.ServiceProvider);
  }

  public async Task EnsureDbCreatedAsync()
  {
    await ExecuteAsync(static async (context, ct) =>
    {
      await context.Database.EnsureCreatedAsync(ct);
    }, TestContext.Current.CancellationToken);
  }

  public async Task EnsureDbDeletedAsync()
  {
    await ExecuteAsync(static async (context, ct) =>
    {
      await context.Database.EnsureDeletedAsync(ct);
    }, TestContext.Current.CancellationToken);
  }

  public override async ValueTask DisposeAsync()
  {
    await EnsureDbDeletedAsync();
    await base.DisposeAsync();
    GC.SuppressFinalize(this);
  }
}