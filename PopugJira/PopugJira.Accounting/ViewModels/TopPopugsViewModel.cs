namespace PopugJira.Accounting.ViewModels;

public record TopPopugsViewModel(int Today, List<PreviousEarn> PreviousEarns);
public record PreviousEarn(int Earned, DateTimeOffset From, DateTimeOffset To);