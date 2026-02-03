
namespace FiscalOS.API.Tests.Integration;

public class RefreshTests(TestApi testApi) : IntegrationTest(testApi)
{
  private static readonly Uri RefreshUri = new("/refresh", UriKind.Relative);

  [Fact]
  public async Task Refresh_WhenCalledWithNoAccessToken_ItShouldReturn401Unauthorized()
  {
    var response = await Client.PostAsync(RefreshUri, null, TestContext.Current.CancellationToken); ;

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }
}