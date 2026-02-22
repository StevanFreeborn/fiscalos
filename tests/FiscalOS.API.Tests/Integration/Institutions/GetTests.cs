using FiscalOS.API.Institutions.Get;

using Institution = FiscalOS.Core.Accounts.Institution;

namespace FiscalOS.API.Tests.Integration.Institutions;

public class GetTests(TestApi testApi) : IntegrationTest(testApi)
{
  private static readonly Uri GetUri = new("/institutions", UriKind.Relative);

  [Fact]
  public async Task Get_WhenCalledWithoutValidToken_ItShouldReturn401WithProblemDetails()
  {
    using var request = HttpRequestBuilder.New()
      .Get(GetUri)
      .Build();

    var res = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await res.Should().BeProblemDetails(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task Get_WhenCalledByNonExistentUser_ItShouldReturn401WithProblemDetails()
  {
    using var request = HttpRequestBuilder.New()
      .Get(GetUri)
      .WithUserId(Guid.NewGuid())
      .Build();

    var res = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await res.Should().BeProblemDetails(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task Get_WhenCalledByUser_ItShouldReturn200WithListOfInstitutions()
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
      .Get(GetUri)
      .WithUserId(user.Id)
      .Build();

    var res = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    (await res.Should().BeJsonContentOfType<Response>(HttpStatusCode.OK))
      .Which.Institutions.Should().BeEquivalentTo(
      [
        InstitutionDto.FromInstitution(institution),
      ]);
  }
}