namespace FiscalOS.API.Tests.Integration;

public abstract class IntegrationTest(TestApi testApi) : IClassFixture<TestApi>, IAsyncLifetime
{
  public HttpClient Client => testApi.CreateClient();

  public async ValueTask InitializeAsync()
  {
    await ExecuteAsync(static async (context, ct) =>
    {
      await context.Database.EnsureCreatedAsync(ct);
    }, TestContext.Current.CancellationToken);
  }

  protected async Task ExecuteAsync(Func<DbContext, CancellationToken, Task> action, CancellationToken ct)
  {
    await using var scope = testApi.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await action(context, ct);
  }

  protected async Task ExecuteAsync(Func<DbContext, CancellationToken, IServiceProvider, Task> action, CancellationToken ct)
  {
    await using var scope = testApi.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await action(context, ct, scope.ServiceProvider);
  }

  protected async Task<T> ExecuteAsync<T>(Func<DbContext, CancellationToken, Task<T>> action, CancellationToken ct)
  {
    await using var scope = testApi.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    return await action(context, ct);
  }

  protected async Task<T> ExecuteAsync<T>(Func<DbContext, CancellationToken, IServiceProvider, Task<T>> action, CancellationToken ct)
  {
    await using var scope = testApi.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    return await action(context, ct, scope.ServiceProvider);
  }

  public async ValueTask DisposeAsync()
  {
    await ExecuteAsync(static async (context, ct) =>
    {
      await context.Database.EnsureDeletedAsync(ct);
    }, TestContext.Current.CancellationToken);

    GC.SuppressFinalize(this);
  }
}