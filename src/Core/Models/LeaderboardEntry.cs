namespace MathStorm.Core.Models;

public class LeaderboardEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Difficulty { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string GameId { get; set; } = string.Empty;
    public double Score { get; set; }
    public DateTime AchievedAt { get; set; } = DateTime.UtcNow;
    public int Rank { get; set; }
}