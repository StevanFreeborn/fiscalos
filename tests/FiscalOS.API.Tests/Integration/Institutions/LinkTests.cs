namespace FiscalOS.API.Tests.Integration.Institutions;

public class LinkTests(TestApi testApi) : IntegrationTest(testApi)
{
  private static readonly Uri LinkUri = new("/institutions/link", UriKind.Relative);

  [Fact]
  public async Task Link_WhenCalledAndUnauthenticated_ItShouldReturn401WithProblemDetails()
  {
    using var request = HttpRequestBuilder.New()
      .Post(LinkUri)
      .Build();

    var res = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await res.Should().BeProblemDetails(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task Link_WhenCalledWithNonExistentUser_ItShouldReturn401WithProblemDetails()
  {
    using var request = HttpRequestBuilder.New()
      .Post(LinkUri)
      .WithUserId(Guid.NewGuid())
      .Build();

    var res = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await res.Should().BeProblemDetails(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task Link_WhenCalledWithUserWhoExists_ItShouldReturnLinkToken()
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
      .Post(LinkUri)
      .WithUserId(user.Id)
      .Build();

    var res = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    (await res.Should().BeJsonContentOfType<API.Institutions.Link.Response>(HttpStatusCode.OK))
      .Which.LinkToken.Should().NotBeEmpty();
  }
}