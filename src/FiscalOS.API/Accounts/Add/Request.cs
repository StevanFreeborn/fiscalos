namespace FiscalOS.API.Accounts.Add;

public record Request : IValidatableObject
{
  public string ProviderInstitutionId { get; init; } = string.Empty;
  public string ProviderAccountId { get; init; } = string.Empty;
  public string ProviderAccountName { get; init; } = string.Empty;

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (string.IsNullOrWhiteSpace(ProviderInstitutionId))
    {
      var fieldName = nameof(ProviderInstitutionId);
      yield return new($"The {fieldName} field is required.", [fieldName]);
    }

    if (string.IsNullOrWhiteSpace(ProviderAccountId))
    {
      var fieldName = nameof(ProviderAccountId);
      yield return new($"The {fieldName} field is required.", [fieldName]);
    }

    if (string.IsNullOrWhiteSpace(ProviderAccountName))
    {
      var fieldName = nameof(ProviderAccountName);
      yield return new($"The {fieldName} field is required.", [fieldName]);
    }
  }
}