namespace FiscalOS.Infra.Common;

public class PlaidException : Exception
{
  public PlaidError? Error { get; }
  public string? RequestId { get; }
  public int? StatusCode { get; }
  public string? ErrorCode => Error?.ErrorCode;
  public string? ErrorType => Error?.ErrorType;
  public string? ErrorMessage => Error?.ErrorMessage;

  public PlaidException()
  {
  }

  public PlaidException(string message) : base(message)
  {
  }

  public PlaidException(string message, Exception innerException) : base(message, innerException)
  {
  }

  public PlaidException(
    string message,
    PlaidError? error = null,
    string? plaidRequestId = null,
    int? statusCode = null,
    Exception? innerException = null
  ) : base(message, innerException)
  {
    Error = error;
    RequestId = plaidRequestId;
    StatusCode = statusCode;

    if (error is not null)
    {
      Data[nameof(ErrorCode)] = error.ErrorCode;
      Data[nameof(ErrorType)] = error.ErrorType;
      Data[nameof(ErrorMessage)] = error.ErrorMessage;
    }

    if (plaidRequestId is not null)
    {
      Data[nameof(RequestId)] = plaidRequestId;
    }

    if (statusCode is not null)
    {
      Data[nameof(StatusCode)] = statusCode;
    }
  }
}