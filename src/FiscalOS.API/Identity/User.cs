namespace FiscalOS.API.Identity;

internal sealed class User
{
  public Guid Id { get; init; }
  public string Username { get; init; } = string.Empty;

  public ICollection<RefreshToken> RefreshTokens { get; init; } = [];
}