

namespace FiscalOS.AppHost.Tests.Infra;

public abstract class BaseTest(
  AspireFixture aspire,
  PlaywrightFixture playwright
) : IAsyncLifetime, IClassFixture<AspireFixture>, IClassFixture<PlaywrightFixture>
{
  protected AspireFixture AspireFixture { get; } = aspire;
  protected PlaywrightFixture PlaywrightFixture { get; } = playwright;

  protected IBrowserContext Context { get; private set; } = null!;
  protected IPage Page { get; private set; } = null!;

  public async ValueTask InitializeAsync()
  {
    Context = await PlaywrightFixture.Browser.NewContextAsync(new()
    {
      BaseURL = AspireFixture.GetBaseWebUri().ToString(),
    });
    Page = await Context.NewPageAsync();
  }

  public async ValueTask DisposeAsync()
  {
    await Page.CloseAsync();
    await Context.CloseAsync();
    await Context.DisposeAsync();

    GC.SuppressFinalize(this);
  }

  public static ILocatorAssertions Expect(ILocator locator)
  {
    return Assertions.Expect(locator);
  }

  public static IPageAssertions Expect(IPage page)
  {
    return Assertions.Expect(page);
  }

  public static IAPIResponseAssertions Expect(IAPIResponse response)
  {
    return Assertions.Expect(response);
  }
}