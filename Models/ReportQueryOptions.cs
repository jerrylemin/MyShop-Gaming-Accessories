namespace ProjectTest.Models;

public class ReportQueryOptions
{
    public DateTime FromDate { get; set; } = new(DateTime.Today.Year, DateTime.Today.Month, 1);

    public DateTime ToDate { get; set; } = DateTime.Today;
}
