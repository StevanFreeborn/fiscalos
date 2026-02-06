namespace FiscalOS.Infra.Tests.Unit;

public class ProblemDetailsAuthResultHandlerTests
{
  private readonly ProblemDetailsAuthResultHandler _sut = new();

  [Fact]
  public async Task HandleAsync_WhenAuthorizationSucceeds_ItShouldCallNextDelegate()
  {
    var nextCalled = false;
    RequestDelegate next = _ =>
    {
      nextCalled = true;
      return Task.CompletedTask;
    };

    var context = new DefaultHttpContext();
    var policy = CreateValidAuthorizationPolicy();
    var authorizeResult = PolicyAuthorizationResult.Success();

    await _sut.HandleAsync(next, context, policy, authorizeResult);

    nextCalled.Should().BeTrue();
  }

  [Fact]
  public async Task HandleAsync_WhenAuthorizationSucceeds_ItShouldNotModifyResponseStatusCode()
  {
    var originalStatusCode = StatusCodes.Status200OK;
    RequestDelegate next = _ => Task.CompletedTask;

    var context = new DefaultHttpContext { Response = { StatusCode = originalStatusCode } };
    var policy = CreateValidAuthorizationPolicy();
    var authorizeResult = PolicyAuthorizationResult.Success();

    await _sut.HandleAsync(next, context, policy, authorizeResult);

    context.Response.StatusCode.Should().Be(originalStatusCode);
  }

  [Fact]
  public async Task HandleAsync_WhenAuthorizationSucceeds_ItShouldNotWriteToResponseBody()
  {
    RequestDelegate next = _ => Task.CompletedTask;

    var context = new DefaultHttpContext();
    var policy = CreateValidAuthorizationPolicy();
    var authorizeResult = PolicyAuthorizationResult.Success();

    await _sut.HandleAsync(next, context, policy, authorizeResult);

    context.Response.Body.Length.Should().Be(0);
  }

  [Fact]
  public async Task HandleAsync_WhenAuthorizationResultIsForbidden_ItShouldSetStatusCodeTo403()
  {
    RequestDelegate next = _ => Task.CompletedTask;

    var context = new DefaultHttpContext();
    var policy = CreateValidAuthorizationPolicy();
    var authorizeResult = PolicyAuthorizationResult.Forbid();

    await _sut.HandleAsync(next, context, policy, authorizeResult);

    context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
  }

  [Fact]
  public async Task HandleAsync_WhenAuthorizationResultIsForbidden_ItShouldSetContentTypeToApplicationProblemJson()
  {
    RequestDelegate next = _ => Task.CompletedTask;

    var context = new DefaultHttpContext();
    var policy = CreateValidAuthorizationPolicy();
    var authorizeResult = PolicyAuthorizationResult.Forbid();

    await _sut.HandleAsync(next, context, policy, authorizeResult);

    context.Response.ContentType.Should().Be("application/problem+json");
  }

  [Fact]
  public async Task HandleAsync_WhenAuthorizationResultIsForbidden_ItShouldWriteProblemDetailsWithForbiddenTitle()
  {
    RequestDelegate next = _ => Task.CompletedTask;

    var context = new DefaultHttpContext();
    context.Response.Body = new MemoryStream();

    var policy = CreateValidAuthorizationPolicy();
    var authorizeResult = PolicyAuthorizationResult.Forbid();

    await _sut.HandleAsync(next, context, policy, authorizeResult);

    context.Response.Body.Seek(0, SeekOrigin.Begin);
    using var stream = new StreamReader(context.Response.Body);
    var responseBody = await stream.ReadToEndAsync(TestContext.Current.CancellationToken);
    var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

    problemDetails!.Title.Should().Be("Forbidden");
  }

