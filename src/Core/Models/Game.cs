namespace MathStorm.Core.Models;

public class Game
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public double TotalScore { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    public string? Analysis { get; set; }
    public List<GameQuestion> Questions { get; set; } = [];
}

public class GameQuestion
{
    public int Id { get; set; }
    public int Number1 { get; set; }
    public int Number2 { get; set; }
    public string Operation { get; set; } = string.Empty;
    public double CorrectAnswer { get; set; }
    public double? UserAnswer { get; set; }
    public double TimeInSeconds { get; set; }
    public double PercentageDifference { get; set; }
    public double Score { get; set; }
    public double AccuracyScore { get; set; }
    public double TimeScore { get; set; }
}