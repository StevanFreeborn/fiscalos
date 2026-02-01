namespace FiscalOS.API.Tests.Infra;

public class TestApi : WebApplicationFactory<Program>
{
  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    base.ConfigureWebHost(builder);

    builder.ConfigureLogging(static c => c.ClearProviders());
  }
}