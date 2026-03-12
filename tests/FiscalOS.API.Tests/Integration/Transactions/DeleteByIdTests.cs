using Transaction = FiscalOS.Core.Transactions.Transaction;

namespace FiscalOS.API.Tests.Integration.Transactions;

public class DeleteByIdTests(TestApi testApi) : IntegrationTest(testApi)
{
  private static Uri GetDeleteByIdUri(Guid id)
  {
    return new($"/transactions/{id}", UriKind.Relative);
  }

  [Fact]
  public async Task DeleteById_WhenCalledAndUnauthenticated_ItShouldREturn401WithProblemDetails()
  {
    using var request = HttpRequestBuilder.New()
    .Delete(GetDeleteByIdUri(Guid.NewGuid()))
    .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeProblemDetails(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task DeleteById_WhenCalledWithNonExistentTransaction_ItShouldReturn401WithProblemDetails()
  {
    using var request = HttpRequestBuilder.New()
    .Delete(GetDeleteByIdUri(Guid.NewGuid()))
    .WithUserId(Guid.NewGuid())
    .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeProblemDetails(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task DeleteById_WhenCalledWithTransactionIdThatDoesNotBelongToTheUser_ItShouldReturn404WithProblemDetails()
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
    .Delete(GetDeleteByIdUri(Guid.NewGuid()))
    .WithUserId(Guid.NewGuid())
    .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    await response.Should().BeProblemDetails(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task DeleteById_WhenCalledWithTransactionIdThatDoesExistAndBelongsToTheUser_ItShouldReturn204WithNoContent()
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
    .Delete(GetDeleteByIdUri(user.Transactions.First().Id))
    .WithUserId(user.Id)
    .Build();

    var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

    response.StatusCode.Should().Be(HttpStatusCode.NoContent);

    var deletedTransaction = await Api.ExecuteAsync(async (context, ct) =>
    {
      return await context.Set<Transaction>()
        .SingleOrDefaultAsync(t => t.Id == user.Transactions.First().Id, ct);
    }, TestContext.Current.CancellationToken);

    deletedTransaction.Should().BeNull();
  }
}