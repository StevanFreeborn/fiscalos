namespace FiscalOS.Core.Queuing;

public interface IAsyncQueue<T>
{
  Task EnqueueAsync(T item, CancellationToken cancellationToken = default);
  Task<T> DequeueAsync(CancellationToken cancellationToken = default);
  ValueTask DisposeAsync();
}
