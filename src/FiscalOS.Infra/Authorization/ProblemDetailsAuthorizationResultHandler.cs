namespace FiscalOS.Infra.Authorization;

internal sealed class ProblemDetailsAuthResultHandler : IAuthorizationMiddlewareResultHandler
{
  public Task HandleAsync(
    RequestDelegate next,
    HttpContext context,
    AuthorizationPolicy policy,
    PolicyAuthorizationResult authorizeResult
  )
  {
    if (authorizeResult.Succeeded)
    {
      return next(context);
    }

    context.Response.StatusCode = authorizeResult.Forbidden
      ? StatusCodes.Status403Forbidden
      : StatusCodes.Status401Unauthorized;

    context.Response.ContentType = "application/problem+json";

    var problemDetails = authorizeResult.Forbidden
      ? new ProblemDetails
      {
        Title = "Forbidden",
        Status = StatusCodes.Status403Forbidden,
        Detail = "You do not have permission to access this resource."
      }
      : new ProblemDetails
      {
        Title = "Unauthorized",
        Status = StatusCodes.Status401Unauthorized,
        Detail = "Authentication is required to access this resource."
      };

    var json = JsonSerializer.Serialize(problemDetails);

    return context.Response.WriteAsync(json);
  }
}