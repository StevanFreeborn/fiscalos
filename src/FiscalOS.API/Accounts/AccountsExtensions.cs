namespace FiscalOS.API.Accounts;

internal static class AccountsExtensions
{
  private const string RouteGroupPrefix = "/accounts";

  public static RouteGroupBuilder MapAccountsEndpoints(this WebApplication app)
  {
    var accountsGroup = app.MapGroup(RouteGroupPrefix).RequireAuthorization();

    accountsGroup.MapConnectEndpoint();

    return accountsGroup;
  }
}