  [Fact]
  public async Task HandleAsync_WhenAuthorizationResultIsForbidden_ItShouldWriteProblemDetailsWithCorrectStatus()
  {
    RequestDelegate next = _ => Task.CompletedTask;

    var context = new DefaultHttpContext();
    context.Response.Body = new MemoryStream();

    var policy = CreateValidAuthorizationPolicy();
    var authorizeResult = PolicyAuthorizationResult.Forbid();

    await _sut.HandleAsync(next, context, policy, authorizeResult);

    context.Response.Body.Seek(0, SeekOrigin.Begin);
    using var stream = new StreamReader(context.Response.Body);
    var responseBody = await stream.ReadToEndAsync(TestContext.Current.CancellationToken);
    var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

    problemDetails!.Status.Should().Be(StatusCodes.Status403Forbidden);
  }

  [Fact]
  public async Task HandleAsync_WhenAuthorizationResultIsForbidden_ItShouldWriteProblemDetailsWithCorrectDetail()
  {
    RequestDelegate next = _ => Task.CompletedTask;

    var context = new DefaultHttpContext();
    context.Response.Body = new MemoryStream();

    var policy = CreateValidAuthorizationPolicy();
    var authorizeResult = PolicyAuthorizationResult.Forbid();

    await _sut.HandleAsync(next, context, policy, authorizeResult);

    context.Response.Body.Seek(0, SeekOrigin.Begin);
    using var stream = new StreamReader(context.Response.Body);
    var responseBody = await stream.ReadToEndAsync(TestContext.Current.CancellationToken);
    var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

    problemDetails!.Detail.Should().Be("You do not have permission to access this resource.");
  }

  [Fact]
  public async Task HandleAsync_WhenAuthorizationResultIsForbidden_ItShouldNotCallNextDelegate()
  {
    var nextCalled = false;
    RequestDelegate next = _ =>
    {
      nextCalled = true;
      return Task.CompletedTask;
    };

    var context = new DefaultHttpContext();
    context.Response.Body = new MemoryStream();

    var policy = CreateValidAuthorizationPolicy();
    var authorizeResult = PolicyAuthorizationResult.Forbid();

    await _sut.HandleAsync(next, context, policy, authorizeResult);

    nextCalled.Should().BeFalse();
  }

  [Fact]
  public async Task HandleAsync_WhenAuthorizationResultIsUnauthorized_ItShouldSetStatusCodeTo401()
  {
    RequestDelegate next = _ => Task.CompletedTask;

    var context = new DefaultHttpContext();
    var policy = CreateValidAuthorizationPolicy();
    var authorizeResult = PolicyAuthorizationResult.Challenge();

    await _sut.HandleAsync(next, context, policy, authorizeResult);

    context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
  }

  [Fact]
  public async Task HandleAsync_WhenAuthorizationResultIsUnauthorized_ItShouldSetContentTypeToApplicationProblemJson()
  {
    RequestDelegate next = _ => Task.CompletedTask;

    var context = new DefaultHttpContext();
    var policy = CreateValidAuthorizationPolicy();
    var unauthorizedResult = PolicyAuthorizationResult.Challenge();

    await _sut.HandleAsync(next, context, policy, unauthorizedResult);

    context.Response.ContentType.Should().Be("application/problem+json");
  }

  [Fact]
  public async Task HandleAsync_WhenAuthorizationResultIsUnauthorized_ItShouldWriteProblemDetailsWithUnauthorizedTitle()
  {
    RequestDelegate next = _ => Task.CompletedTask;

    var context = new DefaultHttpContext();
    context.Response.Body = new MemoryStream();

    var policy = CreateValidAuthorizationPolicy();
    var unauthorizedResult = PolicyAuthorizationResult.Challenge();

    await _sut.HandleAsync(next, context, policy, unauthorizedResult);

    context.Response.Body.Seek(0, SeekOrigin.Begin);
    using var stream = new StreamReader(context.Response.Body);
    var responseBody = await stream.ReadToEndAsync(TestContext.Current.CancellationToken);
    var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

    problemDetails!.Title.Should().Be("Unauthorized");
  }

