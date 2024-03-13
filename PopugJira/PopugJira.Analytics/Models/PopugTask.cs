using System.ComponentModel.DataAnnotations;

namespace PopugJira.Analytics.Models;

public class PopugTask
{
    [Key]
    public int Id { get; set; }
    public Guid ReplicationId { get; set; }
    public string? Description { get; set; }
    public int AssignPrice { get; set; }
    public int CompletePrice { get; set; }
    public DateTimeOffset? CompleteDate { get; set; }
}