namespace FiscalOS.API.Tests.Assertions;

internal static class HttpResponseExtensions
{
  public static HttpResponseMessageAssertions Should(this HttpResponseMessage instance)
  {
    return new HttpResponseMessageAssertions(instance, AssertionChain.GetOrCreate());
  }
}

internal sealed class HttpResponseMessageAssertions(
  HttpResponseMessage instance,
  AssertionChain assertionChain
) : ReferenceTypeAssertions<HttpResponseMessage, HttpResponseMessageAssertions>(instance, assertionChain)
{
  private readonly AssertionChain _chain = assertionChain;

  protected override string Identifier => "HttpResponseMessage";

  public async Task<AndWhichConstraint<HttpResponseMessageAssertions, T>> BeJsonContentOfType<T>(
    HttpStatusCode expectedStatusCode
  )
  {
    _chain.ForCondition(Subject.StatusCode == expectedStatusCode)
      .FailWith(
        "Expected response status code to be {0}, but found {1}",
        expectedStatusCode,
        Subject.StatusCode
      );

    _chain.ForCondition(Subject.Content.Headers.ContentType?.MediaType is "application/json")
      .FailWith(
        "Expected response to be application/json, but found {0}",
        Subject.Content.Headers.ContentType?.MediaType
      );


    var content = await Subject.Content.ReadFromJsonAsync<T>();

    _chain.ForCondition(content is not null)
      .FailWith($"Expected body to be a valid {typeof(T).Name}, but it could not be deserialized.");

    return new AndWhichConstraint<HttpResponseMessageAssertions, T>(this, content!);
  }

  public AndConstraint<HttpResponseMessageAssertions> HaveSetCookieHeader(string cookieName)
  {
    _chain.ForCondition(Subject.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
      .FailWith("Expected response to have 'Set-Cookie' header, but it was not found.");

    var hasCookie = setCookieHeaders!.Any(header => header.StartsWith(cookieName + "=", StringComparison.OrdinalIgnoreCase));

    _chain.ForCondition(hasCookie)
      .FailWith($"Expected response to have 'Set-Cookie' header for cookie '{cookieName}', but it was not found.");

    return new AndConstraint<HttpResponseMessageAssertions>(this);
  }

  public async Task<AndWhichConstraint<HttpResponseMessageAssertions, ProblemDetails>> BeProblemDetails(HttpStatusCode expectedStatusCode)
  {
    var problem = await ValidateAndDeserialize<ProblemDetails>(expectedStatusCode);
    return new AndWhichConstraint<HttpResponseMessageAssertions, ProblemDetails>(this, problem);
  }

  public async Task<AndWhichConstraint<HttpResponseMessageAssertions, ValidationProblemDetails>> BeValidationProblemDetails(
    IDictionary<string, string[]> expectedErrors,
    HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest
  )
  {
    var problem = await ValidateAndDeserialize<ValidationProblemDetails>(expectedStatusCode);

    _chain.ForCondition(problem.Errors is not null)
      .FailWith("Expected ValidationProblemDetails to contain errors, but the Errors dictionary was null.");

    problem.Errors.Should().BeEquivalentTo(expectedErrors, "the validation errors should match the expected dictionary");

    return new AndWhichConstraint<HttpResponseMessageAssertions, ValidationProblemDetails>(this, problem);
  }

  private async Task<T> ValidateAndDeserialize<T>(HttpStatusCode expectedStatusCode) where T : ProblemDetails
  {
    _chain.ForCondition(Subject.StatusCode == expectedStatusCode)
      .FailWith(
        "Expected response status code to be {0}, but found {1}",
        expectedStatusCode,
        Subject.StatusCode
      );

    _chain.ForCondition(Subject.Content.Headers.ContentType?.MediaType is "application/problem+json")
      .FailWith(
        "Expected response to be application/problem+json, but found {0}",
        Subject.Content.Headers.ContentType?.MediaType
      );

    var problem = await Subject.Content.ReadFromJsonAsync<T>();

    _chain.ForCondition(problem is not null)
      .FailWith($"Expected body to be a valid {typeof(T).Name}, but it could not be deserialized.");

    return problem!;
  }
}