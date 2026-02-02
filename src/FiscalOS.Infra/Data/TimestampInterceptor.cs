using FiscalOS.Core.Data;

namespace FiscalOS.Infra.Data;

internal sealed class TimestampInterceptor : SaveChangesInterceptor
{
  public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
  {
    UpdateEntities(eventData.Context);
    return base.SavingChanges(eventData, result);
  }

  public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
  {
    UpdateEntities(eventData.Context);
    return base.SavingChangesAsync(eventData, result, cancellationToken);
  }

  private static void UpdateEntities(DbContext? context)
  {
    if (context is null)
    {
      return;
    }

    var entries = context.ChangeTracker
      .Entries<Entity>()
      .Where(static e => e.State is EntityState.Added or EntityState.Modified);

    var utcNow = DateTimeOffset.UtcNow;

    foreach (var entry in entries)
    {
      if (entry.State == EntityState.Added)
      {
        entry.Entity.SetCreatedAt(utcNow);
      }

      entry.Entity.SetUpdatedAt(utcNow);
    }
  }
}