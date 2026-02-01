namespace FiscalOS.API.Data;

internal sealed class MigrationService(
  IServiceProvider serviceProvider,
  ILogger<MigrationService> logger
) : IHostedService
{
  private readonly IServiceProvider _serviceProvider = serviceProvider;

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    logger.LogInformation("Applying database migrations...");

    using var scope = _serviceProvider.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync(cancellationToken);

    logger.LogInformation("Database migrations applied.");
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}