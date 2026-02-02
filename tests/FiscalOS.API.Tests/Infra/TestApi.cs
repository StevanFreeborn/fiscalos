using System.Security.Cryptography;

using FiscalOS.Infra.Authentication;

namespace FiscalOS.API.Tests.Infra;

public class TestApi : WebApplicationFactory<Program>
{
  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    base.ConfigureWebHost(builder);

    builder.ConfigureLogging(static c => c.ClearProviders());

    builder.ConfigureTestServices(static c =>
    {
      var dbOpts = Options.Create(new AppDbContextOptions()
      {
        DatabaseFilePath = $"{Guid.NewGuid()}.db",
      });

      c.AddSingleton(dbOpts);

      var jwtOpts = Options.Create(new JwtOptions()
      {
        Issuer = "TestIssuer",
        Audience = "TestAudience",
        Secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
        ExpiryInMinutes = 5,
      });

      c.AddSingleton(jwtOpts);
    });
  }
}