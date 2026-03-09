using FiscalOS.API.Auth.Login;

namespace FiscalOS.API.Tests.Integration.Auth;

public class LogoutTests(TestApi testApi) : IntegrationTest(testApi)
{
  private static readonly Uri LogoutUri = new("/auth/logout", UriKind.Relative);
  private static readonly Uri LoginUri = new("/auth/login", UriKind.Relative);

  [Fact]
  public async Task Logout_WhenCalled_ItShouldExpireRefreshTokenCookie()
  {
    var user = await Api.ExecuteAsync(static async (context, ct, sp) =>
    {
      var passwordHasher = sp.GetRequiredService<IPasswordHasher>();
      var encryptor = sp.GetRequiredService<IEncryptor>();

      var userEncryptionKey = await encryptor.GenerateEncryptedKeyAsync(ct);
      var user = User.From("Stevan", passwordHasher.Hash("@Password1"), userEncryptionKey);
      context.Add(user);

      await context.SaveChangesAsync(ct);
      return user;
    }, TestContext.Current.CancellationToken);

    using var loginRequest = HttpRequestBuilder.New()
      .Post(LoginUri)
      .WithBody(new
      {
        username = "Stevan",
        password = "@Password1",
      })
      .Build();

    var loginResponse = await Client.SendAsync(loginRequest, TestContext.Current.CancellationToken);
    var setCookieHeaders = loginResponse.Headers.GetValues("Set-Cookie");
    var refreshTokenCookieHeader = setCookieHeaders.First(h => h.StartsWith("fiscalos_refresh_cookie", StringComparison.OrdinalIgnoreCase));
    var refreshTokenCookieParts = refreshTokenCookieHeader.Trim().Split("=");

    var accessToken = (await loginResponse.Should().BeJsonContentOfType<Response>(HttpStatusCode.OK))
      .Which
      .AccessToken;

    var userRefreshTokens = await Api.ExecuteAsync(async (context, ct) =>
    {
      return await context.Set<RefreshToken>()
      .Where(r => r.UserId == user.Id)
      .ToListAsync(ct);
    }, TestContext.Current.CancellationToken);

    userRefreshTokens.Should()
      .ContainSingle(r => r.IsExpired(DateTimeOffset.UtcNow) == false && r.Revoked == false);

    using var logoutRequest = HttpRequestBuilder.New()
      .Post(LogoutUri)
      .WithBearerToken(accessToken)
      .WithCookie(refreshTokenCookieParts[0], refreshTokenCookieParts[1])
      .Build();

    var logoutResponse = await Client.SendAsync(logoutRequest, TestContext.Current.CancellationToken);

    logoutResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

    logoutResponse.Should().HaveSetCookieHeader("fiscalos_refresh_cookie");

    var updatedUserRefreshTokens = await Api.ExecuteAsync(async (context, ct) =>
    {
      return await context.Set<RefreshToken>()
      .Where(r => r.UserId == user.Id)
      .ToListAsync(ct);
    }, TestContext.Current.CancellationToken);

    updatedUserRefreshTokens.Should()
      .ContainSingle(r => r.Revoked && r.IsExpired(DateTimeOffset.UtcNow) == false);
  }
}