namespace FiscalOS.API.Transactions.FireWebhook;

public sealed record Request(
  string AccountId,
  string WebhookType,
  string WebhookCode
);