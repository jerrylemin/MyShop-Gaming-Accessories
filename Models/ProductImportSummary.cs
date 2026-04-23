namespace ProjectTest.Models;

public class ProductImportSummary
{
    public int CreatedCount { get; set; }

    public int UpdatedCount { get; set; }

    public int CategoryCount { get; set; }

    public int TotalCount => CreatedCount + UpdatedCount;
}
