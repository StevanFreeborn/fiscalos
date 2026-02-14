using Institution = FiscalOS.Core.Accounts.Institution;

namespace FiscalOS.API.Tests.Integration.Institutions;

public class ConnectTests(TestApi testApi) : IntegrationTest(testApi)
{
  private static readonly Uri ConnectUri = new("/institutions/connect", UriKind.Relative);

  [Fact]
  public async Task Connect_WhenCalledWithoutValidToken_ItShouldReturn401WithProblemDetails()
  {
    using var request = HttpRequestBuilder.New()
      .Post(ConnectUri)
      .WithBody(new { })
      .Build();

    var res = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await res.Should().BeProblemDetails(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task Connect_WhenCalledWithoutPublicTokenOrPlaidInstitutionId_ItShouldReturn400WithProblemDetails()
  {
    using var request = HttpRequestBuilder.New()
      .Post(ConnectUri)
      .WithUserId(Guid.NewGuid())
      .WithBody(new { })
      .Build();

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
    using var request = HttpRequestBuilder.New()
      .Post(ConnectUri)
      .WithUserId(Guid.NewGuid())
      .WithBody(new { plaidInstitutionId = "id" })
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeValidationProblemDetails(new Dictionary<string, string[]>()
    {
      ["PublicToken"] = ["The PublicToken field is required."],
    });
  }

  [Fact]
  public async Task Connect_WhenCalledWithoutPlaidInstitutionId_ItShouldReturn400WithProblemDetails()
  {
    using var request = HttpRequestBuilder.New()
      .Post(ConnectUri)
      .WithUserId(Guid.NewGuid())
      .WithBody(new { publicToken = "token" })
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeValidationProblemDetails(new Dictionary<string, string[]>()
    {
      ["PlaidInstitutionId"] = ["The PlaidInstitutionId field is required."],
    });
  }

  [Fact]
  public async Task Connect_WhenCalledWithNonExistentUser_ItShouldReturn401WithProblemDetails()
  {
    using var request = HttpRequestBuilder.New()
      .Post(ConnectUri)
      .WithUserId(Guid.NewGuid())
      .WithBody(new
      {
        publicToken = "token",
        plaidInstitutionId = "id",
      })
      .Build();

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

    using var request = HttpRequestBuilder.New()
      .Post(ConnectUri)
      .WithUserId(user.Id)
      .WithBody(new
      {
        publicToken = "token",
        plaidInstitutionId = ((PlaidMetadata)institution.Metadata!).PlaidId,
      })
      .Build();

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

      var publicTokenResponse = await plaidClient.SandboxPublicTokenCreateAsync(new()
      {
        InstitutionId = plaidInstitutionId,
        InitialProducts = [Products.Transactions],
      });

      return (user, publicTokenResponse.PublicToken);
    }, TestContext.Current.CancellationToken);

    using var request = HttpRequestBuilder.New()
      .Post(ConnectUri)
      .WithUserId(user.Id)
      .WithBody(new
      {
        publicToken,
        plaidInstitutionId,
      })
      .Build();

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