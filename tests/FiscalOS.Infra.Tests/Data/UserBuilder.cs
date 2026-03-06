namespace FiscalOS.Infra.Tests.Data;

// protected async Task<User> CreateTestUserAsync()
// {
//   var user = User.From(
//     "username",
//     "hashedPassword",
//     EncryptedDataKey.From("keyUsed", "encryptedKey")
//   );
//
//   var institutionMetadata = PlaidInstitutionMetadata.From(
//     "plaidId",
//     "plaidName",
//     "encryptedAccessToken",
//     "itemId"
//   );
//   var institution = Institution.From("institutionName", institutionMetadata);
//   user.AddInstitution(institution);
//
//   var accountMetadata = PlaidAccountMetadata.From("plaidId", "plaidName");
//   var account = Account.From("some account", accountMetadata);
//   user.AddAccount(account);
//   institution.AddAccount(account);
//
//   var transactionMetadata = PlaidTransactionMetadata.From("plaidId");
//   var transaction = Transaction.From(
//     "merchantName",
//     "description",
//     100,
//     DateTimeOffset.UtcNow,
//     transactionMetadata
//   );
//   user.AddTransaction(transaction);
//   account.AddTransaction(transaction);
//
//   await AppDbContext.AddAsync(user, TestContext.Current.CancellationToken);
//   await AppDbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
//
//   return user;
// }
internal sealed class UserBuilder
{
  private string _username = "username";
  private string _password = "hashedPassword";
  private EncryptedDataKey _dataKey = EncryptedDataKey.From(
    "keyUsed",
    "encryptedKey"
  );
  private readonly List<InstitutionBuilder> _instituionBuilders = [];

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
    _instituionBuilders.Add(ib);
    return this;
  }

  public User Build()
  {
    var user = User.From(
      _username,
      _password,
      _dataKey
    );

    foreach (var ib in _instituionBuilders)
    {
      var institution = ib.WithUser(user).Build();
      user.AddInstitution(institution);
    }

    return user;
  }
}