namespace PopugJira.Auth.ViewModels.Account;

public record RegisterViewModel
{
    public required string Login { get; set; }
    public required string Password { get; set; }
    public required string Position { get; set; }
    public string? Error { get; set; }
}