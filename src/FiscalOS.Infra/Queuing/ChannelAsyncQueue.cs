namespace FiscalOS.Infra.Queuing;

internal class ChannelAsyncQueue<T> : IAsyncQueue<T>
{
  private readonly Channel<T> _channel = Channel.CreateUnbounded<T>();

  private ChannelAsyncQueue()
  {
  }

  public static ChannelAsyncQueue<T> From(IServiceProvider serviceProvider)
  {
    return new();
  }

  public async Task EnqueueAsync(T item, CancellationToken cancellationToken = default)
  {
    await _channel.Writer.WriteAsync(item, cancellationToken).ConfigureAwait(false);
  }

  public async Task<T> DequeueAsync(CancellationToken cancellationToken = default)
  {
    return await _channel.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
  }

  public async ValueTask DisposeAsync()
  {
    _channel.Writer.Complete();
    await _channel.Reader.Completion.ConfigureAwait(false);
  }
}
