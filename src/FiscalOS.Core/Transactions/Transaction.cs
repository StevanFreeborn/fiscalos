namespace FiscalOS.Core.Transactions;

public sealed class Transaction : Entity
{
  public Guid UserId { get; private set; }
  public User? User { get; init; }

  public Guid AccountId { get; init; }
  public Account? Account { get; init; }

  public string MerchantName { get; private set; } = string.Empty;
  public string Description { get; init; } = string.Empty;
  public decimal Amount { get; init; }
  public DateTimeOffset Date { get; init; }

  public TransactionMetadata? Metadata { get; init; }

  private Transaction()
  {
  }

  public static Transaction From(
    string merchantName,
    string description,
    decimal? amount,
    DateTimeOffset? date,
    TransactionMetadata metadata
  )
  {
    return From(Guid.Empty, Guid.Empty, Guid.Empty, merchantName, description, amount, date, metadata);
  }

  public static Transaction From(
    Guid userId,
    Guid accountId,
    string merchantName,
    string description,
    decimal? amount,
    DateTimeOffset? date,
    TransactionMetadata metadata
  )
  {
    return From(Guid.Empty, userId, accountId, merchantName, description, amount, date, metadata);
  }

  public static Transaction From(
    Guid id,
    Guid userId,
    Guid accountId,
    string merchantName,
    string description,
    decimal? amount,
    DateTimeOffset? date,
    TransactionMetadata metadata
  )
  {
    ArgumentNullException.ThrowIfNull(merchantName, nameof(merchantName));
    ArgumentNullException.ThrowIfNull(description, nameof(description));

    if (amount.HasValue is false)
    {
      throw new ArgumentNullException(nameof(amount));
    }

    if (date.HasValue is false)
    {
      throw new ArgumentNullException(nameof(date));
    }

    return new()
    {
      Id = id,
      UserId = userId,
      AccountId = accountId,
      MerchantName = merchantName,
      Description = description,
      Amount = amount.Value,
      Date = date.Value,
      Metadata = metadata,
    };
  }
}