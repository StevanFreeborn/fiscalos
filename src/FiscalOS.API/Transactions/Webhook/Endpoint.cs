namespace FiscalOS.API.Transactions.Webhook;

internal static class Endpoint
{
  private const string Route = "/webhook";

  public static RouteHandlerBuilder MapWebhookEndpoint(this RouteGroupBuilder groupBuilder)
  {
    return groupBuilder.MapPost(Route, HandleAsync);
  }

  private static async Task<IResult> HandleAsync(
    [FromBody] WebhookBase request,
    [FromServices] ILogger<Program> logger,
    [FromServices] IAsyncQueue<SyncUpdatesQueueItem> queue,
    CancellationToken ct
  )
  {
    logger.LogInformation(
      "Received {WebhookType} webhook with {WebhookCode} payload",
      request.WebhookType,
      request.WebhookCode
    );

    switch (request)
    {
      case SyncUpdatesAvailableWebhook webhook:
        await queue.EnqueueAsync(new(webhook.ItemId), ct);
        logger.LogInformation(
          "Enqueued {WebhookType} webhook with {WebhookCode} payload",
          request.WebhookType,
          request.WebhookCode
        );
        break;
      default:
        break;
    }

    return Results.Ok();
  }
}