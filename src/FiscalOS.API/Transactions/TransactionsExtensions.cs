using FiscalOS.API.Transactions.DeleteById;
using FiscalOS.API.Transactions.FireWebhook;
using FiscalOS.API.Transactions.Get;
using FiscalOS.API.Transactions.Webhook;

namespace FiscalOS.API.Transactions;

internal static class TransactionsExtensions
{
  private const string RouteGroupPrefix = "/transactions";

  public static RouteGroupBuilder MapTransactionsGroup(this WebApplication app)
  {
    var transactionsGroup = app.MapGroup(RouteGroupPrefix)
      .RequireAuthorization();

    transactionsGroup.MapGetEndpoint();
    transactionsGroup.MapDeleteEndpoint();
    transactionsGroup.MapWebhookEndpoint().AllowAnonymous();

    if (app.Environment.IsProduction() is false)
    {
      transactionsGroup.MapFireWebhookEndpoint();
    }

    return transactionsGroup;
  }
}