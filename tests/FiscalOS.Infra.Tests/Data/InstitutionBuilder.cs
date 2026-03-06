namespace FiscalOS.Infra.Tests.Data;

internal sealed class InstitutionBuilder
{
  private User? _user;
  private string _name = "institutionName";
  private InstitutionMetadata? _metadata;
  private readonly List<Account> _accounts = [];

  private InstitutionBuilder()
  {
  }

  public static InstitutionBuilder Create()
  {
    return new();
  }

  public InstitutionBuilder WithUser(User user)
  {
    _user = user;
    return this;
  }

  public InstitutionBuilder WithName(string name)
  {
    _name = name;
    return this;
  }

  public InstitutionBuilder WithMetadata(Action<InstitutionMetadataBuilder>? action = null)
  {
    var imb = InstitutionMetadataBuilder.Create();
    action?.Invoke(imb);
    _metadata = imb.Build();
    return this;
  }

  public InstitutionBuilder WithAccount(Action<AccountBuilder>? action = null)
  {
    var ab = AccountBuilder.Create();
    action?.Invoke(ab);
    _accounts.Add(ab.Build());
    return this;
  }

  public Institution Build()
  {
    if (_metadata is null)
    {
      throw new InvalidOperationException($"You must call {nameof(WithMetadata)} prior to building");
    }

    var institution = Institution.From(
      _user,
      _name,
      _metadata
    );

    foreach (var account in _accounts)
    {
      institution.AddAccount(account);
    }

    return institution;
  }
}