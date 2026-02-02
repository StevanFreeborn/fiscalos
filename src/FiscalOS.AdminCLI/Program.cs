await Host.CreateDefaultBuilder(args)
  .ConfigureAppConfiguration(
    static c => c.SetBasePath(AppContext.BaseDirectory)
      .AddJsonFile("appsettings.json")
      .AddEnvironmentVariables()
  )
  .ConfigureLogging(static c => c.ClearProviders())
  .ConfigureServices(static (_, services) =>
  {
    services.AddSingleton(AnsiConsole.Console);
    services.AddInfrastructure();
    services.AddHostedService<App>();
  })
  .Build()
  .StartAsync();
