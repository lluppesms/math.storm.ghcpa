using System.Text.Json.Serialization;


namespace MathStorm.Common.Models;

public class LeaderboardEntry
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = string.Empty;
    
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
    
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [JsonPropertyName("gameId")]
    public string GameId { get; set; } = string.Empty;
    
    [JsonPropertyName("score")]
    public double Score { get; set; }
    
    [JsonPropertyName("achievedAt")]
    public DateTime AchievedAt { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("rank")]
    public int Rank { get; set; }
}