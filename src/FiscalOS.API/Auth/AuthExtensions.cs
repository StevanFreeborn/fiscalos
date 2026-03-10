using FiscalOS.API.Auth.Login;
using FiscalOS.API.Auth.Logout;
using FiscalOS.API.Auth.Refresh;

namespace FiscalOS.API.Auth;

internal static class AuthExtensions
{
  private const string RouteGroupPrefix = "/auth";

  public static RouteGroupBuilder MapAuthEndpoints(this WebApplication app)
  {
    var authGroup = app.MapGroup(RouteGroupPrefix);

    authGroup.MapLoginEndpoint();
    authGroup.MapLogoutEndpoint();

    authGroup.MapRefreshEndpoint()
      .RequireAuthorization(Schemes.AllowExpiredTokens);

    return authGroup;
  }
}