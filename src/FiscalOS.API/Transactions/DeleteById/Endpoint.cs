namespace FiscalOS.API.Transactions.DeleteById;

internal static class Endpoint
{
  private const string Route = "/{id}";

  public static RouteHandlerBuilder MapDeleteEndpoint(this RouteGroupBuilder groupBuilder)
  {
    return groupBuilder.MapDelete(Route, HandleAsync);
  }

  private static async Task<IResult> HandleAsync(
    HttpContext httpContext,
    [FromRoute] Guid id,
    [FromServices] AppDbContext appDbContext,
    CancellationToken ct
  )
  {
    var userId = httpContext.GetUserId();

    var existingTransaction = await appDbContext.Transactions
      .SingleOrDefaultAsync(t => t.UserId == userId && t.Id == id, ct);

    if (existingTransaction is null)
    {
      return Results.NotFound();
    }

    appDbContext.Transactions.Remove(existingTransaction);
    await appDbContext.SaveChangesAsync(ct);

    return Results.NoContent();
  }
}