namespace FiscalOS.Infra.DependencyInjection;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection services)
  {
    services.AddSingleton<IPasswordHasher, PasswordHasher>(static (_) => PasswordHasher.New());
    services.ConfigureOptions<AppDbContextOptionsSetup>();
    services.AddDbContext<AppDbContext>();
    services.AddHostedService<MigrationService>();
    return services;
  }
}