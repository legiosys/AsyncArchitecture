using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PopugJira.Accounting.Models;

public class PopugTask
{
    [Key]
    public int Id { get; set; }
    public Guid ReplicationId { get; set; }
    public string? Description { get; set; }
    public int AssignPrice { get; set; }
    public int CompletePrice { get; set; }
}