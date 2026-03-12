using FiscalOS.Infra.Common;

using Microsoft.AspNetCore.Diagnostics;

internal sealed class GlobalExceptionHandler(
  IProblemDetailsService problemDetailsService,
  ILogger<GlobalExceptionHandler> logger
) : IExceptionHandler
{
  public async ValueTask<bool> TryHandleAsync(
    HttpContext httpContext,
    Exception exception,
    CancellationToken cancellationToken
  )
  {
    switch (exception)
    {
      case PlaidException plaidException:
        logger.LogError(
          plaidException,
          "Unhandled Plaid exception. PlaidRequestId: {PlaidRequestId}, PlaidStatusCode: {PlaidStatusCode}, PlaidErrorCode: {PlaidErrorCode}, PlaidErrorType: {PlaidErrorType}, PlaidErrorMessage: {PlaidErrorMessage}",
          plaidException.RequestId,
          plaidException.StatusCode,
          plaidException.ErrorCode,
          plaidException.ErrorType,
          plaidException.ErrorMessage
        );
        break;
      default:
        logger.LogError(exception, "An unhandled exception occurred");
        break;
    }

    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

    return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
    {
      HttpContext = httpContext,
      Exception = exception,
      ProblemDetails = new ProblemDetails
      {
        Type = exception.GetType().Name,
        Title = "Uh oh something has gone wrong.",
        Detail = exception.Message
      }
    });
  }
}