using FiscalOS.API.Institutions.GetAvailable;

using Institution = FiscalOS.Core.Accounts.Institution;

namespace FiscalOS.API.Tests.Integration.Institutions;

public class GetAvailableTests(TestApi testApi) : IntegrationTest(testApi)
{
  private static Uri GetAvailableUri(Guid id)
  {
    return new($"/institutions/{id}/available", UriKind.Relative);
  }

  [Fact]
  public async Task GetAvailable_WhenCalledAndUnauthenticated_ItShouldReturn401WithProblemDetails()
  {
    using var request = HttpRequestBuilder.New()
      .WithUri(GetAvailableUri(Guid.NewGuid()))
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeProblemDetails(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetAvailable_WhenCalledByNonExistentUser_ItShouldReturn401WithProblemDetails()
  {
    using var request = HttpRequestBuilder.New()
      .WithUri(GetAvailableUri(Guid.NewGuid()))
      .WithUserId(Guid.NewGuid())
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeProblemDetails(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetAvailable_WhenCalledWithNonExistentInstitutionId_ItShouldReturn404WithProblemDetails()
  {
    var user = await ExecuteAsync(static async (context, ct, sp) =>
    {
      var passwordHasher = sp.GetRequiredService<IPasswordHasher>();
      var encryptor = sp.GetRequiredService<IEncryptor>();

      var userEncryptionKey = await encryptor.GenerateEncryptedKeyAsync(ct);
      var user = User.From("Stevan", passwordHasher.Hash("@Password1"), userEncryptionKey);

      context.Add(user);
      await context.SaveChangesAsync(ct);
      return user;
    }, TestContext.Current.CancellationToken);

    using var request = HttpRequestBuilder.New()
      .WithUri(GetAvailableUri(Guid.NewGuid()))
      .WithUserId(user.Id)
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeProblemDetails(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task GetAvailable_WhenCalledWithConnectedInstitution_ItShouldReturn200WithAvailableAccounts()
  {
    var (user, institution, expectedAccounts) = await ExecuteAsync(static async (context, ct, sp) =>
    {
      var passwordHasher = sp.GetRequiredService<IPasswordHasher>();
      var encryptor = sp.GetRequiredService<IEncryptor>();
      var plaidClient = sp.GetRequiredService<PlaidClient>();

      var userEncryptionKey = await encryptor.GenerateEncryptedKeyAsync(ct);
      var user = User.From("Stevan", passwordHasher.Hash("@Password1"), userEncryptionKey);

      var institutionId = "ins_109508";

      var publicTokenResponse = await plaidClient.SandboxPublicTokenCreateAsync(new()
      {
        InstitutionId = institutionId,
        InitialProducts = [Products.Transactions],
      });

      var exchangeTokenResponse = await plaidClient.ItemPublicTokenExchangeAsync(new()
      {
        PublicToken = publicTokenResponse.PublicToken,
      });

      var accountsResponse = await plaidClient.AccountsGetAsync(new()
      {
        AccessToken = exchangeTokenResponse.AccessToken,
      });
      var encryptedAccessToken = await encryptor.EncryptAsyncFor(user, exchangeTokenResponse.AccessToken, ct);
      var institutionMetadata = PlaidMetadata.From(institutionId, "Some Bank", encryptedAccessToken);
      var institution = Institution.From("Some Bank", institutionMetadata);

      user.AddInstitution(institution);

      context.Add(user);
      context.Add(institution);
      await context.SaveChangesAsync(ct);
      return (user, institution, accountsResponse.Accounts);
    }, TestContext.Current.CancellationToken);

    using var request = HttpRequestBuilder.New()
      .WithUri(GetAvailableUri(institution.Id))
      .WithUserId(user.Id)
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    (await response.Should().BeJsonContentOfType<Response>(HttpStatusCode.OK))
      .Which.Accounts.Should().BeEquivalentTo(expectedAccounts.Select(AvailableAccountDto.FromPlaidAccount));
  }
}