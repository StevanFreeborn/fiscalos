namespace FiscalOS.AdminCLI;

internal sealed class App(
  IAnsiConsole console,
  IPasswordHasher passwordHasher,
  IServiceScopeFactory serviceScopeFactory
) : IHostedService
{
  private readonly IAnsiConsole _console = console;
  private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    using var scope = _serviceScopeFactory.CreateScope();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var selectionPrompt = new SelectionPrompt<string>()
      .Title("What do you want to do?")
      .AddChoices(Commands.All);

    var command = await _console.PromptAsync(selectionPrompt, cancellationToken);

    switch (command)
    {
      case Commands.CreateUser:
        var username = await _console.AskAsync<string>("Enter the [green]username[/]:", cancellationToken);
        var password = _console.Prompt(
          new TextPrompt<string>("Enter the [green]password[/]:")
            .PromptStyle("red")
            .Secret()
        );

        var hashedPassword = passwordHasher.Hash(password);
        var user = User.From(username, hashedPassword);

        await appDbContext.Users.AddAsync(user, cancellationToken);
        await appDbContext.SaveChangesAsync(cancellationToken);

        _console.MarkupLine($"User [green]{username}[/] created successfully.");
        break;
      case Commands.Exit:
      default:
        break;
    }
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}
