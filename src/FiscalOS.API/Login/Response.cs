namespace FiscalOS.API.Login;

internal sealed record Response
{
  public string AccessToken { get; init; }

  private Response(string accessToken)
  {
    AccessToken = accessToken;
  }

  public static Response From(string accessToken)
  {
    return new(accessToken);
  }
}