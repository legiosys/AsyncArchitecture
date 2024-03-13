namespace PopugJira.Tracker.Contracts;

public record Task_Added_V1(Guid TaskId, Guid Assignee);
public record Task_Assigned_V1(Guid TaskId, Guid Assignee);
public record Task_Completed_V1(Guid TaskId, Guid Assignee);
