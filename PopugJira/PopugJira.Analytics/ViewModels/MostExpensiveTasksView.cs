namespace PopugJira.Analytics.ViewModels;

public record MostExpensiveTasksView(MostExpensiveTask? ForDate, MostExpensiveTask? ForWeek, MostExpensiveTask? ForMonth);
public record MostExpensiveTask(int Price, string Description);