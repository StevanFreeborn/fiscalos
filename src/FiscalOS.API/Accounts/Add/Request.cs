namespace FiscalOS.API.Accounts.Add;

public record Request : IValidatableObject
{
  public string PlaidInstitutionId { get; init; } = string.Empty;
  public string PlaidAccountId { get; init; } = string.Empty;
  public string PlaidAccountName { get; init; } = string.Empty;
  public decimal AccountCurrentBalance { get; init; }
  public decimal AccountAvailableBalance { get; init; }

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (string.IsNullOrWhiteSpace(PlaidInstitutionId))
    {
      var fieldName = nameof(PlaidInstitutionId);
      yield return new($"The {fieldName} field is required.", [fieldName]);
    }

    if (string.IsNullOrWhiteSpace(PlaidAccountId))
    {
      var fieldName = nameof(PlaidAccountId);
      yield return new($"The {fieldName} field is required.", [fieldName]);
    }

    if (string.IsNullOrWhiteSpace(PlaidAccountName))
    {
      var fieldName = nameof(PlaidAccountName);
      yield return new($"The {fieldName} field is required.", [fieldName]);
    }
  }
}