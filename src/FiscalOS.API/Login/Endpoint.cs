namespace FiscalOS.API.Login;

internal static class Endpoint
{
  private const string Route = "/login";

  public static RouteHandlerBuilder MapLoginEndpoint(this WebApplication app)
  {
    return app.MapPost(Route, HandleAsync);
  }

  private static async Task<IResult> HandleAsync(
    [FromBody] LoginRequest loginRequest,
    [FromServices] AppDbContext appDbContext
  )
  {
    const string ADMIN_USERNAME = "Stevan";
    const string ADMIN_PASSWORD = "@Password1";

    // TODO: Implement actual auth flow
    // 1. We want to make sure that
    //    the user exists
    // 2. We want to make sure that
    //    tha the password is correct
    // 3. We want to issue an access token
    //    with a refresh token
    // 4. We want to store the refresh
    //    token
    // 5. We want to set the refresh
    //    token in a cookie

    // TODO: Things we need
    // 1. We need a user model
    // 2. We need a refresh token model

    var user = await appDbContext.Users.SingleOrDefaultAsync(u => u.Username == loginRequest.Username);

    if (user is null)
    {
      return Results.Unauthorized();
    }

    if (loginRequest.Username is not ADMIN_USERNAME || loginRequest.Password is not ADMIN_PASSWORD)
    {
      return Results.Unauthorized();
    }

    return Results.Ok();
  }
}