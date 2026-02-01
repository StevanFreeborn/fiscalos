using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc;

namespace FiscalOS.API.Tests.Integration;

public class LoginTests(TestApi testApi) : IClassFixture<TestApi>
{
  private readonly TestApi _testApi = testApi;

  [Fact]
  public async Task Login_WhenUserSubmitsWithoutUsernameOrPassword_ItShouldReturn400WithProblemDetails()
  {
    var client = _testApi.CreateClient();

    var res = await client.PostAsJsonAsync("/login", new { }, TestContext.Current.CancellationToken);

    res.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

    var problem = await res.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);

    problem!.Errors.Should().BeEquivalentTo(new Dictionary<string, string[]>()
    {
      ["Username"] = ["The Username field is required."],
      ["Password"] = ["The Password field is required."]
    });
  }
}