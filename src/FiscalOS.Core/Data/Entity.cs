namespace FiscalOS.Core.Data;

public abstract class Entity
{
  public Guid Id { get; init; }
  public DateTimeOffset CreatedAt { get; private set; }
  public DateTimeOffset UpdatedAt { get; private set; }

  public void SetCreatedAt(DateTimeOffset createdAt)
  {
    CreatedAt = createdAt;
  }

  public void SetUpdatedAt(DateTimeOffset updatedAt)
  {
    UpdatedAt = updatedAt;
  }
}