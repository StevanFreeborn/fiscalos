using Institution = FiscalOS.Core.Accounts.Institution;

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

  [Fact]
  public async Task Connect_WhenCalledWithNonExistentUser_ItShouldReturn401WithProblemDetails()
  {
    var jwt = JwtTokenBuilder.New()
      .WithClaim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString())
      .Build();

    var json = JsonSerializer.Serialize(new
    {
      publicToken = "token",
      plaidInstitutionId = "id",
    });
    using var content = new StringContent(json, Encoding.UTF8, "application/json");
    using var request = new HttpRequestMessage(HttpMethod.Post, ConnectUri)
    {
      Content = content,
    };
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeProblemDetails(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task Connect_WhenCalledWithPlaidInstitutionIdThatIsAlreadyConnected_ItShouldReturn409WithProblemDetails()
  {
    var (user, institution) = await ExecuteAsync(static async (context, ct, sp) =>
    {
      var passwordHasher = sp.GetRequiredService<IPasswordHasher>();
      var encryptor = sp.GetRequiredService<IEncryptor>();

      var userEncryptionKey = await encryptor.GenerateEncryptedKeyAsync(ct);
      var user = User.From("User1", passwordHasher.Hash("@Password1"), userEncryptionKey);

      var encryptedAccessToken = await encryptor.EncryptAsyncFor(user, "accessToken", ct);
      var plaidMetadata = PlaidMetadata.From("alreadyExists", "Some Bank", encryptedAccessToken);
      var institution = Institution.From("Some Bank", plaidMetadata);

      user.AddInstitution(institution);

      await context.AddAsync(user, ct);
      await context.SaveChangesAsync(ct);
      return (user, institution);
    }, TestContext.Current.CancellationToken);

    var jwt = JwtTokenBuilder.New()
      .WithClaim(JwtRegisteredClaimNames.Sub, user.Id.ToString())
      .Build();

    var json = JsonSerializer.Serialize(new
    {
      publicToken = "token",
      plaidInstitutionId = ((PlaidMetadata)institution.Metadata).PlaidId,
    });
    using var content = new StringContent(json, Encoding.UTF8, "application/json");
    using var request = new HttpRequestMessage(HttpMethod.Post, ConnectUri)
    {
      Content = content,
    };
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeProblemDetails(HttpStatusCode.Conflict);
  }

  [Fact]
  public async Task Connect_WhenCalledWithInstitutionThatIsNotConnected_ItShouldReturn200()
  {
    var plaidInstitutionId = "ins_109508";

    var (user, publicToken) = await ExecuteAsync(async (context, ct, sp) =>
    {
      var plaidClient = sp.GetRequiredService<PlaidClient>();
      var passwordHasher = sp.GetRequiredService<IPasswordHasher>();
      var encryptor = sp.GetRequiredService<IEncryptor>();

      var userEncryptionKey = await encryptor.GenerateEncryptedKeyAsync(ct);
      var user = User.From("User1", passwordHasher.Hash("@Password1"), userEncryptionKey);

      await context.AddAsync(user, ct);
      await context.SaveChangesAsync(ct);

      var publicToken = await plaidClient.SandboxPublicTokenCreateAsync(new()
      {
        InstitutionId = plaidInstitutionId,
        InitialProducts = [Products.Transactions],
      });

      return (user, publicToken.PublicToken);
    }, TestContext.Current.CancellationToken);

    var jwt = JwtTokenBuilder.New()
      .WithClaim(JwtRegisteredClaimNames.Sub, user.Id.ToString())
      .Build();

    var json = JsonSerializer.Serialize(new
    {
      publicToken,
      plaidInstitutionId,
    });
    using var content = new StringContent(json, Encoding.UTF8, "application/json");
    using var request = new HttpRequestMessage(HttpMethod.Post, ConnectUri)
    {
      Content = content,
    };
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var updatedUser = await ExecuteAsync(
      async (context, ct) => await context.Set<User>()
        .Include(u => u.Institutions)
        .ThenInclude(i => i.Metadata)
        .SingleAsync(u => u.Id == user.Id, ct),
      TestContext.Current.CancellationToken
    );

    updatedUser.Institutions.Should().HaveCount(1);

    var institution = updatedUser.Institutions.First();
    institution.Name.Should().NotBeNullOrEmpty();

    var metadata = institution.Metadata.As<PlaidMetadata>();
    metadata.PlaidId.Should().Be(plaidInstitutionId);
    metadata.PlaidName.Should().Be(institution.Name);
    metadata.EncryptedAccessToken.Should().NotBeNullOrEmpty();
  }
}