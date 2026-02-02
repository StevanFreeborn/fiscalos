namespace FiscalOS.Core.Authentication;

public interface IPasswordHasher
{
  string Hash(string password);
  bool Verify(string providedPassword, string hashedPassword);
}