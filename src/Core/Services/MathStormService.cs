using MathStorm.Core.Models;

namespace MathStorm.Core;

/// <summary>
/// Implementation of IMathStormService that provides direct method calls
/// instead of HTTP calls to Azure Functions.
/// </summary>
public class MathStormService : IMathStormService
{
    private readonly ILogger<MathStormService> _logger;
    private readonly IGameService _gameService;
    private readonly ICosmosDbService _cosmosDbService;
    private readonly IResultsAnalysisService _analysisService;

    public MathStormService(
        ILogger<MathStormService> logger,
        IGameService gameService,
        ICosmosDbService cosmosDbService,
        IResultsAnalysisService analysisService)
    {
        _logger = logger;
        _gameService = gameService;
        _cosmosDbService = cosmosDbService;
        _analysisService = analysisService;
    }

    public async Task<GameResponseDto> CreateGame(Difficulty difficulty)
    {
        _logger.LogInformation($"Creating game with difficulty: {difficulty}");

        var gameSession = _gameService.CreateNewGame(difficulty);

        var gameDto = new GameResponseDto
        {
            GameId = Guid.NewGuid().ToString(),
            Difficulty = difficulty.ToString(),
            Questions = gameSession.Questions.Select(q => new QuestionDto
            {
                Id = q.Id,
                Number1 = q.Number1,
                Number2 = q.Number2,
                Operation = q.Operation.ToString(),
                CorrectAnswer = q.CorrectAnswer,
                QuestionText = q.QuestionText
            }).ToList()
        };
        return gameDto;
    }

    public async Task<Game?> GetGameByIdAsync(string gameId)
    {
        _logger.LogInformation($"Getting game by ID: {gameId}");

        if (string.IsNullOrEmpty(gameId))
        {
            _logger.LogWarning("GetGameByIdAsync called with null/empty gameId");
            return null;
        }

        return await _cosmosDbService.GetGameByIdAsync(gameId);
    }

    public async Task<bool> UpdateGameAnalysisAsync(string gameId, string analysis)
    {
        _logger.LogInformation($"Updating game analysis for gameId: {gameId}");

        if (string.IsNullOrEmpty(gameId))
        {
            _logger.LogWarning("UpdateGameAnalysisAsync called with null/empty gameId");
            return false;
        }

        return await _cosmosDbService.UpdateGameAnalysisAsync(gameId, analysis);
    }

    public async Task<GameResultsResponseDto?> SubmitGameResultsAsync(GameResultsRequestDto request)
    {
        _logger.LogInformation($"Submitting game results for user: {request.Username}");

        if (request == null)
        {
            _logger.LogWarning("SubmitGameResultsAsync called with null request");
            return null;
        }

        // Get user by Username
        var user = await _cosmosDbService.GetUserByUsernameAsync(request.Username);
        if (user == null)
        {
            user = await _cosmosDbService.CreateUserAsync(request.Username);
            if (user == null)
            {
                _logger.LogWarning($"User not found: {request.Username}");
                return null;
            }
        }

        // Create game record
        var game = new Game
        {
            Id = request.GameId,
            UserId = request.UserId,
            Username = request.Username,
            Difficulty = request.Difficulty,
            TotalScore = request.Questions.Sum(q => q.Score),
            CompletedAt = DateTime.UtcNow,
            Analysis = request.Analysis,
            Questions = request.Questions.Select(q => new GameQuestion
            {
                Id = q.Id,
                Number1 = q.Number1,
                Number2 = q.Number2,
                Operation = q.Operation,
                CorrectAnswer = q.CorrectAnswer,
                UserAnswer = q.UserAnswer,
                TimeInSeconds = q.TimeInSeconds,
                PercentageDifference = q.PercentageDifference,
                Score = q.Score
            }).ToList()
        };

        // Save game
        await _cosmosDbService.CreateGameAsync(game);

        // Update user statistics
        user.GamesPlayed++;
        user.TotalScore += game.TotalScore;
        if (game.TotalScore < user.BestScore || user.BestScore == 0)
        {
            user.BestScore = game.TotalScore;
        }
        user.LastPlayedAt = DateTime.UtcNow;
        await _cosmosDbService.UpdateUserAsync(user);

        // Do NOT add to leaderboard yet - that happens after analysis is complete
        return new GameResultsResponseDto
        {
            GameId = request.GameId,
            TotalScore = game.TotalScore,
            AddedToLeaderboard = false,
            LeaderboardRank = null
        };
    }

