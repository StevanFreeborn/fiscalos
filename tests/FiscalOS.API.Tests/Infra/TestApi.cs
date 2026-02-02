namespace FiscalOS.API.Tests.Infra;

public class TestApi : WebApplicationFactory<Program>
{
  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    base.ConfigureWebHost(builder);

    builder.ConfigureLogging(static c => c.ClearProviders());

    builder.ConfigureTestServices(static c =>
    {
      var opts = Options.Create(new AppDbContextOptions()
      {
        DatabaseFilePath = $"{Guid.NewGuid()}.db",
      });

      c.AddSingleton(opts);
    });
  }
}