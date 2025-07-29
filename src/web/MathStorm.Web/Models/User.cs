using Newtonsoft.Json;

namespace MathStorm.Web.Models;

public class GameUser
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonProperty("username")]
    public string Username { get; set; } = string.Empty;
    
    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonProperty("totalGamesPlayed")]
    public int TotalGamesPlayed { get; set; } = 0;
    
    [JsonProperty("bestScores")]
    public Dictionary<string, double> BestScores { get; set; } = new();
}