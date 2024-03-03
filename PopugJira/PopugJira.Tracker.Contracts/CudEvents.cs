namespace PopugJira.Tracker.Contracts;

public record TaskChanged(Guid TaskId, string Description, Guid Assignee, string Status, string ChangeType);

public static class TaskChangedTypes
{
    public const string Created = "created";
    public const string Assigned = "assigned";
    public const string Completed = "completed";
}