namespace FiscalOS.Tests.Common.Data;

public sealed class UserBuilder
{
  private string _username = "username";
  private string _password = "hashedPassword";
  private EncryptedDataKey _dataKey = EncryptedDataKey.From(
    "keyUsed",
    "encryptedKey"
  );
  private readonly List<Institution> _instituions = [];

  private UserBuilder()
  {
  }

  public static UserBuilder Create()
  {
    return new();
  }

  public UserBuilder WithUsername(string username)
  {
    _username = username;
    return this;
  }

  public UserBuilder WithPassword(string password)
  {
    _password = password;
    return this;
  }

  public UserBuilder WithDataKey(EncryptedDataKey key)
  {
    _dataKey = key;
    return this;
  }

  public UserBuilder WithInstitution(Action<InstitutionBuilder>? action = null)
  {
    var ib = InstitutionBuilder.Create();
    action?.Invoke(ib);
    _instituions.Add(ib.Build());
    return this;
  }

  public User Build()
  {
    var user = User.From(
      _username,
      _password,
      _dataKey
    );

    foreach (var institution in _instituions)
    {
      user.AddInstitution(institution);

      foreach (var account in institution.Accounts)
      {
        user.AddAccount(account);

        foreach (var transaction in account.Transactions)
        {
          user.AddTransaction(transaction);
        }
      }
    }

    return user;
  }
}