using System.ComponentModel.DataAnnotations;

namespace PopugJira.Accounting.Models;

public class Popug
{
    [Key]
    public int Id { get; set; }
    public required Guid ReplicationId { get; init; }
    public required string Position { get; set; }
    public int Balance { get; set; } = 0;
}