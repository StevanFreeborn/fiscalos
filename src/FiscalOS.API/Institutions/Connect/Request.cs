namespace FiscalOS.API.Institutions.Connect;

public record Request : IValidatableObject
{
  public string PublicToken { get; init; } = string.Empty;
  public string PlaidInstitutionId { get; init; } = string.Empty;

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (string.IsNullOrWhiteSpace(PublicToken))
    {
      var fieldName = nameof(PublicToken);
      yield return new($"The {fieldName} field is required.", [fieldName]);
    }

    if (string.IsNullOrWhiteSpace(PlaidInstitutionId))
    {
      var fieldName = nameof(PlaidInstitutionId);
      yield return new($"The {fieldName} field is required.", [fieldName]);
    }
  }
}