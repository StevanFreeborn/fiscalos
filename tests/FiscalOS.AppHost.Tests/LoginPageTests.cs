namespace FiscalOS.AppHost.Tests;

public class LoginPageTests(
  AspireFixture aspire,
  PlaywrightFixture playwright
) : BaseTest(aspire, playwright)
{
  [Fact]
  public async Task LoginPage_WhenNavigatedTo_ItShouldDisplaysLoginButton()
  {
    await Page.GotoAsync("/public/login");
    await Expect(Page.GetByText("Login")).ToBeVisibleAsync();
  }
}