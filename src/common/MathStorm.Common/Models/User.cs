using System.Text.Json.Serialization;


namespace MathStorm.Common.Models;

public class GameUser
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("pin")]
    public string? Pin { get; set; }

    [JsonPropertyName("gamesPlayed")]
    public int GamesPlayed { get; set; }

    [JsonPropertyName("totalScore")]
    public double TotalScore { get; set; }

    [JsonPropertyName("bestScore")]
    public double BestScore { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("lastPlayedAt")]
    public DateTime LastPlayedAt { get; set; } = DateTime.UtcNow;
}