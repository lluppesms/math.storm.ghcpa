namespace MathStorm.Core.Services;

public class MockCosmosDbService : ICosmosDbService
{
    private readonly List<GameUser> _users = [];
    private readonly List<Game> _games = [];
    private readonly List<LeaderboardEntry> _leaderboard = [];

    public Task<GameUser?> GetUserByUsernameAsync(string username)
    {
        var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task<GameUser> CreateUserAsync(string username)
    {
        var user = new GameUser
        {
            Id = Guid.NewGuid().ToString(),
            Username = username,
            GamesPlayed = 0,
            TotalScore = 0,
            BestScore = 0,
            CreatedAt = DateTime.UtcNow,
            LastPlayedAt = DateTime.UtcNow
        };
        _users.Add(user);
        return Task.FromResult(user);
    }

    public Task<GameUser> UpdateUserAsync(GameUser user)
    {
        var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
        if (existingUser != null)
        {
            existingUser.GamesPlayed = user.GamesPlayed;
            existingUser.TotalScore = user.TotalScore;
            existingUser.BestScore = user.BestScore;
            existingUser.LastPlayedAt = user.LastPlayedAt;
        }
        return Task.FromResult(user);
    }

    public Task<Game> CreateGameAsync(Game game)
    {
        game.Id = Guid.NewGuid().ToString();
        game.CompletedAt = DateTime.UtcNow;
        _games.Add(game);
        return Task.FromResult(game);
    }

    public Task<Game?> GetGameAsync(string gameId)
    {
        var game = _games.FirstOrDefault(g => g.Id == gameId);
        return Task.FromResult(game);
    }

    public Task<Game?> GetGameByIdAsync(string gameId)
    {
        // This is an alias to GetGameAsync for consistency with the web service interface
        return GetGameAsync(gameId);
    }

    public Task<bool> UpdateGameAnalysisAsync(string gameId, string analysis)
    {
        var game = _games.FirstOrDefault(g => g.Id == gameId);
        if (game == null)
        {
            return Task.FromResult(false);
        }

        game.Analysis = analysis;
        return Task.FromResult(true);
    }

    public Task<List<LeaderboardEntry>> GetLeaderboardAsync(string difficulty, int topCount = 10)
    {
        var entries = _leaderboard
            .Where(l => l.Difficulty.Equals(difficulty, StringComparison.OrdinalIgnoreCase))
            .OrderBy(l => l.Score)
            .Take(topCount)
            .ToList();

        // Update ranks
        for (int i = 0; i < entries.Count; i++)
        {
            entries[i].Rank = i + 1;
        }

        return Task.FromResult(entries);
    }

    public Task<List<LeaderboardEntry>> GetGlobalLeaderboardAsync(int topCount = 10)
    {
        var entries = _leaderboard
            .OrderBy(l => l.Score)
            .Take(topCount)
            .ToList();

        // Update ranks
        for (int i = 0; i < entries.Count; i++)
        {
            entries[i].Rank = i + 1;
        }

        return Task.FromResult(entries);
    }

    public Task<LeaderboardEntry?> AddToLeaderboardAsync(string userId, string username, string gameId, string difficulty, double score)
    {
        // Get all entries for this user in this difficulty (case-insensitive)
        var userEntries = _leaderboard
            .Where(l => l.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && 
                       l.Difficulty.Equals(difficulty, StringComparison.OrdinalIgnoreCase))
            .OrderBy(l => l.Score)
            .ToList();
        
        // Check if user already has 3 or more entries
        if (userEntries.Count >= 3)
        {
            // Check if new score is better (lower) than any existing score
            var worstUserScore = userEntries.Max(e => e.Score);
            if (score >= worstUserScore)
            {
                // New score is not better than any existing entry, don't add it
                return Task.FromResult<LeaderboardEntry?>(null);
            }
            
            // New score is better, remove the worst existing entry
            var worstUserEntry = userEntries.OrderByDescending(e => e.Score).First();
            _leaderboard.Remove(worstUserEntry);
        }
        
        var entry = new LeaderboardEntry
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Username = username,
            GameId = gameId,
            Difficulty = difficulty,
            Score = score,
            AchievedAt = DateTime.UtcNow,
            Rank = 1 // Will be updated when rankings are calculated
        };

        _leaderboard.Add(entry);
        return Task.FromResult<LeaderboardEntry?>(entry);
    }

    public Task UpdateLeaderboardRankingsAsync(string difficulty)
    {
        var entries = _leaderboard
            .Where(l => l.Difficulty.Equals(difficulty, StringComparison.OrdinalIgnoreCase))
            .OrderBy(l => l.Score)
            .ToList();

        for (int i = 0; i < entries.Count; i++)
        {
            entries[i].Rank = i + 1;
        }

        return Task.CompletedTask;
    }
}