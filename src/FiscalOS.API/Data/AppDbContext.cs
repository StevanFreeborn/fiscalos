namespace FiscalOS.API.Data;

internal sealed class AppDbContext(IOptions<AppDbContextOptions> ctxOptions) : DbContext
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

    optionsBuilder.UseSqlite(connectionString);
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<User>()
      .Property(static u => u.Id)
      .ValueGeneratedOnAdd();

    modelBuilder.Entity<RefreshToken>()
      .Property(static t => t.Id)
      .ValueGeneratedOnAdd();
  }
}