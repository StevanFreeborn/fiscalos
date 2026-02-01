namespace FiscalOS.API.Login;

public record LoginRequest(
  [Required]
  string Username,
  [Required]
  string Password
);