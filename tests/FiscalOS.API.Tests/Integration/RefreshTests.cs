namespace FiscalOS.API.Tests.Integration;

public class RefreshTests(TestApi testApi) : IntegrationTest(testApi)
{
  private static readonly Uri RefreshUri = new("/refresh", UriKind.Relative);

  [Fact]
  public async Task Refresh_WhenCalledWithNoAccessToken_ItShouldReturn401WithProblemDetails()
  {
    var response = await Client.PostAsync(RefreshUri, null, TestContext.Current.CancellationToken);

    await response.Should().BeProblemDetails(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task Refresh_WhenCalledWithNonExistentRefreshToken_ItShouldReturn400WithProblemDetails()
  {
    var jwt = JwtTokenBuilder.New()
      .WithClaim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString())
      .Build();

    using var request = new HttpRequestMessage(HttpMethod.Post, RefreshUri);
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
    request.Headers.Add("Cookie", "fiscalos_refresh_cookie=nonexistenttoken");

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeProblemDetails(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task Refresh_WhenCalledWithTokenBelongingToDifferentUser_ItShouldReturn403WithProblemDetails()
  {
    var (users, refreshToken) = await ExecuteDbContextAsync(static async (context, sp) =>
    {
      var passwordHasher = sp.GetRequiredService<IPasswordHasher>();
      var tokenGenerator = sp.GetRequiredService<ITokenGenerator>();
      var encryptor = sp.GetRequiredService<IEncryptor>();

      var user1EncryptionKey = await encryptor.GenerateEncryptedKeyAsync(TestContext.Current.CancellationToken);
      var user2EncryptionKey = await encryptor.GenerateEncryptedKeyAsync(TestContext.Current.CancellationToken);
      var user1 = User.From("User1", passwordHasher.Hash("@Password1"), user1EncryptionKey);
      var user2 = User.From("User2", passwordHasher.Hash("@Password2"), user2EncryptionKey);
      var refreshToken1 = tokenGenerator.GenerateRefreshToken(user2);
      var refreshToken2 = tokenGenerator.GenerateRefreshToken(user2);
      user2.AddRefreshToken(refreshToken1);
      user2.AddRefreshToken(refreshToken2);

      context.Add(user1);
      context.Add(user2);

      await context.SaveChangesAsync(TestContext.Current.CancellationToken);
      return (new User[] { user1, user2 }, refreshToken1);
    });

    var jwt = JwtTokenBuilder.New()
      .WithClaim(JwtRegisteredClaimNames.Sub, users[0].Id.ToString())
      .Build();

    using var request = new HttpRequestMessage(HttpMethod.Post, RefreshUri);
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
    request.Headers.Add("Cookie", $"fiscalos_refresh_cookie={refreshToken.Token}");

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeProblemDetails(HttpStatusCode.Forbidden);

    var unrevokedTokensCountForUser2 = await ExecuteDbContextAsync(
      async context => await context.Set<RefreshToken>()
        .Include(t => t.User)
        .Where(t => t.UserId == users[1].Id && t.Revoked == false)
        .CountAsync()
    );

    unrevokedTokensCountForUser2.Should().Be(0);
  }

  [Fact]
  public async Task Refresh_WhenCalledWithRevokedRefreshToken_ItShouldReturn400WithProblemDetails()
  {
    var (user, refreshToken) = await ExecuteDbContextAsync(static async (context, sp) =>
    {
      var passwordHasher = sp.GetRequiredService<IPasswordHasher>();
      var tokenGenerator = sp.GetRequiredService<ITokenGenerator>();
      var encryptor = sp.GetRequiredService<IEncryptor>();

      var userEncryptionKey = await encryptor.GenerateEncryptedKeyAsync(TestContext.Current.CancellationToken);
      var user = User.From("User1", passwordHasher.Hash("@Password1"), userEncryptionKey);
      var refreshToken = tokenGenerator.GenerateRefreshToken(user);
      refreshToken.Revoke();

      user.AddRefreshToken(refreshToken);

      context.Add(user);

      await context.SaveChangesAsync(TestContext.Current.CancellationToken);
      return (user, refreshToken);
    });

    var jwt = JwtTokenBuilder.New()
      .WithClaim(JwtRegisteredClaimNames.Sub, user.Id.ToString())
      .Build();

    using var request = new HttpRequestMessage(HttpMethod.Post, RefreshUri);
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
    request.Headers.Add("Cookie", $"fiscalos_refresh_cookie={refreshToken.Token}");

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeProblemDetails(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task Refresh_WhenCalledWithExpiredRefreshToken_ItShouldReturn400WithProblemDetails()
  {
    var (user, refreshToken) = await ExecuteDbContextAsync(static async (context, sp) =>
    {
      var passwordHasher = sp.GetRequiredService<IPasswordHasher>();
      var tokenGenerator = sp.GetRequiredService<ITokenGenerator>();
      var timeProvider = sp.GetRequiredService<TimeProvider>();
      var encryptor = sp.GetRequiredService<IEncryptor>();

      var userEncryptionKey = await encryptor.GenerateEncryptedKeyAsync(TestContext.Current.CancellationToken);
      var user = User.From("User1", passwordHasher.Hash("@Password1"), userEncryptionKey);
      var refreshToken = RefreshToken.From(user.Id, "expiredtoken", timeProvider.GetUtcNow().AddHours(-1));
      user.AddRefreshToken(refreshToken);

      context.Add(user);

      await context.SaveChangesAsync(TestContext.Current.CancellationToken);
      return (user, refreshToken);
    });

    var jwt = JwtTokenBuilder.New()
      .WithClaim(JwtRegisteredClaimNames.Sub, user.Id.ToString())
      .Build();

    using var request = new HttpRequestMessage(HttpMethod.Post, RefreshUri);
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
    request.Headers.Add("Cookie", $"fiscalos_refresh_cookie={refreshToken.Token}");

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeProblemDetails(HttpStatusCode.BadRequest);
  }

  [Theory]
  [InlineData(5)]
  [InlineData(-5)]
  public async Task Refresh_WhenCalledWithValidRefreshTokenAndExpiredOrNotExpiredAccessToken_ItShouldReturn200WithNewTokensAndSetRefreshCookie(int accessTokenExpiresAtOffset)
  {
    var (user, refreshToken) = await ExecuteDbContextAsync(static async (context, sp) =>
    {
      var passwordHasher = sp.GetRequiredService<IPasswordHasher>();
      var tokenGenerator = sp.GetRequiredService<ITokenGenerator>();
      var timeProvider = sp.GetRequiredService<TimeProvider>();
      var encryptor = sp.GetRequiredService<IEncryptor>();

      var userEncryptionKey = await encryptor.GenerateEncryptedKeyAsync(TestContext.Current.CancellationToken);
      var user = User.From("User1", passwordHasher.Hash("@Password1"), userEncryptionKey);
      var refreshToken = tokenGenerator.GenerateRefreshToken(user);
      user.AddRefreshToken(refreshToken);

      context.Add(user);

      await context.SaveChangesAsync(TestContext.Current.CancellationToken);
      return (user, refreshToken);
    });

    var jwt = JwtTokenBuilder.New()
      .WithClaim(JwtRegisteredClaimNames.Sub, user.Id.ToString())
      .WithExpiresAt(DateTime.UtcNow.AddMinutes(accessTokenExpiresAtOffset))
      .Build();

    using var request = new HttpRequestMessage(HttpMethod.Post, RefreshUri);
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
    request.Headers.Add("Cookie", $"fiscalos_refresh_cookie={refreshToken.Token}");

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    response.Should().HaveSetCookieHeader("fiscalos_refresh_cookie");
    await response.Should().BeJsonContentOfType<Refresh.Response>(HttpStatusCode.OK);

    var oldRefreshTokenInDb = await ExecuteDbContextAsync(
      async context => await context.Set<RefreshToken>()
        .Include(t => t.User)
        .Where(t => t.UserId == user.Id && t.Token == refreshToken.Token && t.Revoked == true)
        .SingleOrDefaultAsync()
    );

    oldRefreshTokenInDb.Should().NotBeNull();

    var newRefreshTokenInDb = await ExecuteDbContextAsync(
      async context => await context.Set<RefreshToken>()
        .Include(t => t.User)
        .Where(t => t.UserId == user.Id && t.Revoked == false && t.Token != refreshToken.Token)
        .SingleOrDefaultAsync()
    );

    newRefreshTokenInDb.Should().NotBeNull();
  }
}