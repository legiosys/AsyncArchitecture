namespace PopugJira.Tracker.Contracts;

public record TaskOperationPerformed(Guid Id, string ChangeType, Guid Assignee);

public static class TaskOperationTypes
{
    public const string AddedNew = "added";
    public const string Assigned = "assigned";
    public const string Completed = "completed";
}