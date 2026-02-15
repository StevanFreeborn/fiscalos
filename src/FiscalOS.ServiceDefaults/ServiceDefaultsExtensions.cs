namespace FiscalOS.ServiceDefaults;

public static class ServiceDefaultsExtensions
{
  private const string HealthEndpointPath = "/health";
  private const string AlivenessEndpointPath = "/alive";

  public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
  {
    builder.ConfigureOpenTelemetry();

    builder.AddDefaultHealthChecks();

    builder.Services.AddServiceDiscovery();

    builder.Services.ConfigureHttpClientDefaults(static http =>
    {
      http.AddStandardResilienceHandler();
      http.AddServiceDiscovery();
    });

    return builder;
  }

  public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
  {
    builder.Logging.AddOpenTelemetry(logging =>
    {
      logging.IncludeFormattedMessage = true;
      logging.IncludeScopes = true;
    });

    builder.Services.AddOpenTelemetry()
      .WithMetrics(metrics => metrics.AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
      )
      .WithTracing(tracing => tracing.AddSource(builder.Environment.ApplicationName)
        .AddAspNetCoreInstrumentation(tracing =>
          tracing.Filter = context =>
            context.Request.Path.StartsWithSegments(HealthEndpointPath, StringComparison.InvariantCultureIgnoreCase) is false &&
            context.Request.Path.StartsWithSegments(AlivenessEndpointPath, StringComparison.InvariantCultureIgnoreCase) is false
        )
        .AddHttpClientInstrumentation()
      );

    builder.AddOpenTelemetryExporters();

    return builder;
  }

  public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
  {
    builder.Services.AddHealthChecks()
      .AddCheck("self", static () => HealthCheckResult.Healthy(), ["live"]);

    return builder;
  }

  public static WebApplication MapDefaultEndpoints(this WebApplication app)
  {
    ArgumentNullException.ThrowIfNull(app);

    if (app.Environment.IsDevelopment())
    {
      app.MapHealthChecks(HealthEndpointPath);

      app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
      {
        Predicate = static r => r.Tags.Contains("live")
      });
    }

    return app;
  }

  private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
  {
    var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

    if (useOtlpExporter)
    {
      builder.Services.AddOpenTelemetry().UseOtlpExporter();
    }

    return builder;
  }

}