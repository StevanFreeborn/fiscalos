namespace FiscalOS.Core.Authentication;

public interface ITokenGenerator
{
  string GenerateAccessToken(User user);
  RefreshToken GenerateRefreshToken(User user);
}