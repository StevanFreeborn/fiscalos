namespace FiscalOS.AdminCLI;

internal static class Commands
{
  public const string CreateUser = "Create User";
  public const string GenerateKey = "Generate Key";
  public const string Exit = "Exit";

  public static readonly string[] All = [
    CreateUser,
    GenerateKey,
    Exit
  ];
}