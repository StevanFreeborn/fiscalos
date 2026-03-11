namespace FiscalOS.Infra.Accounts.Plaid;

public interface IPlaidAccountService
{
  Task<string> CreateLinkTokenAsync(string id);
  Task<(string ItemId, string AccessToken)> ExchangeTokenAsync(string publicToken);
  Task<List<Going.Plaid.Entity.Account>> GetAccountsAsync(string accessToken);
  Task<ItemWithConsentFields> GetItemAsync(string accessToken);
}

internal sealed class PlaidAccountService : IPlaidAccountService
{
  private readonly IOptions<PlaidClientOptions> _options;
  private readonly PlaidClient _client;

  private PlaidAccountService(PlaidClient client, IOptions<PlaidClientOptions> options)
  {
    _client = client;
    _options = options;
  }

  public static PlaidAccountService From(IServiceProvider sp)
  {
    var client = sp.GetRequiredService<PlaidClient>();
    var options = sp.GetRequiredService<IOptions<PlaidClientOptions>>();
    return new(client, options);
  }

  public async Task<string> CreateLinkTokenAsync(string id)
  {
    var assemblyName = Assembly.GetExecutingAssembly().GetName().FullName;
    var environmentName = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

    var linkTokenResponse = await _client.LinkTokenCreateAsync(new()
    {
      ClientName = $"{assemblyName}_{environmentName}",
      Products = [Products.Transactions],
      CountryCodes = [CountryCode.Us],
      Language = Language.English,
      User = new()
      {
        ClientUserId = id,
      },
      Webhook = _options.Value.Webhook,
    }).ConfigureAwait(false);

    if (linkTokenResponse.IsSuccessStatusCode is false)
    {
      throw new PlaidException(
        "Unable to create link token",
        linkTokenResponse.Error,
        linkTokenResponse.RequestId,
        (int?)linkTokenResponse.StatusCode
      );
    }

    return linkTokenResponse.LinkToken;
  }

  public async Task<(string ItemId, string AccessToken)> ExchangeTokenAsync(string publicToken)
  {
    var publicTokenResponse = await _client.ItemPublicTokenExchangeAsync(new()
    {
      PublicToken = publicToken,
    }).ConfigureAwait(false);

    if (publicTokenResponse.IsSuccessStatusCode is false)
    {
      throw new PlaidException(
        "Unable to exchange public token for access token",
        publicTokenResponse.Error,
        publicTokenResponse.RequestId,
        (int?)publicTokenResponse.StatusCode
      );
    }

    return (publicTokenResponse.ItemId, publicTokenResponse.AccessToken);
  }

  public async Task<List<Going.Plaid.Entity.Account>> GetAccountsAsync(string accessToken)
  {
    var accountResponse = await _client.AccountsGetAsync(new()
    {
      AccessToken = accessToken,
    }).ConfigureAwait(false);

    if (accountResponse.IsSuccessStatusCode is false)
    {
      throw new PlaidException(
        "Unable to retrieve accounts",
        accountResponse.Error,
        accountResponse.RequestId,
        (int?)accountResponse.StatusCode
      );
    }

    return [.. accountResponse.Accounts];
  }

  public async Task<ItemWithConsentFields> GetItemAsync(string accessToken)
  {
    var itemResponse = await _client.ItemGetAsync(new()
    {
      AccessToken = accessToken,
    }).ConfigureAwait(false);

    if (itemResponse.IsSuccessStatusCode is false)
    {
      throw new PlaidException(
        "Unable to retrieve item"
      );
    }

    return itemResponse.Item;
  }
}