  [Fact]
  public async Task HandleAsync_WhenAuthorizationResultIsUnauthorized_ItShouldWriteProblemDetailsWithCorrectStatus()
  {
    RequestDelegate next = _ => Task.CompletedTask;

    var context = new DefaultHttpContext();
    context.Response.Body = new MemoryStream();

    var policy = CreateValidAuthorizationPolicy();
    var unauthorizedResult = PolicyAuthorizationResult.Challenge();

    await _sut.HandleAsync(next, context, policy, unauthorizedResult);

    context.Response.Body.Seek(0, SeekOrigin.Begin);
    using var stream = new StreamReader(context.Response.Body);
    var responseBody = await stream.ReadToEndAsync(TestContext.Current.CancellationToken);
    var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

    problemDetails!.Status.Should().Be(StatusCodes.Status401Unauthorized);
  }

  [Fact]
  public async Task HandleAsync_WhenAuthorizationResultIsUnauthorized_ItShouldWriteProblemDetailsWithCorrectDetail()
  {
    RequestDelegate next = _ => Task.CompletedTask;

    var context = new DefaultHttpContext();
    context.Response.Body = new MemoryStream();

    var policy = CreateValidAuthorizationPolicy();
    var unauthorizedResult = PolicyAuthorizationResult.Challenge();

    await _sut.HandleAsync(next, context, policy, unauthorizedResult);

    context.Response.Body.Seek(0, SeekOrigin.Begin);
    using var stream = new StreamReader(context.Response.Body);
    var responseBody = await stream.ReadToEndAsync(TestContext.Current.CancellationToken);
    var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

    problemDetails!.Detail.Should().Be("Authentication is required to access this resource.");
  }

  [Fact]
  public async Task HandleAsync_WhenAuthorizationResultIsUnauthorized_ItShouldNotCallNextDelegate()
  {
    var nextCalled = false;
    RequestDelegate next = _ =>
    {
      nextCalled = true;
      return Task.CompletedTask;
    };

    var context = new DefaultHttpContext();
    context.Response.Body = new MemoryStream();

    var policy = CreateValidAuthorizationPolicy();
    var unauthorizedResult = PolicyAuthorizationResult.Challenge();

    await _sut.HandleAsync(next, context, policy, unauthorizedResult);

    nextCalled.Should().BeFalse();
  }

  [Fact]
  public async Task HandleAsync_WhenAuthorizationFails_ItShouldWriteValidJsonToProblemDetails()
  {
    RequestDelegate next = _ => Task.CompletedTask;

    var context = new DefaultHttpContext();
    context.Response.Body = new MemoryStream();

    var policy = CreateValidAuthorizationPolicy();
    var authorizeResult = PolicyAuthorizationResult.Challenge();

    await _sut.HandleAsync(next, context, policy, authorizeResult);

    context.Response.Body.Seek(0, SeekOrigin.Begin);
    using var stream = new StreamReader(context.Response.Body);
    var responseBody = await stream.ReadToEndAsync(TestContext.Current.CancellationToken);

    var action = () => JsonSerializer.Deserialize<ProblemDetails>(responseBody);
    action.Should().NotThrow();
  }

  [Fact]
  public async Task HandleAsync_WhenAuthorizationFails_ItShouldWriteResponseBodyWithContent()
  {
    RequestDelegate next = _ => Task.CompletedTask;

    var context = new DefaultHttpContext();
    context.Response.Body = new MemoryStream();

    var policy = CreateValidAuthorizationPolicy();
    var authorizeResult = PolicyAuthorizationResult.Challenge();

    await _sut.HandleAsync(next, context, policy, authorizeResult);

    context.Response.Body.Length.Should().BeGreaterThan(0);
  }

  private static AuthorizationPolicy CreateValidAuthorizationPolicy()
  {
    var requirement = new DummyAuthorizationRequirement();
    return new AuthorizationPolicy([requirement], []);
  }

  private sealed class DummyAuthorizationRequirement : IAuthorizationRequirement
  {
  }
}