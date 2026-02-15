namespace FiscalOS.API.Institutions;

internal static class InstitutionsExtensions
{
  private const string RouteGroupPrefix = "/institutions";

  public static RouteGroupBuilder MapInstitutionsEndpoints(this WebApplication app)
  {
    var institutionsGroup = app.MapGroup(RouteGroupPrefix)
      .RequireAuthorization();

    institutionsGroup.MapConnectEndpoint();
    institutionsGroup.MapGetAvailableEndpoint();
    institutionsGroup.MapLinkEndpoint();

    return institutionsGroup;
  }
}