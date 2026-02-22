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

internal sealed record InstitutionDto
{
  public string Id { get; init; } = string.Empty;
  public string Name { get; init; } = string.Empty;

  [JsonConstructor]
  private InstitutionDto()
  {
  }

  public static InstitutionDto FromInstitution(Institution institution)
  {
    return new InstitutionDto
    {
      Id = institution.Id.ToString(),
      Name = institution.Name,
    };
  }
}