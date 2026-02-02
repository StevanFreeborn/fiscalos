using FiscalOS.Core.Data;

namespace FiscalOS.Infra.Data;

public sealed class AppDbContext(IOptions<AppDbContextOptions> ctxOptions) : DbContext
{
  private const string DataSourceKey = "Data Source=";
  private readonly AppDbContextOptions _ctxOptions = ctxOptions.Value;

  public DbSet<User> Users => Set<User>();
  public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    var dbPath = _ctxOptions.GetFullyQualifiedDatabasePath();
    var dbDirectory = Path.GetDirectoryName(dbPath) ?? throw new InvalidOperationException("Database directory path could not be determined.");

    if (Directory.Exists(dbDirectory) is false)
    {
      Directory.CreateDirectory(dbDirectory);
    }

    var connectionString = $"{DataSourceKey}{dbPath}";

    optionsBuilder.UseSqlite(connectionString)
      .AddInterceptors(new TimestampInterceptor());
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
      eb.HasMany(static u => u.RefreshTokens)
        .WithOne(static t => t.User)
        .HasForeignKey(static t => t.UserId)
        .OnDelete(DeleteBehavior.Cascade);

      eb.Property(static u => u.Username);
      eb.Property(static u => u.HashedPassword);
    });

    modelBuilder.Entity<RefreshToken>(static eb =>
    {
      eb.Property(static t => t.Id);
      eb.Property(static t => t.ExpiresAt);
      eb.Property(static t => t.Token);
    });
  }
}