namespace FiscalOS.API.Institutions.GetAvailable;

internal sealed record Response
{
  public IEnumerable<AccountDto> Accounts { get; init; } = [];

  [JsonConstructor]
  private Response()
  {
  }

  public static Response From(IEnumerable<AccountDto> accounts)
  {
    return new Response
    {
      Accounts = accounts
    };
  }
}

internal sealed record AccountDto
{
  public string ProviderId { get; init; } = string.Empty;
  public string ProviderName { get; init; } = string.Empty;

  [JsonConstructor]
  private AccountDto()
  {
  }

  public static AccountDto FromPlaidAccount(Going.Plaid.Entity.Account plaidAccount)
  {
    return new AccountDto
    {
      ProviderId = plaidAccount.AccountId,
      ProviderName = plaidAccount.Name
    };
  }
}