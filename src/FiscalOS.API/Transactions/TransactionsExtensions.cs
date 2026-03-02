namespace FiscalOS.API.Transactions;

internal static class TransactionsExtensions
{
  private const string RouteGroupPrefix = "/transactions";

  public static RouteGroupBuilder MapTransactionsGroup(this WebApplication app)
  {
    var transactionsGroup = app.MapGroup(RouteGroupPrefix)
      .RequireAuthorization();

    if (app.Environment.IsProduction() is false)
    {
      transactionsGroup.MapFireWebhookEndpoint();
    }

    transactionsGroup.MapWebhookEndpoint().AllowAnonymous();

    return transactionsGroup;
  }
}