namespace FiscalOS.API.Login;

public record LoginRequest : IValidatableObject
{
  public string Username { get; init; } = string.Empty;
  public string Password { get; init; } = string.Empty;

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (string.IsNullOrWhiteSpace(Username))
    {
      yield return new("The Username field is required.", [nameof(Username)]);
    }

    if (string.IsNullOrWhiteSpace(Password))
    {
      yield return new("The Password field is required.", [nameof(Password)]);
    }
  }
}