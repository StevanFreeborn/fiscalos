namespace FiscalOS.Infra.Tests.Unit;

public class PasswordHasherTests
{
  private readonly PasswordHasher _sut = PasswordHasher.New();

  [Fact]
  public void Hash_WhenGivenPassword_ShouldReturnHashedPassword()
  {
    var password = "SecurePassword123!";

    var result = _sut.Hash(password);

    result.Should().NotBeNullOrEmpty();
    result.Should().NotBe(password);
  }

  [Fact]
  public void Verify_WhenGivenCorrectPassword_ShouldReturnTrue()
  {
    var password = "SecurePassword123!";
    var hashedPassword = _sut.Hash(password);

    var result = _sut.Verify(password, hashedPassword);

    result.Should().BeTrue();
  }

  [Fact]
  public void Verify_WhenGivenIncorrectPassword_ShouldReturnFalse()
  {
    var password = "SecurePassword123!";
    var wrongPassword = "WrongPassword!";
    var hashedPassword = _sut.Hash(password);

    var result = _sut.Verify(wrongPassword, hashedPassword);

    result.Should().BeFalse();
  }
}