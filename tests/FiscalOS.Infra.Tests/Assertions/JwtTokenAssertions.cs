namespace FiscalOS.Infra.Tests.Assertions;

internal static class JwtTokenAssertions
{
  public static AndConstraint<string> HaveClaim(
    this StringAssertions assertions,
    string claimType
  )
  {
    var handler = new JwtSecurityTokenHandler();
    var jwtToken = handler.ReadJwtToken(assertions.Subject);

    var claim = jwtToken.Claims.FirstOrDefault(c => c.Type == claimType);

    assertions.CurrentAssertionChain
      .ForCondition(claim is not null)
      .FailWith($"Expected JWT token to have claim '{claimType}', but it was not found.");

    return new AndConstraint<string>(assertions.Subject);
  }

  public static AndConstraint<string> HaveClaimWithValue(
    this StringAssertions assertions,
    string claimType,
    string expectedValue
  )
  {
    var handler = new JwtSecurityTokenHandler();
    var jwtToken = handler.ReadJwtToken(assertions.Subject);

    var claim = jwtToken.Claims.FirstOrDefault(c => c.Type == claimType);

    assertions.CurrentAssertionChain
      .ForCondition(claim is not null)
      .FailWith($"Expected JWT token to have claim '{claimType}', but it was not found.");

    assertions.CurrentAssertionChain
      .ForCondition(claim!.Value == expectedValue)
      .FailWith(
        $"Expected JWT token claim '{claimType}' to have value '{expectedValue}', but found '{claim.Value}'."
      );

    return new AndConstraint<string>(assertions.Subject);
  }
}