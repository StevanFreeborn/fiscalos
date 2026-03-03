namespace FiscalOS.Infra.Transactions.Plaid;

public static class PlaidExtensions
{
#pragma warning disable CA1034
  extension(Transaction transaction)
  {
    public string Merchant => transaction.MerchantName ?? "Unknown merchant";
    public string Description => transaction.OriginalDescription ?? "";
    public DateTimeOffset PostedDate => transaction.Datetime ?? (
        transaction.Date.HasValue
          ? new DateTimeOffset(
              transaction.Date.Value.Year,
              transaction.Date.Value.Month,
              transaction.Date.Value.Day,
              0,
              0,
              0,
              TimeSpan.Zero
            )
          : throw new InvalidOperationException($"Transaction {transaction.TransactionId} does not have a valid date or datetime")
      );
  }
}