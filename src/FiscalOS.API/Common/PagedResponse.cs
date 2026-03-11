namespace FiscalOS.API.Common;

internal sealed record PagedResponse<T>
{
  public int PageNumber { get; init; }
  public int PageSize { get; init; }
  public int TotalItems { get; init; }
  public int TotalPages { get; init; }
  public T[] Items { get; init; } = [];

  [JsonConstructor]
  private PagedResponse()
  {
  }

  public static PagedResponse<T> From(
    int pageNumber,
    int pageSize,
    int totalItems,
    T[] items
  )
  {
    var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

    return new PagedResponse<T>
    {
      PageNumber = pageNumber,
      PageSize = pageSize,
      TotalItems = totalItems,
      TotalPages = totalPages,
      Items = items
    };
  }
}