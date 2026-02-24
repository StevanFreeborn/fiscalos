namespace FiscalOS.API.Institutions.Get;

internal sealed record Response
{
  public IEnumerable<InstitutionDto> Institutions { get; init; } = [];

  [JsonConstructor]
  private Response()
  {
  }

  public static Response From(IEnumerable<InstitutionDto> institutions)
  {
    return new Response
    {
      Institutions = institutions,
    };
  }
}

internal sealed record AccountDto
{
  public string Id { get; init; } = string.Empty;
  public string Name { get; init; } = string.Empty;

  [JsonConstructor]
  private AccountDto()
  {
  }

  public static AccountDto From(Account account)
  {
    return new()
    {
      Id = account.Id.ToString(),
      Name = account.Name,
    };
  }
}

internal sealed record InstitutionDto
{
  public string Id { get; init; } = string.Empty;
  public string Name { get; init; } = string.Empty;
  public IEnumerable<AccountDto> Accounts { get; init; } = [];

  [JsonConstructor]
  private InstitutionDto()
  {
  }

  public static InstitutionDto From(Institution institution)
  {
    return new()
    {
      Id = institution.Id.ToString(),
      Name = institution.Name,
      Accounts = institution.Accounts.Select(AccountDto.From),
    };
  }
}