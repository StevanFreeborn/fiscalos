namespace FiscalOS.Infra.DependencyInjection;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection services)
  {
    services.AddSingleton<IAuthorizationMiddlewareResultHandler, ProblemDetailsAuthResultHandler>();
    
    services.AddHttpClient();
    services.AddSingleton(TimeProvider.System);
    services.AddSingleton<IFileSystem, FileSystem>();

    services.ConfigureOptions<JwtOptionsSetup>();
    services.ConfigureOptions<JwtBearerOptionsSetup>();

    services.AddSingleton<ITokenGenerator>(TokenGenerator.From);
    services.AddSingleton<IPasswordHasher>(PasswordHasher.From);

    services.ConfigureOptions<FileKeyRingOptionsSetup>();
    services.AddSingleton<IKeyRing>(FileKeyRing.From);
    services.AddSingleton<IEncryptor>(Encryptor.From);

    services.ConfigureOptions<AppDbContextOptionsSetup>();
    services.AddDbContext<AppDbContext>();
    services.AddHostedService<MigrationService>();

    services.ConfigureOptions<PlaidClientOptionsSetup>();
    services.AddSingleton(static sp =>
    {
      var factory = sp.GetRequiredService<IHttpClientFactory>();
      var logger = sp.GetRequiredService<ILogger<PlaidClient>>();
      var clientOptions = sp.GetRequiredService<IOptions<PlaidClientOptions>>();
      var options = clientOptions.Value.ToPlaidOptions();
      return new PlaidClient(options, factory, logger);
    });
    services.AddSingleton(PlaidService.From);

    return services;
  }
}