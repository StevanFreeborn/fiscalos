namespace FiscalOS.API.Refresh;

internal sealed record Response
{
  public string AccessToken { get; init; }

  [JsonConstructor]
  private Response()
  {
    AccessToken = string.Empty;
  }

  private Response(string accessToken)
  {
    AccessToken = accessToken;
  }

  public static Response From(string accessToken)
  {
    return new(accessToken);
  }
}