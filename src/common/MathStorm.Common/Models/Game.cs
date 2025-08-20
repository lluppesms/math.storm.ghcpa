using System.Text.Json.Serialization;

namespace MathStorm.Common.Models;

public class Game
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = string.Empty;

    [JsonPropertyName("totalScore")]
    public double TotalScore { get; set; }

    [JsonPropertyName("completedAt")]
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("analysis")]
    public string? Analysis { get; set; }

    [JsonPropertyName("questions")]
    public List<GameQuestion> Questions { get; set; } = [];
}

public class GameQuestion
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("number1")]
    public int Number1 { get; set; }

    [JsonPropertyName("number2")]
    public int Number2 { get; set; }

    [JsonPropertyName("operation")]
    public string Operation { get; set; } = string.Empty;

    [JsonPropertyName("correctAnswer")]
    public double CorrectAnswer { get; set; }

    [JsonPropertyName("userAnswer")]
    public double? UserAnswer { get; set; }

    [JsonPropertyName("timeInSeconds")]
    public double TimeInSeconds { get; set; }

    [JsonPropertyName("percentageDifference")]
    public double PercentageDifference { get; set; }

    [JsonPropertyName("score")]
    public double Score { get; set; }
}