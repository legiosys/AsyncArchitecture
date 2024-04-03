namespace PopugJira.Tracker.Contracts;

public record Task_Changed_V1(Guid TaskId, string Description, Guid Assignee, string Status, string ChangeType);
public record Task_Changed_V2(Guid TaskId, string Description, string? JiraId, Guid Assignee, string Status, string ChangeType);

public static class TaskChangedTypes
{
    public const string Created = "created";
}