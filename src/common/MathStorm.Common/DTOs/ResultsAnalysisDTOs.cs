namespace MathStorm.Common.DTOs;

public class ResultsAnalysisRequestDto
{
    public string GameId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public double TotalScore { get; set; }
    public List<QuestionResultDto> Questions { get; set; } = [];
    public string Personality { get; set; } = "default";
    public string Model { get; set; } = "gpt_4o_mini";
    public bool UserMadeLeaderboard { get; set; } = false;
}

public class ResultsAnalysisResponseDto
{
    public string GameId { get; set; } = string.Empty;
    public string Personality { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Analysis { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public enum AnalysisPersonality
{
    Default,
    ComedyRoast,
    Pirate,
    Limerick,
    SportsBroadcaster,
    Haiku,
    Australian,
    YourMother
}