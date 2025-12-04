namespace MathStorm.Common.DTOs;

public class LeaderboardRequestDto
{
    public string? Difficulty { get; set; }
    public int TopCount { get; set; } = 10;
}

public class LeaderboardResponseDto
{
    public string? Difficulty { get; set; }
    public List<LeaderboardEntryDto> Entries { get; set; } = [];
}

public class LeaderboardEntryDto
{
    public string Id { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string GameId { get; set; } = string.Empty;
    public double Score { get; set; }
    public DateTime AchievedAt { get; set; }
    public int Rank { get; set; }
}