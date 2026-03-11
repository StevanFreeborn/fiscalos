using FiscalOS.API.Accounts.Add;
using FiscalOS.Core.Queuing;
using FiscalOS.Infra.Transactions.Plaid;

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
  public async Task Add_WhenCalledWithoutRequiredInformation_ItShouldReturn400WithProblemDetails()
  {
    using var request = HttpRequestBuilder.New()
      .Post(AddUri)
      .WithUserId(Guid.NewGuid())
      .WithBody(new { })
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeValidationProblemDetails(new Dictionary<string, string[]>()
    {
      [nameof(Request.ProviderInstitutionId)] = [$"The {nameof(Request.ProviderInstitutionId)} field is required."],
      [nameof(Request.ProviderAccountId)] = [$"The {nameof(Request.ProviderAccountId)} field is required."],
      [nameof(Request.ProviderAccountName)] = [$"The {nameof(Request.ProviderAccountName)} field is required."],
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
        providerAccountId = "accountId",
        providerAccountName = "Some Account",
      })
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeValidationProblemDetails(new Dictionary<string, string[]>()
    {
      [nameof(Request.ProviderInstitutionId)] = [$"The {nameof(Request.ProviderInstitutionId)} field is required."],
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
        providerInstitutionId = "institutionId",
        providerAccountName = "Some Account",
      })
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeValidationProblemDetails(new Dictionary<string, string[]>()
    {
      [nameof(Request.ProviderAccountId)] = [$"The {nameof(Request.ProviderAccountId)} field is required."],
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
        providerInstitutionId = "institutionId",
        providerAccountId = "accountId",
      })
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeValidationProblemDetails(new Dictionary<string, string[]>()
    {
      [nameof(Request.ProviderAccountName)] = [$"The {nameof(Request.ProviderAccountName)} field is required."],
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
        providerInstitutionId = "id",
        providerAccountId = "id",
        providerAccountName = "Some Account",
      })
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeProblemDetails(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task Add_WhenCalledWithPlaidInstitutionIdThatHasNotBeenAdded_ItShouldReturn400WithProblemDetails()
  {
    var user = await Api.ExecuteAsync(static async (context, ct, sp) =>
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
        providerInstitutionId = "id",
        providerAccountId = "id",
        providerAccountName = "Some Account",
      })
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeValidationProblemDetails(new Dictionary<string, string[]>()
    {
      [nameof(Request.ProviderInstitutionId)] = [$"The {nameof(Request.ProviderInstitutionId)} field is invalid."],
    });
  }

  [Fact]
  public async Task Add_WhenCalledWithPlaidAccountIdThatHasAlreadyBeenAdded_ItShouldReturn409WithProblemDetails()
  {
    var (user, institution, account) = await Api.ExecuteAsync(static async (context, ct, sp) =>
    {
      var passwordHasher = sp.GetRequiredService<IPasswordHasher>();
      var encryptor = sp.GetRequiredService<IEncryptor>();

      var userEncryptionKey = await encryptor.GenerateEncryptedKeyAsync(ct);
      var user = User.From("User1", passwordHasher.Hash("@Password1"), userEncryptionKey);

      var encryptedAccessToken = await encryptor.EncryptAsyncFor(user, "accessToken", ct);
      var plaidMetadata = PlaidInstitutionMetadata.From(
        "alreadyExists",
        "Some Bank",
        encryptedAccessToken,
        "itemId"
      );
      var institution = Institution.From("Some Bank", plaidMetadata);

      await context.AddAsync(institution, ct);

      var plaidAccountMetadata = PlaidAccountMetadata.From("accountId", "Some Account");
      var account = Account.From("Some Account", plaidAccountMetadata);

      institution.AddAccount(account);
      user.AddInstitution(institution);
      user.AddAccount(account);

      await context.AddAsync(user, ct);
      await context.SaveChangesAsync(ct);
      return (user, institution, account);
    }, TestContext.Current.CancellationToken);

    using var request = HttpRequestBuilder.New()
      .Post(AddUri)
      .WithUserId(user.Id)
      .WithBody(new
      {
        providerInstitutionId = ((PlaidInstitutionMetadata)institution.Metadata!).PlaidId,
        providerAccountId = ((PlaidAccountMetadata)account.Metadata!).PlaidId,
        providerAccountName = ((PlaidAccountMetadata)account.Metadata).PlaidName,
      })
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeProblemDetails(HttpStatusCode.Conflict);
  }

  [Fact]
  public async Task Add_WhenCalledWithNewAccount_ItShouldReturn200()
  {
    var mockQueue = new Mock<IAsyncQueue<SyncUpdatesQueueItem>>();

    await using var testApi = Api
      .WithAdditionalConfig(whb =>
      {
        whb.ConfigureTestServices(s =>
        {
          s.AddSingleton(mockQueue.Object);
        });
      });

    var (user, institution) = await testApi.ExecuteAsync(async (context, ct, sp) =>
    {
      var passwordHasher = sp.GetRequiredService<IPasswordHasher>();
      var encryptor = sp.GetRequiredService<IEncryptor>();
      var plaidClient = sp.GetRequiredService<PlaidClient>();

      var userEncryptionKey = await encryptor.GenerateEncryptedKeyAsync(ct);
      var user = User.From("User1", passwordHasher.Hash("@Password1"), userEncryptionKey);

      var encryptedAccessToken = await encryptor.EncryptAsyncFor(user, "accessToken", ct);
      var plaidMetadata = PlaidInstitutionMetadata.From(
        "id",
        "Some Bank",
        encryptedAccessToken,
        "itemId"
      );
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
        providerInstitutionId = ((PlaidInstitutionMetadata)institution.Metadata!).PlaidId,
        providerAccountId = newAccountId,
        providerAccountName = newAccountName,
      })
      .Build();

    var client = testApi.CreateClient();

    var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var updatedUser = await testApi.ExecuteAsync(
      async (context, ct) => await context.Set<User>()
        .Include(u => u.Accounts)
        .ThenInclude(a => a.Metadata)
        .Include(u => u.Accounts)
        .ThenInclude(a => a.Balances)
        .AsSplitQuery()
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

    mockQueue.Verify(m => m.EnqueueAsync(
      It.IsAny<SyncUpdatesQueueItem>(),
      It.IsAny<CancellationToken>()
    ), Times.Once());
  }
}