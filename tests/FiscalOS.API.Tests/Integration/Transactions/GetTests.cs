using FiscalOS.API.Transactions;

namespace FiscalOS.API.Tests.Integration.Transactions;

public class GetTests(TestApi testApi) : IntegrationTest(testApi)
{
  private static readonly Uri GetUri = new("/transactions", UriKind.Relative);

  [Fact]
  public async Task Get_WhenNotLoggedIn_ItShouldReturn401WithProblemDetails()
  {
    using var request = HttpRequestBuilder.New()
      .Get(GetUri)
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeProblemDetails(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task Get_WhenCalledWithInvalidPageNumber_ItShouldReturn400WithProblemDetails()
  {
    using var request = HttpRequestBuilder.New()
      .Get(GetUri)
      .WithQueryParameter("pageNumber", "-1")
      .WithUserId(Guid.NewGuid())
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeValidationProblemDetails(new Dictionary<string, string[]>()
    {
      ["PageNumber"] = ["PageNumber must be greater than 0."],
    });
  }

  [Theory]
  [InlineData(0)]
  [InlineData(1001)]
  public async Task Get_WhenCalledWithInvalidPageSize_ItShouldReturn400WithProblemDetails(int pageSize)
  {
    using var request = HttpRequestBuilder.New()
      .Get(GetUri)
      .WithQueryParameter("pageSize", pageSize.ToString(CultureInfo.InvariantCulture))
      .WithUserId(Guid.NewGuid())
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeValidationProblemDetails(new Dictionary<string, string[]>()
    {
      ["PageSize"] = ["PageSize must be between 1 and 1000."],
    });
  }

  [Fact]
  public async Task Get_WhenCalled_ItShouldReturn200WithPagedResponseOfTransactions()
  {
    var user = await Api.ExecuteAsync(static async (context, ct, sp) =>
    {
      var passwordHasher = sp.GetRequiredService<IPasswordHasher>();
      var encryptor = sp.GetRequiredService<IEncryptor>();

      var user = UserBuilder.Create()
        .WithInstitution(static ib =>
        {
          ib.WithMetadata();
          ib.WithAccount(static ab =>
          {
            ab.WithMetadata();
            ab.WithTransaction(static tb =>
            {
              tb.WithMetadata();
            });
          });
        })
        .Build();

      await context.AddAsync(user, ct);
      await context.SaveChangesAsync(ct);
      return user;
    }, TestContext.Current.CancellationToken);

    using var request = HttpRequestBuilder.New()
      .Get(GetUri)
      .WithUserId(user.Id)
      .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    (await response.Should()
      .BeJsonContentOfType<PagedResponse<TransactionDto>>(HttpStatusCode.OK))
      .Which
      .Items
      .Should()
      .BeEquivalentTo(
        user.Transactions.Select(TransactionDto.From)
      );
  }
}