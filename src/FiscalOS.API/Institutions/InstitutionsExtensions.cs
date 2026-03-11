using FiscalOS.API.Institutions.Connect;
using FiscalOS.API.Institutions.Get;
using FiscalOS.API.Institutions.GetAvailable;
using FiscalOS.API.Institutions.Link;

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
    institutionsGroup.MapGetEndpoint();

    return institutionsGroup;
  }
}