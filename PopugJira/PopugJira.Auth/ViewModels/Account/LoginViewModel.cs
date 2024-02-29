namespace PopugJira.Auth.ViewModels.Account;

public record LoginViewModel
{
    public required string Login { get; set; }
    public required string Password { get; set; }
    public bool Remember { get; set; }
    public string? Error { get; set; }
    public string? ReturnUrl { get; set; }
}