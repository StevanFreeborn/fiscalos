
using FiscalOS.API.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FiscalOS.API.Tests.Integration;

public abstract class IntegrationTest(TestApi testApi) : IClassFixture<TestApi>, IAsyncLifetime
{
  protected TestApi TestApi { get; } = testApi;

  // TODO: This seems not correct
  // would need to dispose of scope
  // we need to clean up the test
  // database files when the test ends

  protected DbContext TestDbContext
  {
    get
    {
      var scopeFactory = TestApi.Services.GetRequiredService<IServiceScopeFactory>();
      var scope = scopeFactory.CreateScope();
      return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }
  }

  public ValueTask InitializeAsync()
  {
    return new(TestDbContext.Database.EnsureCreatedAsync());
  }

  public ValueTask DisposeAsync()
  {
    GC.SuppressFinalize(this);
    return new(TestDbContext.Database.EnsureDeletedAsync());
  }

}