namespace MathStorm.Core.Models;

public class GameUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; } = string.Empty;
    public int GamesPlayed { get; set; }
    public double TotalScore { get; set; }
    public double BestScore { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastPlayedAt { get; set; } = DateTime.UtcNow;
}