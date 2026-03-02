namespace FiscalOS.Infra.Queuing;

internal class AsyncQueueHostedService<T> : BackgroundService where T : notnull
{
  private readonly IAsyncQueue<T> _queue;
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<AsyncQueueHostedService<T>> _logger;

  private AsyncQueueHostedService(
    IAsyncQueue<T> queue,
    IServiceProvider serviceProvider,
    ILogger<AsyncQueueHostedService<T>> logger
  )
  {
    _queue = queue;
    _serviceProvider = serviceProvider;
    _logger = logger;
  }

  public static AsyncQueueHostedService<T> From(IServiceProvider serviceProvider)
  {
    var queue = serviceProvider.GetRequiredService<IAsyncQueue<T>>();
    var logger = serviceProvider.GetRequiredService<ILogger<AsyncQueueHostedService<T>>>();
    return new(queue, serviceProvider, logger);
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("Queue hosted service for {ItemType} starting", typeof(T).Name);

    try
    {
      while (!stoppingToken.IsCancellationRequested)
      {
        var item = await _queue.DequeueAsync(stoppingToken).ConfigureAwait(false);

        using var scope = _serviceProvider.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<IAsyncQueueProcessor<T>>();

        try
        {
          await processor.ProcessAsync(item, stoppingToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error processing queue item of type {ItemType}", typeof(T).Name);
        }
      }
    }
    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
    {
      _logger.LogInformation("Queue hosted service for {ItemType} stopped", typeof(T).Name);
    }
  }
}
