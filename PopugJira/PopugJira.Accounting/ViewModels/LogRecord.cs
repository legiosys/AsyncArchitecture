namespace PopugJira.Accounting.ViewModels;

public record LogRecord(DateTimeOffset Date, int Change, string Description);