    public async Task<LeaderboardResponseDto?> GetLeaderboardAsync(string? difficulty = null, int topCount = 10)
    {
        _logger.LogInformation($"Getting leaderboard - difficulty: {difficulty}, topCount: {topCount}");

        var entries = string.IsNullOrEmpty(difficulty)
            ? await _cosmosDbService.GetGlobalLeaderboardAsync(topCount)
            : await _cosmosDbService.GetLeaderboardAsync(difficulty, topCount);

        return new LeaderboardResponseDto
        {
            Difficulty = difficulty,
            Entries = entries.Select(entry => new LeaderboardEntryDto
            {
                Id = entry.Id,
                Difficulty = entry.Difficulty,
                Username = entry.Username,
                UserId = entry.UserId,
                GameId = entry.GameId,
                Score = entry.Score,
                AchievedAt = entry.AchievedAt,
                Rank = entry.Rank
            }).ToList()
        };
    }

    public async Task<GameResultsResponseDto?> AddGameToLeaderboardAsync(string gameId)
    {
        _logger.LogInformation($"Adding game to leaderboard: {gameId}");

        if (string.IsNullOrEmpty(gameId))
        {
            _logger.LogWarning("AddGameToLeaderboardAsync called with null/empty gameId");
            return null;
        }

        // Get the game record
        var game = await _cosmosDbService.GetGameByIdAsync(gameId);
        if (game == null)
        {
            _logger.LogWarning($"Game not found: {gameId}");
            return null;
        }

        // Add to leaderboard
        var leaderboardEntry = await _cosmosDbService.AddToLeaderboardAsync(
            game.UserId, game.Username, game.Id, game.Difficulty, game.TotalScore);

        // Update rankings
        await _cosmosDbService.UpdateLeaderboardRankingsAsync(game.Difficulty);

        return new GameResultsResponseDto
        {
            GameId = game.Id,
            TotalScore = game.TotalScore,
            AddedToLeaderboard = leaderboardEntry != null,
            LeaderboardRank = leaderboardEntry?.Rank
        };
    }

    public async Task<ResultsAnalysisResponseDto?> AnalyzeGameResultsAsync(ResultsAnalysisRequestDto request)
    {
        _logger.LogInformation($"Analyzing game results for user: {request.Username}");

        if (request == null)
        {
            _logger.LogWarning("AnalyzeGameResultsAsync called with null request");
            return null;
        }

        // Set defaults
        if (string.IsNullOrEmpty(request.Personality))
        {
            request.Personality = "default";
        }
        if (string.IsNullOrEmpty(request.Model))
        {
            request.Model = "gpt-4o-mini";
        }

        var analysis = await _analysisService.AnalyzeGameResultsAsync(request);

        return new ResultsAnalysisResponseDto
        {
            GameId = request.GameId,
            Personality = request.Personality,
            Model = request.Model,
            Analysis = analysis,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<UserAuthResponseDto?> AuthenticateUserAsync(UserAuthRequestDto request)
    {
        _logger.LogInformation($"Authenticating user: {request.Username}");

        if (request == null || string.IsNullOrWhiteSpace(request.Username))
        {
            _logger.LogWarning("AuthenticateUserAsync called with null/empty username");
            return null;
        }

        // Validate username format
        if (request.Username.Length < 2 || request.Username.Length > 20)
        {
            return new UserAuthResponseDto
            {
                Username = request.Username,
                IsAuthenticated = false,
                ErrorMessage = "Username must be between 2 and 20 characters"
            };
        }

        // Check if user exists
        var existingUser = await _cosmosDbService.GetUserByUsernameAsync(request.Username);
        var response = new UserAuthResponseDto
        {
            Username = request.Username
        };

        if (existingUser == null)
        {
            // Create new user
            var newUser = await _cosmosDbService.CreateUserAsync(request.Username);
            response.IsAuthenticated = true;
            response.IsNewUser = true;
            response.UserId = newUser.Id;
        }
        else
        {
            // User exists, authenticate them
            response.IsAuthenticated = true;
            response.IsNewUser = false;
            response.UserId = existingUser.Id;
        }

        return response;
    }
}
