using MathStorm.Shared.Models;

namespace MathStorm.Shared.Services;

public interface ICosmosDbService
{
    Task<GameUser?> GetUserByUsernameAsync(string username);
    Task<GameUser> CreateUserAsync(string username);
    Task<GameUser> UpdateUserAsync(GameUser user);
    
    Task<Game> CreateGameAsync(Game game);
    Task<Game?> GetGameAsync(string gameId);
    
    Task<List<LeaderboardEntry>> GetLeaderboardAsync(string difficulty, int topCount = 10);
    Task<List<LeaderboardEntry>> GetGlobalLeaderboardAsync(int topCount = 10);
    Task<LeaderboardEntry?> AddToLeaderboardAsync(string userId, string username, string gameId, string difficulty, double score);
    Task UpdateLeaderboardRankingsAsync(string difficulty);
}