namespace FiscalOS.API.Tests.Infra;

internal sealed class HttpRequestBuilder
{
  private HttpMethod _method = HttpMethod.Get;
  private Uri? _uri;
  private object? _body;
  private string? _bearerToken;
  private readonly Dictionary<string, string> _cookies = [];
  private readonly Dictionary<string, string> _headers = [];

  private HttpRequestBuilder()
  {
  }

  public static HttpRequestBuilder New() => new();

  public HttpRequestBuilder WithMethod(HttpMethod method)
  {
    _method = method;
    return this;
  }

  public HttpRequestBuilder WithUri(Uri uri)
  {
    _uri = uri;
    return this;
  }

  public HttpRequestBuilder WithBody<T>(T body)
  {
    _body = body;
    return this;
  }

  public HttpRequestBuilder WithBearerToken(string token)
  {
    _bearerToken = token;
    return this;
  }

  public HttpRequestBuilder WithUserId(Guid userId)
  {
    _bearerToken = JwtTokenBuilder.New()
      .WithClaim(JwtRegisteredClaimNames.Sub, userId.ToString())
      .Build();

    return this;
  }

  public HttpRequestBuilder WithCookie(string name, string value)
  {
    _cookies[name] = value;
    return this;
  }

  public HttpRequestBuilder WithRefreshCookie(string token)
  {
    return WithCookie("fiscalos_refresh_cookie", token);
  }

  public HttpRequestBuilder WithHeader(string name, string value)
  {
    _headers[name] = value;
    return this;
  }

  public HttpRequestBuilder Post(Uri uri)
  {
    _method = HttpMethod.Post;
    _uri = uri;
    return this;
  }

  public HttpRequestBuilder Get(Uri uri)
  {
    _method = HttpMethod.Get;
    _uri = uri;
    return this;
  }

  public HttpRequestBuilder Put(Uri uri)
  {
    _method = HttpMethod.Put;
    _uri = uri;
    return this;
  }

  public HttpRequestBuilder Delete(Uri uri)
  {
    _method = HttpMethod.Delete;
    _uri = uri;
    return this;
  }

  public HttpRequestMessage Build()
  {
    if (_uri is null)
    {
      throw new InvalidOperationException("URI must be set before building the request.");
    }

    var request = new HttpRequestMessage(_method, _uri);

    if (_body is not null)
    {
      var json = JsonSerializer.Serialize(_body);
      request.Content = new StringContent(json, Encoding.UTF8, "application/json");
    }

    if (_bearerToken is not null)
    {
      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);
    }

    foreach (var (key, value) in _cookies)
    {
      request.Headers.Add("Cookie", $"{key}={value}");
    }

    foreach (var (key, value) in _headers)
    {
      request.Headers.Add(key, value);
    }

    return request;
  }
}