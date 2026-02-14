using Account = FiscalOS.Core.Accounts.Account;
using Institution = FiscalOS.Core.Accounts.Institution;

namespace FiscalOS.API.Tests.Integration.Accounts;

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
  public async Task Add_WhenCalledWithoutInstitutionIdOrAccountIdOrAccountName_ItShouldReturn400WithProblemDetails()
  {
    using var request = HttpRequestBuilder.New()
      .Post(AddUri)
      .WithUserId(Guid.NewGuid())
      .WithBody(new { })
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeValidationProblemDetails(new Dictionary<string, string[]>()
    {
      ["PlaidInstitutionId"] = ["The PlaidInstitutionId field is required."],
      ["PlaidAccountId"] = ["The PlaidAccountId field is required."],
      ["PlaidAccountName"] = ["The PlaidAccountName field is required."],
    });
  }

  [Fact]
  public async Task Add_WhenCalledWithoutInstitutionId_ItShouldReturn400WithProblemDetails()
  {
    using var request = HttpRequestBuilder.New()
      .Post(AddUri)
      .WithUserId(Guid.NewGuid())
      .WithBody(new
      {
        plaidAccountId = "accountId",
        plaidAccountName = "Some Account",
      })
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeValidationProblemDetails(new Dictionary<string, string[]>()
    {
      ["PlaidInstitutionId"] = ["The PlaidInstitutionId field is required."],
    });
  }

  [Fact]
  public async Task Add_WhenCalledWithoutAccountId_ItShouldReturn400WithProblemDetails()
  {
    using var request = HttpRequestBuilder.New()
      .Post(AddUri)
      .WithUserId(Guid.NewGuid())
      .WithBody(new
      {
        plaidInstitutionId = "institutionId",
        plaidAccountName = "Some Account",
      })
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeValidationProblemDetails(new Dictionary<string, string[]>()
    {
      ["PlaidAccountId"] = ["The PlaidAccountId field is required."],
    });
  }

  [Fact]
  public async Task Add_WhenCalledWithoutAccountName_ItShouldReturn400WithProblemDetails()
  {
    using var request = HttpRequestBuilder.New()
      .Post(AddUri)
      .WithUserId(Guid.NewGuid())
      .WithBody(new
      {
        plaidInstitutionId = "institutionId",
        plaidAccountId = "accountId",
      })
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeValidationProblemDetails(new Dictionary<string, string[]>()
    {
      ["PlaidAccountName"] = ["The PlaidAccountName field is required."],
    });
  }

  [Fact]
  public async Task Add_WhenCalledWithNonExistentUser_ItShouldReturn401WithProblemDetails()
  {
    using var request = HttpRequestBuilder.New()
      .Post(AddUri)
      .WithUserId(Guid.NewGuid())
      .WithBody(new
      {
        plaidInstitutionId = "id",
        plaidAccountId = "id",
        plaidAccountName = "Some Account",
      })
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeProblemDetails(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task Add_WhenCalledWithPlaidInstitutionIdThatHasNotBeenAdded_ItShouldReturn400WithProblemDetails()
  {
    var user = await ExecuteAsync(static async (context, ct, sp) =>
    {
      var passwordHasher = sp.GetRequiredService<IPasswordHasher>();
      var encryptor = sp.GetRequiredService<IEncryptor>();

      var userEncryptionKey = await encryptor.GenerateEncryptedKeyAsync(ct);
      var user = User.From("User1", passwordHasher.Hash("@Password1"), userEncryptionKey);


      await context.AddAsync(user, ct);
      await context.SaveChangesAsync(ct);
      return user;
    }, TestContext.Current.CancellationToken);

    using var request = HttpRequestBuilder.New()
      .Post(AddUri)
      .WithUserId(user.Id)
      .WithBody(new
      {
        plaidInstitutionId = "id",
        plaidAccountId = "id",
        plaidAccountName = "Some Account",
      })
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeValidationProblemDetails(new Dictionary<string, string[]>()
    {
      ["PlaidInstitutionId"] = ["The PlaidInstitutionId field is invalid. No institution connected with the given PlaidInstitutionId was found for the user."],
    });
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

    using var request = HttpRequestBuilder.New()
      .Post(AddUri)
      .WithUserId(user.Id)
      .WithBody(new
      {
        plaidInstitutionId = ((PlaidMetadata)institution.Metadata!).PlaidId,
        plaidAccountId = ((PlaidAccountMetadata)account.Metadata!).PlaidId,
        plaidAccountName = ((PlaidAccountMetadata)account.Metadata).PlaidName,
      })
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeProblemDetails(HttpStatusCode.Conflict);
  }

  [Fact]
  public async Task Add_WhenCalledWithNewAccount_ItShouldReturn200()
  {
    var (user, institution) = await ExecuteAsync(async (context, ct, sp) =>
    {
      var passwordHasher = sp.GetRequiredService<IPasswordHasher>();
      var encryptor = sp.GetRequiredService<IEncryptor>();
      var plaidClient = sp.GetRequiredService<PlaidClient>();

      var userEncryptionKey = await encryptor.GenerateEncryptedKeyAsync(ct);
      var user = User.From("User1", passwordHasher.Hash("@Password1"), userEncryptionKey);

      var encryptedAccessToken = await encryptor.EncryptAsyncFor(user, "accessToken", ct);
      var plaidMetadata = PlaidMetadata.From("id", "Some Bank", encryptedAccessToken);
      var institution = Institution.From("Some Bank", plaidMetadata);

      user.AddInstitution(institution);

      await context.AddAsync(user, ct);
      await context.SaveChangesAsync(ct);
      return (user, institution);
    }, TestContext.Current.CancellationToken);

    var newAccountId = "newAccountId";
    var newAccountName = "New Account";

    using var request = HttpRequestBuilder.New()
      .Post(AddUri)
      .WithUserId(user.Id)
      .WithBody(new
      {
        plaidInstitutionId = ((PlaidMetadata)institution.Metadata!).PlaidId,
        plaidAccountId = newAccountId,
        plaidAccountName = newAccountName,
      })
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var updatedUser = await ExecuteAsync(
      async (context, ct) => await context.Set<User>()
        .Include(u => u.Accounts)
        .ThenInclude(a => a.Metadata)
        .FirstAsync(u => u.Id == user.Id, ct),
      TestContext.Current.CancellationToken
    );

    updatedUser.Accounts.Should().ContainSingle(
      a => a.Name == newAccountName &&
        a.InstitutionId == institution.Id &&
        a.Metadata is PlaidAccountMetadata &&
        ((PlaidAccountMetadata)a.Metadata).PlaidId == newAccountId &&
        ((PlaidAccountMetadata)a.Metadata).PlaidName == newAccountName
    );
  }
}