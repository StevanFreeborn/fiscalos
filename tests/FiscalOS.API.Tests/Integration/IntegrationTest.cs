namespace FiscalOS.API.Tests.Integration;

public abstract class IntegrationTest(TestApi testApi) : IClassFixture<TestApi>, IAsyncLifetime
{
  public HttpClient Client => testApi.CreateClient();

  public async ValueTask InitializeAsync()
  {
    await ExecuteDbContextAsync(static async context =>
    {
      await context.Database.EnsureCreatedAsync();
    });
  }

  protected async Task ExecuteDbContextAsync(Func<DbContext, Task> action)
  {
    await using var scope = testApi.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await action(context);
  }

  protected async Task<T> ExecuteDbContextAsync<T>(Func<DbContext, Task<T>> action)
  {
    await using var scope = testApi.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    return await action(context);
  }

  public async ValueTask DisposeAsync()
  {
    await ExecuteDbContextAsync(static async context =>
    {
      await context.Database.EnsureDeletedAsync();
    });

    GC.SuppressFinalize(this);
  }

}