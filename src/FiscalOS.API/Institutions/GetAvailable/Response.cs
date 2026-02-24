namespace FiscalOS.API.Institutions.GetAvailable;

internal sealed record Response
{
  public IEnumerable<AvailableAccountDto> Accounts { get; init; } = [];

  [JsonConstructor]
  private Response()
  {
  }

  public static Response From(IEnumerable<AvailableAccountDto> accounts)
  {
    return new Response
    {
      Accounts = accounts
    };
  }
}

internal sealed record AvailableAccountDto
{
  public string ProviderInstitutionId { get; init; } = string.Empty;
  public string ProviderId { get; init; } = string.Empty;
  public string ProviderName { get; init; } = string.Empty;
  public decimal CurrentBalance { get; init; }
  public decimal AvailableBalance { get; init; }
  public string CurrencyCode { get; init; } = string.Empty;

  [JsonConstructor]
  private AvailableAccountDto()
  {
  }

  public static AvailableAccountDto From(
    PlaidMetadata plaidMetadata,
    Going.Plaid.Entity.Account plaidAccount
  )
  {
    return new AvailableAccountDto
    {
      ProviderInstitutionId = plaidMetadata.PlaidId,
      ProviderId = plaidAccount.AccountId,
      ProviderName = plaidAccount.Name,
      AvailableBalance = plaidAccount.Available,
      CurrentBalance = plaidAccount.Current,
      CurrencyCode = plaidAccount.CurrencyCode,
    };
  }
}