namespace MathStorm.Web.Services;

public class BuildInfo
{
    public string BuildDate { get; set; } = string.Empty;
    public string BuildNumber { get; set; } = string.Empty;
    public string BuildId { get; set; } = string.Empty;
    public string RunId { get; set; } = string.Empty;
    public string RunAttempt { get; set; } = string.Empty;
    public string RunNumber { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string CommitHash { get; set; } = string.Empty;
}