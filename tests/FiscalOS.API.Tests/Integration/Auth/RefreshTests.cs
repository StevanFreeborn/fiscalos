namespace FiscalOS.API.Tests.Integration.Auth;

public class RefreshTests(TestApi testApi) : IntegrationTest(testApi)
{
  private static readonly Uri RefreshUri = new("/auth/refresh", UriKind.Relative);

  [Fact]
  public async Task Refresh_WhenCalledWithNoAccessToken_ItShouldReturn401WithProblemDetails()
  {
    var response = await Client.PostAsync(RefreshUri, null, TestContext.Current.CancellationToken);

    await response.Should().BeProblemDetails(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task Refresh_WhenCalledWithNonExistentRefreshToken_ItShouldReturn400WithProblemDetails()
  {
    using var request = HttpRequestBuilder.New()
      .Post(RefreshUri)
      .WithUserId(Guid.NewGuid())
      .WithRefreshCookie("nonexistenttoken")
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeProblemDetails(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task Refresh_WhenCalledWithTokenBelongingToDifferentUser_ItShouldReturn403WithProblemDetails()
  {
    var (users, refreshToken) = await ExecuteAsync(static async (context, ct, sp) =>
    {
      var passwordHasher = sp.GetRequiredService<IPasswordHasher>();
      var tokenGenerator = sp.GetRequiredService<ITokenGenerator>();
      var encryptor = sp.GetRequiredService<IEncryptor>();

      var user1EncryptionKey = await encryptor.GenerateEncryptedKeyAsync(ct);
      var user2EncryptionKey = await encryptor.GenerateEncryptedKeyAsync(ct);
      var user1 = User.From("User1", passwordHasher.Hash("@Password1"), user1EncryptionKey);
      var user2 = User.From("User2", passwordHasher.Hash("@Password2"), user2EncryptionKey);
      var refreshToken1 = tokenGenerator.GenerateRefreshToken(user2);
      var refreshToken2 = tokenGenerator.GenerateRefreshToken(user2);
      user2.AddRefreshToken(refreshToken1);
      user2.AddRefreshToken(refreshToken2);

      context.Add(user1);
      context.Add(user2);

      await context.SaveChangesAsync(ct);
      return (new User[] { user1, user2 }, refreshToken1);
    }, TestContext.Current.CancellationToken);

    using var request = HttpRequestBuilder.New()
      .Post(RefreshUri)
      .WithUserId(users[0].Id)
      .WithRefreshCookie(refreshToken.Token)
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeProblemDetails(HttpStatusCode.Forbidden);

    var unrevokedTokensCountForUser2 = await ExecuteAsync(
      async (context, ct) => await context.Set<RefreshToken>()
        .Include(t => t.User)
        .Where(t => t.UserId == users[1].Id && t.Revoked == false)
        .CountAsync(ct),
      TestContext.Current.CancellationToken
    );

    unrevokedTokensCountForUser2.Should().Be(0);
  }

  [Fact]
  public async Task Refresh_WhenCalledWithRevokedRefreshToken_ItShouldReturn400WithProblemDetails()
  {
    var (user, refreshToken) = await ExecuteAsync(static async (context, ct, sp) =>
    {
      var passwordHasher = sp.GetRequiredService<IPasswordHasher>();
      var tokenGenerator = sp.GetRequiredService<ITokenGenerator>();
      var encryptor = sp.GetRequiredService<IEncryptor>();

      var userEncryptionKey = await encryptor.GenerateEncryptedKeyAsync(ct);
      var user = User.From("User1", passwordHasher.Hash("@Password1"), userEncryptionKey);
      var refreshToken = tokenGenerator.GenerateRefreshToken(user);
      refreshToken.Revoke();

      user.AddRefreshToken(refreshToken);

      context.Add(user);

      await context.SaveChangesAsync(ct);
      return (user, refreshToken);
    }, TestContext.Current.CancellationToken);

    using var request = HttpRequestBuilder.New()
      .Post(RefreshUri)
      .WithUserId(user.Id)
      .WithRefreshCookie(refreshToken.Token)
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeProblemDetails(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task Refresh_WhenCalledWithExpiredRefreshToken_ItShouldReturn400WithProblemDetails()
  {
    var (user, refreshToken) = await ExecuteAsync(static async (context, ct, sp) =>
    {
      var passwordHasher = sp.GetRequiredService<IPasswordHasher>();
      var tokenGenerator = sp.GetRequiredService<ITokenGenerator>();
      var timeProvider = sp.GetRequiredService<TimeProvider>();
      var encryptor = sp.GetRequiredService<IEncryptor>();

      var userEncryptionKey = await encryptor.GenerateEncryptedKeyAsync(ct);
      var user = User.From("User1", passwordHasher.Hash("@Password1"), userEncryptionKey);
      var refreshToken = RefreshToken.From(user.Id, "expiredtoken", timeProvider.GetUtcNow().AddHours(-1));
      user.AddRefreshToken(refreshToken);

      context.Add(user);

      await context.SaveChangesAsync(ct);
      return (user, refreshToken);
    }, TestContext.Current.CancellationToken);

    using var request = HttpRequestBuilder.New()
      .Post(RefreshUri)
      .WithUserId(user.Id)
      .WithRefreshCookie(refreshToken.Token)
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeProblemDetails(HttpStatusCode.BadRequest);
  }

  [Theory]
  [InlineData(5)]
  [InlineData(-5)]
  public async Task Refresh_WhenCalledWithValidRefreshTokenAndExpiredOrNotExpiredAccessToken_ItShouldReturn200WithNewTokensAndSetRefreshCookie(int accessTokenExpiresAtOffset)
  {
    var (user, refreshToken) = await ExecuteAsync(static async (context, ct, sp) =>
    {
      var passwordHasher = sp.GetRequiredService<IPasswordHasher>();
      var tokenGenerator = sp.GetRequiredService<ITokenGenerator>();
      var timeProvider = sp.GetRequiredService<TimeProvider>();
      var encryptor = sp.GetRequiredService<IEncryptor>();

      var userEncryptionKey = await encryptor.GenerateEncryptedKeyAsync(ct);
      var user = User.From("User1", passwordHasher.Hash("@Password1"), userEncryptionKey);
      var refreshToken = tokenGenerator.GenerateRefreshToken(user);
      user.AddRefreshToken(refreshToken);

      context.Add(user);

      await context.SaveChangesAsync(ct);
      return (user, refreshToken);
    }, TestContext.Current.CancellationToken);

    using var request = HttpRequestBuilder.New()
      .Post(RefreshUri)
      .WithBearerToken(JwtTokenBuilder.New()
        .WithClaim(JwtRegisteredClaimNames.Sub, user.Id.ToString())
        .WithExpiresAt(DateTime.UtcNow.AddMinutes(accessTokenExpiresAtOffset))
        .Build())
      .WithRefreshCookie(refreshToken.Token)
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    response.Should().HaveSetCookieHeader("fiscalos_refresh_cookie");
    await response.Should().BeJsonContentOfType<API.Auth.Refresh.Response>(HttpStatusCode.OK);

    var oldRefreshTokenInDb = await ExecuteAsync(
      async (context, ct) => await context.Set<RefreshToken>()
        .Include(t => t.User)
        .Where(t => t.UserId == user.Id && t.Token == refreshToken.Token && t.Revoked == true)
        .SingleOrDefaultAsync(ct),
      TestContext.Current.CancellationToken
    );

    oldRefreshTokenInDb.Should().NotBeNull();

    var newRefreshTokenInDb = await ExecuteAsync(
      async (context, ct) => await context.Set<RefreshToken>()
        .Include(t => t.User)
        .Where(t => t.UserId == user.Id && t.Revoked == false && t.Token != refreshToken.Token)
        .SingleOrDefaultAsync(ct),
      TestContext.Current.CancellationToken
    );

    newRefreshTokenInDb.Should().NotBeNull();
  }
}