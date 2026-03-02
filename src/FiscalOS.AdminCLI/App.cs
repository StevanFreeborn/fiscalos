namespace FiscalOS.AdminCLI;

internal sealed class App(
  IAnsiConsole console,
  IPasswordHasher passwordHasher,
  IServiceScopeFactory serviceScopeFactory,
  IEncryptor encryptor,
  IKeyRing keyRing
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
        var userEncryptionKey = await encryptor.GenerateEncryptedKeyAsync(cancellationToken);
        var user = User.From(username, hashedPassword, userEncryptionKey);

        await appDbContext.Users.AddAsync(user, cancellationToken);
        await appDbContext.SaveChangesAsync(cancellationToken);

        _console.MarkupLine($"User [green]{username}[/] created successfully.");
        break;
      case Commands.GenerateKey:
        var key = encryptor.GenerateKey();
        var keyRingEntry = await keyRing.SaveKeyAsync(key);
        _console.MarkupLine($"Key generated with id [green]{keyRingEntry.KeyId}[/] generated successfully.");
        break;
      case Commands.RemoveInstitution:
        var institutionId = await _console.AskAsync<string>("Enter the [green]institution id[/] of the item you want to remove:", cancellationToken);

        var institution = await appDbContext.Institutions
          .Include(i => i.User)
          .Include(i => i.Metadata)
          .FirstOrDefaultAsync(cancellationToken);

        if (institution is null)
        {
          _console.MarkupLine($"Institution with id [green]{institutionId}[/] not found.");
          break;
        }

        if (institution.User is null)
        {
          _console.MarkupLine($"Institution with id [green]{institutionId}[/] has no user.");
          break;
        }

        if (institution.Metadata is not PlaidInstitutionMetadata plaidInstitutionMetadata)
        {
          _console.MarkupLine($"Institution with id [green]{institutionId}[/] has no plaid metadata.");
          break;
        }

        var accessToken = await encryptor.DecryptAsyncFor(institution.User, plaidInstitutionMetadata.EncryptedAccessToken, cancellationToken);

        var plaidClient = scope.ServiceProvider.GetRequiredService<PlaidClient>();

        var removeItemResponse = await plaidClient.ItemRemoveAsync(new()
        {
          AccessToken = accessToken,
        });

        if (removeItemResponse.IsSuccessStatusCode is false)
        {
          _console.MarkupLine($"Failed to remove item for institution id [green]{institutionId}[/]. Plaid error: {removeItemResponse.Error?.ErrorMessage}");
          break;
        }

        appDbContext.Institutions.Remove(institution);
        await appDbContext.SaveChangesAsync(cancellationToken);

        _console.MarkupLine($"Institution with id [green]{institutionId}[/] removed successfully.");
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