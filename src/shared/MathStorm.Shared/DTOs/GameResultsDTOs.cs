namespace MathStorm.Shared.DTOs;

public class GameResultsRequestDto
{
    public string GameId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public List<QuestionResultDto> Questions { get; set; } = [];
}

public class QuestionResultDto
{
    public int Id { get; set; }
    public int Number1 { get; set; }
    public int Number2 { get; set; }
    public string Operation { get; set; } = string.Empty;
    public double CorrectAnswer { get; set; }
    public double UserAnswer { get; set; }
    public double TimeInSeconds { get; set; }
    public double PercentageDifference { get; set; }
    public double Score { get; set; }
}

public class GameResultsResponseDto
{
    public string GameId { get; set; } = string.Empty;
    public double TotalScore { get; set; }
    public bool AddedToLeaderboard { get; set; }
    public int? LeaderboardRank { get; set; }
}