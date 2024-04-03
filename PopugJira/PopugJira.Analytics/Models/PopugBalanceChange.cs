using System.ComponentModel.DataAnnotations;

namespace PopugJira.Analytics.Models;

public class PopugBalanceChange
{
    [Key]
    public int Id { get; set; }
    public Guid PopugId { get; set; }
    public int Debit { get; set; }
    public int Credit { get; set; }
    public DateTimeOffset Date { get; set; }
    public bool ZeroBalance { get; set; } = false;
}