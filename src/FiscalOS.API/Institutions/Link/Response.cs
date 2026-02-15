namespace FiscalOS.API.Institutions.Link;

public sealed record Response
{
  public string LinkToken { get; init; } = string.Empty;

  [JsonConstructor]
  private Response()
  {
  }

  public static Response From(string linkToken)
  {
    return new()
    {
      LinkToken = linkToken,
    };
  }
}