namespace FiscalOS.Infra.Authentication;

public static class Schemes
{
  public const string Default = JwtBearerDefaults.AuthenticationScheme;
  public const string AllowExpiredTokens = "AllowExpiredTokens";
}