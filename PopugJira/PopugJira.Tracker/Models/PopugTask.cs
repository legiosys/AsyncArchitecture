namespace PopugJira.Tracker.Models;

public class PopugTask
{
    public required Guid Id { get; init; }
    public required Guid Assignee { get; set; }
    public required string Description { get; set; }
    public string? JiraId { get; set; }
    public required string Status { get; set; }
}

public static class PopugTaskStatus
{
    public const string Active = "active";
    public const string Completed = "completed";
}