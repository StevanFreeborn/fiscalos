namespace FiscalOS.AppHost.Tests.Infra;

public sealed class PlaywrightFixture : IAsyncLifetime
{
  private IPlaywright? _playwright;

  internal IBrowser Browser { get; set; } = null!;

  public async ValueTask InitializeAsync()
  {
    _playwright = await Playwright.CreateAsync();

    Browser = await _playwright.Chromium.LaunchAsync(new()
    {
      Headless = false,
    });
  }

  public async ValueTask DisposeAsync()
  {
    await Browser.CloseAsync();
    await Browser.DisposeAsync();

    _playwright?.Dispose();
  }
}