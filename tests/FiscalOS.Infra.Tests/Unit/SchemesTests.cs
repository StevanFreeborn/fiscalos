namespace FiscalOS.Infra.Tests.Unit;

public class SchemesTests
{
  [Fact]
  public void Default_WhenCalled_ItShouldHaveExpectedValue()
  {
    Schemes.Default.Should().Be(JwtBearerDefaults.AuthenticationScheme);
  }

  [Fact]
  public void AllowExpiredTokens_WhenCalled_ItShouldHaveExpectedValue()
  {
    Schemes.AllowExpiredTokens.Should().Be("AllowExpiredTokens");
  }
}