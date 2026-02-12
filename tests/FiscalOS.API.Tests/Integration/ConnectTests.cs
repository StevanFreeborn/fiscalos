namespace FiscalOS.API.Tests.Integration;

public class ConnectTests(TestApi testApi) : IntegrationTest(testApi)
{
  private static readonly Uri ConnectUri = new("/accounts/connect", UriKind.Relative);

  [Fact]
  public async Task Connect_WhenCalledWithoutValidToken_ItShouldReturn401WithProblemDetails()
  {
    var res = await Client.PostAsJsonAsync(ConnectUri, new { }, TestContext.Current.CancellationToken);

    await res.Should().BeProblemDetails(HttpStatusCode.Unauthorized);
  }
}