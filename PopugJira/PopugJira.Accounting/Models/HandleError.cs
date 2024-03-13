using System.ComponentModel.DataAnnotations;

namespace PopugJira.Accounting.Models;

public class HandleError
{
    [Key]
    public int Id { get; set; }
    public required string Message { get; set; }
    public required string Error { get; set; }
}