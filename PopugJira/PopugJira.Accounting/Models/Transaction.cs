using System.ComponentModel.DataAnnotations;

namespace PopugJira.Accounting.Models;

public class Transaction
{
    [Key]
    public int Id { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public int PopugId { get; set; }
    public int? TaskId { get; set; }
    public required string TransactionType { get; set; }
    public int Credit { get; set; }
    public int Debit { get; set; }
    public DateTimeOffset Date { get; set; }
    public required int BillingCycle { get; set; }
}
