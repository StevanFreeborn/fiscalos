namespace FiscalOS.API.Login;

internal static class Endpoint
{
  private const string Route = "/login";

  public static RouteHandlerBuilder MapLoginEndpoint(this WebApplication app)
  {
    return app.MapPost(Route, HandleAsync);
  }

  private static async Task<IResult> HandleAsync([FromBody] LoginRequest loginRequest)
  {
    Console.WriteLine(loginRequest.Username);
    Console.WriteLine(loginRequest.Password);
    return Results.Ok();
  }
}