using Newtonsoft.Json;

namespace MathStorm.Common.Models;

public class GameUser
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonProperty("username")]
    public string Username { get; set; } = string.Empty;
    
    [JsonProperty("gamesPlayed")]
    public int GamesPlayed { get; set; }
    
    [JsonProperty("totalScore")]
    public double TotalScore { get; set; }
    
    [JsonProperty("bestScore")]
    public double BestScore { get; set; }
    
    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonProperty("lastPlayedAt")]
    public DateTime LastPlayedAt { get; set; } = DateTime.UtcNow;
}