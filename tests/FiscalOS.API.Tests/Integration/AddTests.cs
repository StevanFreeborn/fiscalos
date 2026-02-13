using Institution = FiscalOS.Core.Accounts.Institution;
using Account = FiscalOS.Core.Accounts.Account;

namespace FiscalOS.API.Tests.Integration;

public class AddTests(TestApi testApi) : IntegrationTest(testApi)
{
  private static readonly Uri AddUri = new("/accounts", UriKind.Relative);

  [Fact]
  public async Task Add_WhenCalledWhenNotLoggedIn_ItShouldReturn401WithProblemDetails()
  {
    var response = await Client.PostAsync(AddUri, null, TestContext.Current.CancellationToken);

    await response.Should().BeProblemDetails(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task Add_WhenCalledWithoutInstitutionIdOrAccountId_ItShouldReturn400WithProblemDetails()
  {
    var jwt = JwtTokenBuilder.New()
      .WithClaim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString())
      .Build();

    using var content = new StringContent(JsonSerializer.Serialize(new { }), Encoding.UTF8, "application/json");
    using var request = new HttpRequestMessage(HttpMethod.Post, AddUri)
    {
      Content = content
    };
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeValidationProblemDetails(new Dictionary<string, string[]>()
    {
      ["PlaidInstitutionId"] = ["The PlaidInstitutionId field is required."],
      ["PlaidAccountId"] = ["The PlaidAccountId field is required."],
    });
  }

  [Fact]
  public async Task Add_WhenCalledWithoutInstitutionId_ItShouldReturn400WithProblemDetails()
  {
    var jwt = JwtTokenBuilder.New()
      .WithClaim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString())
      .Build();

    using var content = new StringContent(JsonSerializer.Serialize(new { plaidAccountId = "accountId" }), Encoding.UTF8, "application/json");
    using var request = new HttpRequestMessage(HttpMethod.Post, AddUri)
    {
      Content = content
    };
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeValidationProblemDetails(new Dictionary<string, string[]>()
    {
      ["PlaidInstitutionId"] = ["The PlaidInstitutionId field is required."],
    });
  }

  [Fact]
  public async Task Add_WhenCalledWithoutAccountId_ItShouldReturn400WithProblemDetails()
  {
    var jwt = JwtTokenBuilder.New()
      .WithClaim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString())
      .Build();

    using var content = new StringContent(JsonSerializer.Serialize(new { plaidInstitutionId = "institutionId" }), Encoding.UTF8, "application/json");
    using var request = new HttpRequestMessage(HttpMethod.Post, AddUri)
    {
      Content = content
    };
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeValidationProblemDetails(new Dictionary<string, string[]>()
    {
      ["PlaidAccountId"] = ["The PlaidAccountId field is required."],
    });
  }

  [Fact]
  public async Task Add_WhenCalledWithNonExistentUser_ItShouldReturn401WithProblemDetails()
  {
    var jwt = JwtTokenBuilder.New()
      .WithClaim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString())
      .Build();

    var json = JsonSerializer.Serialize(new
    {
      plaidInstitutionId = "id",
      plaidAccountId = "id",
    });
    using var content = new StringContent(json, Encoding.UTF8, "application/json");
    using var request = new HttpRequestMessage(HttpMethod.Post, AddUri)
    {
      Content = content,
    };
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeProblemDetails(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task Add_WhenCalledWithPlaidAccountIdThatHasAlreadyBeenAdded_ItShouldReturn409WithProblemDetails()
  {
    var (user, institution, account) = await ExecuteAsync(static async (context, ct, sp) =>
    {
      var passwordHasher = sp.GetRequiredService<IPasswordHasher>();
      var encryptor = sp.GetRequiredService<IEncryptor>();

      var userEncryptionKey = await encryptor.GenerateEncryptedKeyAsync(ct);
      var user = User.From("User1", passwordHasher.Hash("@Password1"), userEncryptionKey);

      await context.AddAsync(user, ct);

      var encryptedAccessToken = await encryptor.EncryptAsyncFor(user, "accessToken", ct);
      var plaidMetadata = PlaidMetadata.From("alreadyExists", "Some Bank", encryptedAccessToken);
      var institution = Institution.From("Some Bank", plaidMetadata);

      await context.AddAsync(institution, ct);

      var plaidAccountMetadata = PlaidAccountMetadata.From("accountId", "Some Account");
      var account = Account.From(institution.Id, "Some Account", plaidAccountMetadata);

      user.AddInstitution(institution);
      user.AddAccount(account);

      await context.SaveChangesAsync(ct);
      return (user, institution, account);
    }, TestContext.Current.CancellationToken);

    var jwt = JwtTokenBuilder.New()
      .WithClaim(JwtRegisteredClaimNames.Sub, user.Id.ToString())
      .Build();

    var json = JsonSerializer.Serialize(new
    {
      plaidInstitutionId = ((PlaidMetadata)institution.Metadata).PlaidId,
      plaidAccountId = ((PlaidAccountMetadata)account.Metadata).PlaidId,
    });
    using var content = new StringContent(json, Encoding.UTF8, "application/json");
    using var request = new HttpRequestMessage(HttpMethod.Post, AddUri)
    {
      Content = content,
    };
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeProblemDetails(HttpStatusCode.Conflict);
  }
}