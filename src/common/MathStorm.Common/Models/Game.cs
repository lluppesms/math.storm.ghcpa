namespace MathStorm.Common.Models;

public class Game
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonProperty("username")]
    public string Username { get; set; } = string.Empty;

    [JsonProperty("difficulty")]
    public string Difficulty { get; set; } = string.Empty;

    [JsonProperty("totalScore")]
    public double TotalScore { get; set; }

    [JsonProperty("completedAt")]
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    [JsonProperty("analysis")]
    public string? Analysis { get; set; }

    [JsonProperty("questions")]
    public List<GameQuestion> Questions { get; set; } = [];
}

public class GameQuestion
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("number1")]
    public int Number1 { get; set; }

    [JsonProperty("number2")]
    public int Number2 { get; set; }

    [JsonProperty("operation")]
    public string Operation { get; set; } = string.Empty;

    [JsonProperty("correctAnswer")]
    public double CorrectAnswer { get; set; }

    [JsonProperty("userAnswer")]
    public double? UserAnswer { get; set; }

    [JsonProperty("timeInSeconds")]
    public double TimeInSeconds { get; set; }

    [JsonProperty("percentageDifference")]
    public double PercentageDifference { get; set; }

    [JsonProperty("score")]
    public double Score { get; set; }
}