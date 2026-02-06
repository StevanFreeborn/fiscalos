namespace FiscalOS.Infra.Tests.Mocks;

internal sealed class MockOptionsMonitor<T> : IOptionsMonitor<T> where T : new()
{
  private Action<T, string>? _listener;

  public T CurrentValue
  {
    get;
    set
    {
      field = value;
      _listener?.Invoke(field, string.Empty);
    }
  } = new();

  public T Get(string? name)
  {
    return CurrentValue;
  }

  public IDisposable? OnChange(Action<T, string> listener)
  {
    _listener = listener;
    return new Mock<IDisposable>().Object;
  }
}