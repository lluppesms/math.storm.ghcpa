using MathStorm.Web.Models;

namespace MathStorm.Web.Services;

public class MockCosmosDbService : ICosmosDbService
{
    private readonly List<GameUser> _users = [];
    private readonly List<Game> _games = [];
    private readonly List<LeaderboardEntry> _leaderboard = [];
    private readonly ILogger<MockCosmosDbService> _logger;

    public MockCosmosDbService(ILogger<MockCosmosDbService> logger)
    {
        _logger = logger;
        // Add some sample leaderboard data
        SeedSampleData();
    }

    private void SeedSampleData()
    {
        // Add some sample users and leaderboard entries for testing
        var sampleUsers = new[]
        {
            new GameUser { Id = "1", Username = "MathWiz", TotalGamesPlayed = 12 },
            new GameUser { Id = "2", Username = "SpeedyCalc", TotalGamesPlayed = 8 },
            new GameUser { Id = "3", Username = "NumberNinja", TotalGamesPlayed = 15 },
            new GameUser { Id = "4", Username = "QuickMath", TotalGamesPlayed = 6 },
            new GameUser { Id = "5", Username = "CalcMaster", TotalGamesPlayed = 9 },
            new GameUser { Id = "6", Username = "FastFigures", TotalGamesPlayed = 7 }
        };
        _users.AddRange(sampleUsers);

        var sampleEntries = new[]
        {
            // Beginner difficulty (at least 3 entries)
            new LeaderboardEntry { Id = "1", UserId = "1", Username = "MathWiz", Difficulty = "Beginner", Score = 45.2, Rank = 1, AchievedAt = DateTime.UtcNow.AddDays(-1) },
            new LeaderboardEntry { Id = "2", UserId = "4", Username = "QuickMath", Difficulty = "Beginner", Score = 52.7, Rank = 2, AchievedAt = DateTime.UtcNow.AddDays(-2) },
            new LeaderboardEntry { Id = "3", UserId = "5", Username = "CalcMaster", Difficulty = "Beginner", Score = 58.9, Rank = 3, AchievedAt = DateTime.UtcNow.AddDays(-3) },
            
            // Novice difficulty (at least 3 entries)
            new LeaderboardEntry { Id = "4", UserId = "2", Username = "SpeedyCalc", Difficulty = "Novice", Score = 67.3, Rank = 1, AchievedAt = DateTime.UtcNow.AddDays(-1) },
            new LeaderboardEntry { Id = "5", UserId = "6", Username = "FastFigures", Difficulty = "Novice", Score = 72.1, Rank = 2, AchievedAt = DateTime.UtcNow.AddDays(-2) },
            new LeaderboardEntry { Id = "6", UserId = "3", Username = "NumberNinja", Difficulty = "Novice", Score = 78.4, Rank = 3, AchievedAt = DateTime.UtcNow.AddDays(-4) },
            
            // Intermediate difficulty (at least 3 entries)
            new LeaderboardEntry { Id = "7", UserId = "1", Username = "MathWiz", Difficulty = "Intermediate", Score = 98.4, Rank = 1, AchievedAt = DateTime.UtcNow.AddDays(-1) },
            new LeaderboardEntry { Id = "8", UserId = "2", Username = "SpeedyCalc", Difficulty = "Intermediate", Score = 112.7, Rank = 2, AchievedAt = DateTime.UtcNow.AddDays(-2) },
            new LeaderboardEntry { Id = "9", UserId = "5", Username = "CalcMaster", Difficulty = "Intermediate", Score = 125.6, Rank = 3, AchievedAt = DateTime.UtcNow.AddDays(-3) },
            
            // Expert difficulty (at least 3 entries)
            new LeaderboardEntry { Id = "10", UserId = "1", Username = "MathWiz", Difficulty = "Expert", Score = 156.7, Rank = 1, AchievedAt = DateTime.UtcNow.AddDays(-2) },
            new LeaderboardEntry { Id = "11", UserId = "2", Username = "SpeedyCalc", Difficulty = "Expert", Score = 189.3, Rank = 2, AchievedAt = DateTime.UtcNow.AddDays(-1) },
            new LeaderboardEntry { Id = "12", UserId = "3", Username = "NumberNinja", Difficulty = "Expert", Score = 203.1, Rank = 3, AchievedAt = DateTime.UtcNow.AddDays(-3) }
        };
        _leaderboard.AddRange(sampleEntries);
    }

    public async Task<GameUser?> GetUserByUsernameAsync(string username)
    {
        await Task.Delay(50); // Simulate network delay
        return _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<GameUser> CreateUserAsync(string username)
    {
        await Task.Delay(50);
        var user = new GameUser
        {
            Id = Guid.NewGuid().ToString(),
            Username = username,
            CreatedAt = DateTime.UtcNow
        };
        _users.Add(user);
        return user;
    }

    public async Task<GameUser> UpdateUserAsync(GameUser user)
    {
        await Task.Delay(50);
        var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
        if (existingUser != null)
        {
            var index = _users.IndexOf(existingUser);
            _users[index] = user;
        }
        return user;
    }

    public async Task<Game> CreateGameAsync(Game game)
    {
        await Task.Delay(50);
        _games.Add(game);
        return game;
    }

    public async Task<Game?> GetGameAsync(string gameId)
    {
        await Task.Delay(50);
        return _games.FirstOrDefault(g => g.Id == gameId);
    }

    public async Task<List<LeaderboardEntry>> GetLeaderboardAsync(string difficulty, int topCount = 10)
    {
        await Task.Delay(50);
        var entries = _leaderboard
            .Where(e => e.Difficulty == difficulty)
            .OrderBy(e => e.Score)
            .Take(topCount)
            .ToList();
            
        // Update ranks
        for (int i = 0; i < entries.Count; i++)
        {
            entries[i].Rank = i + 1;
        }
        
        return entries;
    }

    public async Task<LeaderboardEntry?> AddToLeaderboardAsync(string userId, string username, string gameId, string difficulty, double score)
    {
        await Task.Delay(50);
        
        // Check if this score qualifies for top 10
        var currentLeaderboard = await GetLeaderboardAsync(difficulty, 10);
        
        // If leaderboard has less than 10 entries or this score is better than the worst score
        if (currentLeaderboard.Count < 10 || score < currentLeaderboard.Last().Score)
        {
            var entry = new LeaderboardEntry
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Username = username,
                GameId = gameId,
                Difficulty = difficulty,
                Score = score,
                AchievedAt = DateTime.UtcNow
            };
            
            _leaderboard.Add(entry);
            
            // Remove entries beyond top 10
            var allEntriesForDifficulty = _leaderboard
                .Where(e => e.Difficulty == difficulty)
                .OrderBy(e => e.Score)
                .ToList();
                
            if (allEntriesForDifficulty.Count > 10)
            {
                var entriesToRemove = allEntriesForDifficulty.Skip(10).ToList();
                foreach (var entryToRemove in entriesToRemove)
                {
                    _leaderboard.Remove(entryToRemove);
                }
            }
            
            await UpdateLeaderboardRankingsAsync(difficulty);
            return entry;
        }
        
        return null;
    }

    public async Task UpdateLeaderboardRankingsAsync(string difficulty)
    {
        await Task.Delay(50);
        
        var entries = _leaderboard
            .Where(e => e.Difficulty == difficulty)
            .OrderBy(e => e.Score)
            .ToList();
            
        for (int i = 0; i < entries.Count; i++)
        {
            entries[i].Rank = i + 1;
        }
    }
}