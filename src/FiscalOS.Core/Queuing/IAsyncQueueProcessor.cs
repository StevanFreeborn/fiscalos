namespace FiscalOS.Core.Queuing;

public interface IAsyncQueueProcessor<T>
{
  Task ProcessAsync(T item, CancellationToken cancellationToken = default);
}
