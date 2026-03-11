namespace FiscalOS.ServiceDefaults;

internal sealed class SeqOptions
{
  public string ServerUrl { get; init; } = string.Empty;
  public string ApiKeyHeader { get; init; } = string.Empty;
  public string ApiKey { get; init; } = string.Empty;
  public bool IsEnabled => string.IsNullOrWhiteSpace(ServerUrl) is false &&
    string.IsNullOrWhiteSpace(ApiKeyHeader) is false &&
    string.IsNullOrWhiteSpace(ApiKey) is false;
  public string LogEndpoint => $"{ServerUrl}/ingest/otlp/v1/logs";
  public string TraceEndpoint => $"{ServerUrl}/ingest/otlp/v1/traces";
  public string AuthHeader => $"{ApiKeyHeader}={ApiKey}";
}