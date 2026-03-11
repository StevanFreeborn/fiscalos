using Transaction = FiscalOS.Core.Transactions.Transaction;

namespace FiscalOS.API.Transactions;

internal sealed record TransactionDto
{
  public Guid Id { get; init; }
  public string MerchantName { get; init; } = string.Empty;
  public decimal Amount { get; init; }
  public DateTimeOffset Date { get; init; }
  public string Description { get; init; } = string.Empty;

  [JsonConstructor]
  private TransactionDto()
  {
  }

  public static TransactionDto From(Transaction transaction)
  {
    return new TransactionDto
    {
      Id = transaction.Id,
      MerchantName = transaction.MerchantName,
      Amount = transaction.Amount,
      Date = transaction.Date,
      Description = transaction.Description
    };
  }
}