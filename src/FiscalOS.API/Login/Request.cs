namespace FiscalOS.API.Login;

public record Request : IValidatableObject
{
  public string Username { get; init; } = string.Empty;
  public string Password { get; init; } = string.Empty;

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (string.IsNullOrWhiteSpace(Username))
    {
      var fieldName = nameof(Username);
      yield return new($"The {fieldName} field is required.", [fieldName]);
    }

    if (string.IsNullOrWhiteSpace(Password))
    {
      var fieldName = nameof(Password);
      yield return new($"The {fieldName} field is required.", [fieldName]);
    }
  }
}