namespace MathStorm.Core.Services;

public interface ICosmosDbService
{
    Task<GameUser?> GetUserByUsernameAsync(string username);
    Task<GameUser> CreateUserAsync(string username, string? pin = null);
    Task<GameUser> UpdateUserAsync(GameUser user);
    Task<bool> ValidateUserAsync(string username, string? pin);

    Task<Game> CreateGameAsync(Game game);
    Task<Game?> GetGameAsync(string gameId);
    Task<Game?> GetGameByIdAsync(string gameId);
    Task<bool> UpdateGameAnalysisAsync(string gameId, string analysis);

    Task<List<LeaderboardEntry>> GetLeaderboardAsync(string difficulty, int topCount = 10);
    Task<List<LeaderboardEntry>> GetGlobalLeaderboardAsync(int topCount = 10);
    Task<LeaderboardEntry?> AddToLeaderboardAsync(string userId, string username, string gameId, string difficulty, double score);
    Task UpdateLeaderboardRankingsAsync(string difficulty);
}