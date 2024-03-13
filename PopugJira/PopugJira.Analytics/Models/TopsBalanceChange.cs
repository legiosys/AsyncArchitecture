using System.ComponentModel.DataAnnotations;

namespace PopugJira.Analytics.Models;

public class TopsBalanceChange
{
    [Key]
    public int Id { get; set; }
    public DateTimeOffset Date { get; set; }
    public int Debit { get; set; }
    public int Credit { get; set; }
}