using MathStorm.Services;

namespace MathStorm.Console.Services;

public interface IConsoleMathStormService
{
    GameResponseDto? GetGame(Difficulty difficulty);
    Task<GameResultsResponseDto?> SubmitGameResultsAsync(GameResultsRequestDto request);
    Task<LeaderboardResponseDto?> GetLeaderboardAsync(string? difficulty = null, int topCount = 10);
    Task<ResultsAnalysisResponseDto?> AnalyzeGameResultsAsync(ResultsAnalysisRequestDto request);
    Task<UserAuthResponseDto?> AuthenticateUserAsync(UserAuthRequestDto request);
}

public class ConsoleMathStormService : IConsoleMathStormService
{
    private readonly IMathStormService _mathStormService;
    private readonly ILogger<ConsoleMathStormService> _logger;

    public ConsoleMathStormService(IMathStormService mathStormService, ILogger<ConsoleMathStormService> logger)
    {
        _mathStormService = mathStormService;
        _logger = logger;
    }

    public GameResponseDto? GetGame(Difficulty difficulty)
    {
        try
        {
            return _mathStormService.CreateGame(difficulty);
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error getting game: {ex.Message}");
            _logger.LogError(ex, "Error getting game");
            AnsiConsole.MarkupLine("[red]Error: Unable to create game. Please try again later.[/]");
            return null;
        }
    }

    public async Task<GameResultsResponseDto?> SubmitGameResultsAsync(GameResultsRequestDto request)
    {
        try
        {
            return await _mathStormService.SubmitGameResultsAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error submitting game results: {ex.Message}");
            _logger.LogError(ex, "Error submitting game results");
            AnsiConsole.MarkupLine("[red]Error: Unable to submit results. Your score may not be saved.[/]");
            return null;
        }
    }

    public async Task<LeaderboardResponseDto?> GetLeaderboardAsync(string? difficulty = null, int topCount = 10)
    {
        try
        {
            return await _mathStormService.GetLeaderboardAsync(difficulty, topCount);
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error getting leaderboard: {ex.Message}");
            _logger.LogError(ex, "Error getting leaderboard");
            AnsiConsole.MarkupLine("[red]Error: Unable to load leaderboard. Please try again later.[/]");
            return null;
        }
    }

    public async Task<ResultsAnalysisResponseDto?> AnalyzeGameResultsAsync(ResultsAnalysisRequestDto request)
    {
        try
        {
            return await _mathStormService.AnalyzeGameResultsAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error analyzing game results: {ex.Message}");
            _logger.LogError(ex, "Error analyzing game results");
            AnsiConsole.MarkupLine("[red]Error: Unable to analyze results. Please try again later.[/]");
            return null;
        }
    }

    public async Task<UserAuthResponseDto?> AuthenticateUserAsync(UserAuthRequestDto request)
    {
        try
        {
            return await _mathStormService.AuthenticateUserAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error authenticating user: {ex.Message}");
            _logger.LogError(ex, "Error authenticating user");
            AnsiConsole.MarkupLine("[red]Error: Unable to authenticate user. Please try again later.[/]");
            return null;
        }
    }
}