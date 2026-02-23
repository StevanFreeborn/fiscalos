using Aspire.Hosting;
using Aspire.Hosting.Testing;

namespace FiscalOS.AppHost.Tests.Infra;

public sealed class AspireFixture : IAsyncLifetime
{
  private DistributedApplication _app = null!;

  public async ValueTask InitializeAsync()
  {
    var appHost = await DistributedApplicationTestingBuilder
        .CreateAsync<Projects.FiscalOS_AppHost>();

    _app = await appHost.BuildAsync();

    await _app.StartAsync();
  }

  public async ValueTask DisposeAsync()
  {
    await _app.StopAsync();
    await _app.DisposeAsync();
  }

  public Uri GetBaseWebUri()
  {
    return _app.GetEndpoint(ProjectNames.Web, "https");
  }
}