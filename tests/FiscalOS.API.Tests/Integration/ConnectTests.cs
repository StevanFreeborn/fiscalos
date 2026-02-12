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

  [Fact]
  public async Task Connect_WhenCalledWithoutPublicTokenOrPlaidInstitutionId_ItShouldReturn400WithProblemDetails()
  {
    var jwt = JwtTokenBuilder.New()
      .WithClaim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString())
      .Build();

    using var content = new StringContent(JsonSerializer.Serialize(new { }), Encoding.UTF8, "application/json");
    using var request = new HttpRequestMessage(HttpMethod.Post, ConnectUri)
    {
      Content = content,
    };
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeValidationProblemDetails(new Dictionary<string, string[]>()
    {
      ["PublicToken"] = ["The PublicToken field is required."],
      ["PlaidInstitutionId"] = ["The PlaidInstitutionId field is required."],
    });
  }

  [Fact]
  public async Task Connect_WhenCalledWithoutPublicToken_ItShouldReturn400WithProblemDetails()
  {
    var jwt = JwtTokenBuilder.New()
      .WithClaim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString())
      .Build();

    var json = JsonSerializer.Serialize(new { plaidInstitutionId = "id" });
    using var content = new StringContent(json, Encoding.UTF8, "application/json");
    using var request = new HttpRequestMessage(HttpMethod.Post, ConnectUri)
    {
      Content = content,
    };
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeValidationProblemDetails(new Dictionary<string, string[]>()
    {
      ["PublicToken"] = ["The PublicToken field is required."],
    });
  }

  [Fact]
  public async Task Connect_WhenCalledWithoutPlaidInstitutionId_ItShouldReturn400WithProblemDetails()
  {
    var jwt = JwtTokenBuilder.New()
      .WithClaim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString())
      .Build();

    var json = JsonSerializer.Serialize(new { publicToken = "token" });
    using var content = new StringContent(json, Encoding.UTF8, "application/json");
    using var request = new HttpRequestMessage(HttpMethod.Post, ConnectUri)
    {
      Content = content,
    };
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeValidationProblemDetails(new Dictionary<string, string[]>()
    {
      ["PlaidInstitutionId"] = ["The PlaidInstitutionId field is required."],
    });
  }
}