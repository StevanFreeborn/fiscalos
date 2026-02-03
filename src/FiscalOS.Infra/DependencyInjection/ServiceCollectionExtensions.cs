namespace FiscalOS.Infra.DependencyInjection;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection services)
  {
    services.AddSingleton(TimeProvider.System);

    services.ConfigureOptions<JwtOptionsSetup>();
    services.ConfigureOptions<JwtBearerOptionsSetup>();

    services.AddSingleton<ITokenGenerator, TokenGenerator>(TokenGenerator.From);
    services.AddSingleton<IPasswordHasher, PasswordHasher>(PasswordHasher.From);

    services.ConfigureOptions<AppDbContextOptionsSetup>();
    services.AddDbContext<AppDbContext>();
    services.AddHostedService<MigrationService>();

    return services;
  }
}