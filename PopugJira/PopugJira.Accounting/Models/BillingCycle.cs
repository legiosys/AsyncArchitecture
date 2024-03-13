namespace PopugJira.Accounting.Models;

public class BillingCycle
{
    public int Id { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset? End { get; set; }
}