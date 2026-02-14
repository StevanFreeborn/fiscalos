
namespace FiscalOS.Infra.Accounts.Plaid;

public sealed class PlaidService
{
  private readonly PlaidClient _client;

  private PlaidService(PlaidClient client)
  {
    _client = client;
  }

  public static PlaidService From(IServiceProvider sp)
  {
    var client = sp.GetRequiredService<PlaidClient>();
    return new(client);
  }

  public async Task<(string ItemId, string AccessToken)> ExchangeTokenAsync(string publicToken)
  {
    var ptr = await _client.ItemPublicTokenExchangeAsync(new()
    {
      PublicToken = publicToken,
    }).ConfigureAwait(false);

    if (ptr.IsSuccessStatusCode is false)
    {
      throw new PlaidException("Unable to exchange public token for access token");
    }

    return (ptr.ItemId, ptr.AccessToken);
  }

  public async Task<List<Going.Plaid.Entity.Account>> GetAccountsAsync(string accessToken)
  {
    var ar = await _client.AccountsGetAsync(new()
    {
      AccessToken = accessToken,
    }).ConfigureAwait(false);

    if (ar.IsSuccessStatusCode is false)
    {
      throw new PlaidException("Unable to retrieve accounts");
    }

    return [.. ar.Accounts];
  }

  public async Task<ItemWithConsentFields> GetItemAsync(string accessToken)
  {
    var ar = await _client.ItemGetAsync(new()
    {
      AccessToken = accessToken,
    }).ConfigureAwait(false);

    if (ar.IsSuccessStatusCode is false)
    {
      throw new PlaidException("Unable to retrieve item");
    }

    return ar.Item;
  }
}