namespace MathStorm.Core;

public interface IMathStormService
{
    // Game Operations
    Task<GameResponseDto> CreateGame(Difficulty difficulty, GameMode gameMode = GameMode.Classic);
    Task<Game?> GetGameByIdAsync(string gameId);
    Task<bool> UpdateGameAnalysisAsync(string gameId, string analysis);

    // Game Results Operations
    Task<GameResultsResponseDto?> SubmitGameResultsAsync(GameResultsRequestDto request);

    // Leaderboard Operations
    Task<LeaderboardResponseDto?> GetLeaderboardAsync(string? difficulty = null, int topCount = 10);
    Task<GameResultsResponseDto?> AddGameToLeaderboardAsync(string gameId);

    // Results Analysis Operations
    Task<ResultsAnalysisResponseDto?> AnalyzeGameResultsAsync(ResultsAnalysisRequestDto request);

    // User Authentication Operations
    Task<UserAuthResponseDto?> AuthenticateUserAsync(UserAuthRequestDto request);
}
