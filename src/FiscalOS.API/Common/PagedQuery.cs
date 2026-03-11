namespace FiscalOS.API.Common;

internal sealed record PagedQuery
{
  public int PageNumber { get; init; }
  public int PageSize { get; init; }

  public static async ValueTask<PagedQuery> BindAsync(HttpContext context)
  {
    var pageNumber = int.TryParse(context.Request.Query["pageNumber"], out var pn) ? pn : 1;
    var pageSize = int.TryParse(context.Request.Query["pageSize"], out var ps) ? ps : 1000;

    return new PagedQuery
    {
      PageNumber = pageNumber,
      PageSize = pageSize
    };
  }

  public Dictionary<string, string[]> Validate()
  {
    var validationResults = new Dictionary<string, string[]>();

    if (PageNumber <= 0)
    {
      validationResults[nameof(PageNumber)] = ["PageNumber must be greater than 0."];
    }

    if (PageSize <= 0 || PageSize > 1000)
    {
      validationResults[nameof(PageSize)] = ["PageSize must be between 1 and 1000."];
    }

    return validationResults;
  }
}