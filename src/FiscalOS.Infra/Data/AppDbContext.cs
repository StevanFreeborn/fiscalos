using Institution = FiscalOS.Core.Accounts.Institution;

namespace FiscalOS.Infra.Data;

public sealed class AppDbContext(
  IOptions<AppDbContextOptions> ctxOptions,
  IFileSystem fileSystem
) : DbContext
{
  private const string DataSourceKey = "Data Source=";
  private readonly AppDbContextOptions _ctxOptions = ctxOptions.Value;
  private readonly IFileSystem _fileSystem = fileSystem;

  public DbSet<User> Users => Set<User>();
  public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    var dbPath = _fileSystem.Path.GetFullPath(_ctxOptions.DatabaseFilePath, AppContext.BaseDirectory);
    var dbDirectory = _fileSystem.Path.GetDirectoryName(dbPath) ?? throw new InvalidOperationException("Database directory path could not be determined.");

    if (_fileSystem.Directory.Exists(dbDirectory) is false)
    {
      _fileSystem.Directory.CreateDirectory(dbDirectory);
    }

    var connectionString = $"{DataSourceKey}{dbPath}";

    optionsBuilder.UseSqlite(connectionString)
      .AddInterceptors(TimestampInterceptor.New());
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    var entityTypes = modelBuilder.Model.GetEntityTypes()
      .Where(static e => typeof(Entity).IsAssignableFrom(e.ClrType));

    foreach (var entityType in entityTypes)
    {
      modelBuilder.Entity(entityType.ClrType)
        .HasKey(nameof(Entity.Id));

      modelBuilder.Entity(entityType.ClrType)
        .Property(nameof(Entity.CreatedAt));

      modelBuilder.Entity(entityType.ClrType)
        .Property(nameof(Entity.UpdatedAt));
    }

    modelBuilder.Entity<User>(static eb =>
    {
      eb.Property(static u => u.Username);
      eb.HasIndex(static u => u.Username).IsUnique();

      eb.Property(static u => u.HashedPassword);

      eb.HasMany(static u => u.RefreshTokens)
        .WithOne(static t => t.User)
        .HasForeignKey(static t => t.UserId)
        .OnDelete(DeleteBehavior.Cascade);

      eb.HasMany(static u => u.Institutions)
        .WithOne()
        .HasForeignKey(static i => i.UserId)
        .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<RefreshToken>(static eb =>
    {
      eb.Property(static t => t.Id);
      eb.Property(static t => t.ExpiresAt);
      eb.Property(static t => t.UserId);

      eb.Property(static t => t.Token);
      eb.HasIndex(static t => t.Token).IsUnique();
    });

    modelBuilder.Entity<Institution>(static eb =>
    {
      eb.Property(static i => i.Name);

      eb.HasOne(static i => i.Metadata)
        .WithOne()
        .HasForeignKey<InstitutionMetadata>(static m => m.InstitutionId)
        .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<InstitutionMetadata>(static eb =>
    {
      eb.HasDiscriminator(static m => m.Type)
        .HasValue<PlaidMetadata>(PlaidMetadata.TypeValue);

      eb.Property(static m => m.InstitutionId);
      eb.Property(static m => m.Type);
    });

    modelBuilder.Entity<PlaidMetadata>(static eb =>
    {
      eb.HasBaseType<InstitutionMetadata>();

      eb.Property(static m => m.PlaidId);
      eb.Property(static m => m.PlaidName);
      eb.Property(static m => m.EncryptedAccessToken);
    });
  }
}