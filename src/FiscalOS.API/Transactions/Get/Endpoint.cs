namespace FiscalOS.API.Transactions.Get;

internal static class Endpoint
{
  private const string Route = "/";

  public static RouteHandlerBuilder MapGetEndpoint(this RouteGroupBuilder groupBuilder)
  {
    return groupBuilder.MapGet(Route, HandleAsync);
  }

  private static async Task<IResult> HandleAsync(
    HttpContext httpContext,
    PagedQuery pagedQuery,
    [FromServices] AppDbContext appDbContext,
    CancellationToken ct
  )
  {
    var pagedQueryValidationResults = pagedQuery.Validate();

    if (pagedQueryValidationResults.Count is not 0)
    {
      return Results.ValidationProblem(pagedQueryValidationResults);
    }

    var userId = httpContext.GetUserId();
    var query = appDbContext.Transactions
      .Where(t => t.UserId == userId);

    var transactions = await query
      .Skip((pagedQuery.PageNumber - 1) * pagedQuery.PageSize)
      .Take(pagedQuery.PageSize)
      // TODO: Migrate Date to DateTime
      // to avoid in-memory sorting
      .AsAsyncEnumerable()
      .OrderByDescending(t => t.Date)
      .ToListAsync(ct);

    var count = await query
      .Where(t => t.UserId == userId)
      .CountAsync(ct);

    var transactionDtos = transactions.Select(TransactionDto.From).ToArray();
    var pagedResponse = PagedResponse<TransactionDto>.From(
      pagedQuery.PageNumber,
      pagedQuery.PageSize,
      count,
      transactionDtos
    );

    return Results.Ok(pagedResponse);
  }
}