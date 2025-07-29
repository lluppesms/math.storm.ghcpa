using Newtonsoft.Json;

namespace MathStorm.Shared.Models;

public class LeaderboardEntry
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonProperty("difficulty")]
    public string Difficulty { get; set; } = string.Empty;
    
    [JsonProperty("username")]
    public string Username { get; set; } = string.Empty;
    
    [JsonProperty("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [JsonProperty("gameId")]
    public string GameId { get; set; } = string.Empty;
    
    [JsonProperty("score")]
    public double Score { get; set; }
    
    [JsonProperty("achievedAt")]
    public DateTime AchievedAt { get; set; } = DateTime.UtcNow;
    
    [JsonProperty("rank")]
    public int Rank { get; set; }
}