namespace MathStorm.Core;

/// <summary>
/// Service interface for game operations.
/// Replaces HTTP calls to Azure Functions with direct method calls.
/// </summary>
public interface IMathStormService
{
    // Game Operations
    Task<GameResponseDto> CreateGame(Difficulty difficulty);
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
