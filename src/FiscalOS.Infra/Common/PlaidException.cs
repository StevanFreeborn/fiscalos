namespace FiscalOS.Infra.Common;

public class PlaidException : Exception
{
  public PlaidException()
  {
  }

  public PlaidException(string message) : base(message)
  {
  }

  public PlaidException(string message, Exception innerException) : base(message, innerException)
  {
  }
}