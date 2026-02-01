namespace FiscalOS.API.Tests.Integration;

public class LoginTests(TestApi testApi) : IntegrationTest(testApi)
{
  [Theory]
  [ClassData<LoginValidationTestCases>]
  public async Task Login_WhenUserSubmitsInvalidRequest_ItShouldReturn400WithProblemDetails(LoginValidationTestCase tc)
  {
    var client = TestApi.CreateClient();

    var req = new
    {
      username = tc.Username,
      password = tc.Password,
    };

    var res = await client.PostAsJsonAsync("/login", req, TestContext.Current.CancellationToken);

    res.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    var problem = await res.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);

    problem!.Errors.Should().BeEquivalentTo(tc.ExpectedErrors);
  }

  [Fact]
  public async Task Login_WhenUserCredentialsAreIncorrect_ItShouldReturn401WithProblemDetails()
  {
    var client = TestApi.CreateClient();

    var req = new
    {
      username = "Test",
      password = "@Password2",
    };

    var res = await client.PostAsJsonAsync("/login", req, TestContext.Current.CancellationToken);

    res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }
}


public class LoginValidationTestCases : TheoryData<LoginValidationTestCase>
{
  public LoginValidationTestCases()
  {
    Add(new LoginValidationTestCase(
      "No username or password",
      string.Empty,
      string.Empty,
      new Dictionary<string, string[]>()
      {
        ["Username"] = ["The Username field is required."],
        ["Password"] = ["The Password field is required."]
      }
    ));

    Add(new LoginValidationTestCase(
      "No username",
      string.Empty,
      "@Password1",
      new Dictionary<string, string[]>()
      {
        ["Username"] = ["The Username field is required."],
      }
    ));

    Add(new LoginValidationTestCase(
      "No password",
      "Stevan",
      string.Empty,
      new Dictionary<string, string[]>()
      {
        ["Password"] = ["The Password field is required."]
      }
    ));
  }
}

public record LoginValidationTestCase : IXunitSerializable
{
  public string Name { get; private set; } = string.Empty;
  public string Username { get; private set; } = string.Empty;
  public string Password { get; private set; } = string.Empty;
  public Dictionary<string, string[]> ExpectedErrors { get; private set; } = [];

  public override string ToString()
  {
    return Name;
  }

  public LoginValidationTestCase()
  {
  }

  public LoginValidationTestCase(
    string name,
    string username,
    string password,
    Dictionary<string, string[]> expectedErrors
  )
  {
    Name = name;
    Username = username;
    Password = password;
    ExpectedErrors = expectedErrors;
  }

  public void Deserialize(IXunitSerializationInfo info)
  {
    Name = info.GetValue<string>(nameof(Name)) ?? string.Empty;
    Username = info.GetValue<string>(nameof(Username)) ?? string.Empty;
    Password = info.GetValue<string>(nameof(Password)) ?? string.Empty;
    ExpectedErrors = info.GetValue<Dictionary<string, string[]>>(nameof(ExpectedErrors)) ?? [];
  }

  public void Serialize(IXunitSerializationInfo info)
  {
    info.AddValue(nameof(Name), Name);
    info.AddValue(nameof(Username), Username);
    info.AddValue(nameof(Password), Password);
    info.AddValue(nameof(ExpectedErrors), ExpectedErrors);
  }
}