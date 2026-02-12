namespace FiscalOS.API.Accounts.Connect;

internal static class Endpoint
{
  private const string Route = "/connect";

  public static RouteHandlerBuilder MapConnectEndpoint(this RouteGroupBuilder groupBuilder)
  {
    return groupBuilder.MapPost(Route, HandleAsync);
  }

  private static async Task<IResult> HandleAsync(
    [FromBody] Request request
  )
  {
    return Results.Ok();
  }
}