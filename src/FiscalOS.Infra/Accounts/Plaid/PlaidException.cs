namespace FiscalOS.Infra.Accounts.Plaid;

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