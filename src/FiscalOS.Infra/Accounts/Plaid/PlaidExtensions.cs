namespace FiscalOS.Infra.Accounts.Plaid;

public static class PlaidExtensions
{
#pragma warning disable CA1034
  extension(Going.Plaid.Entity.Account act)
  {
    public string CurrencyCode => act.Balances.IsoCurrencyCode
      ?? act.Balances.UnofficialCurrencyCode
      ?? "Unknown";

    public decimal Available => act.Balances.Available.GetValueOrDefault();
    public decimal Current => act.Balances.Current.GetValueOrDefault();
  }
}