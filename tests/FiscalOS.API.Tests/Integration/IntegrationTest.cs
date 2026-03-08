namespace FiscalOS.API.Tests.Integration;

public abstract class IntegrationTest(TestApi testApi) : IClassFixture<TestApi>, IAsyncLifetime
{
  protected TestApi Api => testApi;
  protected HttpClient Client => testApi.CreateClient();

  public async ValueTask InitializeAsync()
  {
    await Api.EnsureDbCreatedAsync();
  }

  public async ValueTask DisposeAsync()
  {
    await Api.EnsureDbDeletedAsync();
    GC.SuppressFinalize(this);
  }
}