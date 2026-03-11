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
    logger.LogError(exception, "An unhandled exception occurred");

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