namespace FiscalOS.Infra.Data;

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
    await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);

    logger.LogInformation("Database migrations applied.");
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}