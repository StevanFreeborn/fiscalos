namespace FiscalOS.API.Tests.Unit;

public class PagedResponseTests
{
  [Fact]
  public void From_WhenCalled_ItShouldReturnPageWithCorrectValues()
  {
    var pageNumber = 1;
    var pageSize = 10;
    var totalItems = 25;
    string[] items = ["test"];

    var results = PagedResponse<string>.From(pageNumber, pageSize, totalItems, items);

    results.PageNumber.Should().Be(pageNumber);
    results.PageSize.Should().Be(pageSize);
    results.TotalItems.Should().Be(totalItems);
    results.TotalPages.Should().Be(3);
    results.Items.Should().BeEquivalentTo(items);
  }
}