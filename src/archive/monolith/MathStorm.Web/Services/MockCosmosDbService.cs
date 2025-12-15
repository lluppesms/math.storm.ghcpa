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
            new LeaderboardEntry { Id = "1", UserId = "1", Username = "MathWiz", GameId = "game_1", Difficulty = "Beginner", Score = 45.2, Rank = 1, AchievedAt = DateTime.UtcNow.AddDays(-1), Analysis = "Excellent performance! You showed great accuracy and speed in solving basic arithmetic problems. Your quick thinking really paid off!" },
            new LeaderboardEntry { Id = "2", UserId = "4", Username = "QuickMath", GameId = "game_2", Difficulty = "Beginner", Score = 52.7, Rank = 2, AchievedAt = DateTime.UtcNow.AddDays(-2), Analysis = "Good job! You're getting the hang of mental math. A few more practice sessions and you'll be even faster!" },
            new LeaderboardEntry { Id = "3", UserId = "5", Username = "CalcMaster", GameId = "game_3", Difficulty = "Beginner", Score = 58.9, Rank = 3, AchievedAt = DateTime.UtcNow.AddDays(-3), Analysis = "Nice work! Your calculation skills are solid. Focus on improving your speed for even better scores." },
            
            // Novice difficulty (at least 3 entries)
            new LeaderboardEntry { Id = "4", UserId = "2", Username = "SpeedyCalc", GameId = "game_4", Difficulty = "Novice", Score = 67.3, Rank = 1, AchievedAt = DateTime.UtcNow.AddDays(-1), Analysis = "Outstanding! You're mastering all four operations with impressive speed and accuracy. Keep up the great work!" },
            new LeaderboardEntry { Id = "5", UserId = "6", Username = "FastFigures", GameId = "game_5", Difficulty = "Novice", Score = 72.1, Rank = 2, AchievedAt = DateTime.UtcNow.AddDays(-2), Analysis = "Well done! Your multiplication and division skills are really improving. Just a bit more practice on speed!" },
            new LeaderboardEntry { Id = "6", UserId = "3", Username = "NumberNinja", GameId = "game_6", Difficulty = "Novice", Score = 78.4, Rank = 3, AchievedAt = DateTime.UtcNow.AddDays(-4), Analysis = "Good progress! You're handling the mix of operations well. Keep working on those mental calculation shortcuts." },
            
            // Intermediate difficulty (at least 3 entries)
            new LeaderboardEntry { Id = "7", UserId = "1", Username = "MathWiz", GameId = "game_7", Difficulty = "Intermediate", Score = 98.4, Rank = 1, AchievedAt = DateTime.UtcNow.AddDays(-1), Analysis = "Phenomenal! Your ability to handle 3-digit calculations quickly and accurately is truly impressive. You're a math champion!" },
            new LeaderboardEntry { Id = "8", UserId = "2", Username = "SpeedyCalc", GameId = "game_8", Difficulty = "Intermediate", Score = 112.7, Rank = 2, AchievedAt = DateTime.UtcNow.AddDays(-2), Analysis = "Excellent work! You're tackling intermediate problems with confidence. Your calculation strategies are paying off." },
            new LeaderboardEntry { Id = "9", UserId = "5", Username = "CalcMaster", GameId = "game_9", Difficulty = "Intermediate", Score = 125.6, Rank = 3, AchievedAt = DateTime.UtcNow.AddDays(-3), Analysis = "Great job! You're showing steady improvement in handling complex calculations. Keep practicing!" },
            
            // Expert difficulty (at least 3 entries)
            new LeaderboardEntry { Id = "10", UserId = "1", Username = "MathWiz", GameId = "game_10", Difficulty = "Expert", Score = 156.7, Rank = 1, AchievedAt = DateTime.UtcNow.AddDays(-2), Analysis = "Absolutely incredible! You've mastered the most challenging level with exceptional skill. You're truly an expert mathematician!" },
            new LeaderboardEntry { Id = "11", UserId = "2", Username = "SpeedyCalc", GameId = "game_11", Difficulty = "Expert", Score = 189.3, Rank = 2, AchievedAt = DateTime.UtcNow.AddDays(-1), Analysis = "Impressive performance! Handling 4-digit calculations at this speed shows remarkable mathematical ability." },
            new LeaderboardEntry { Id = "12", UserId = "3", Username = "NumberNinja", GameId = "game_12", Difficulty = "Expert", Score = 203.1, Rank = 3, AchievedAt = DateTime.UtcNow.AddDays(-3), Analysis = "Excellent work on the expert level! Your persistence and skill are evident in this challenging performance." }
        };
        _leaderboard.AddRange(sampleEntries);

        // Add sample game data with questions for testing modal functionality
        var sampleGames = new[]
        {
            new Game 
            { 
                Id = "game_1", UserId = "1", Username = "MathWiz", Difficulty = "Beginner", TotalScore = 45.2, 
                CompletedAt = DateTime.UtcNow.AddDays(-1),
                Analysis = "Excellent performance! You showed great accuracy and speed in solving basic arithmetic problems. Your quick thinking really paid off!",
                Questions = 
                [
                    new GameQuestion { Id = 1, Number1 = 23, Number2 = 17, Operation = "Addition", CorrectAnswer = 40, UserAnswer = 40, TimeInSeconds = 2.1, PercentageDifference = 0, Score = 8.5 },
                    new GameQuestion { Id = 2, Number1 = 45, Number2 = 28, Operation = "Subtraction", CorrectAnswer = 17, UserAnswer = 17, TimeInSeconds = 1.8, Score = 7.2 },
                    new GameQuestion { Id = 3, Number1 = 34, Number2 = 19, Operation = "Addition", CorrectAnswer = 53, UserAnswer = 53, TimeInSeconds = 2.3, Score = 9.1 },
                    new GameQuestion { Id = 4, Number1 = 67, Number2 = 29, Operation = "Subtraction", CorrectAnswer = 38, UserAnswer = 38, TimeInSeconds = 1.9, Score = 8.7 },
                    new GameQuestion { Id = 5, Number1 = 56, Number2 = 34, Operation = "Addition", CorrectAnswer = 90, UserAnswer = 90, TimeInSeconds = 2.0, Score = 11.7 }
                ]
            },
            new Game 
            { 
                Id = "game_7", UserId = "1", Username = "MathWiz", Difficulty = "Intermediate", TotalScore = 98.4, 
                CompletedAt = DateTime.UtcNow.AddDays(-1),
                Analysis = "Phenomenal! Your ability to handle 3-digit calculations quickly and accurately is truly impressive. You're a math champion!",
                Questions = 
                [
                    new GameQuestion { Id = 1, Number1 = 234, Number2 = 167, Operation = "Addition", CorrectAnswer = 401, UserAnswer = 401, TimeInSeconds = 3.2, Score = 12.1 },
                    new GameQuestion { Id = 2, Number1 = 456, Number2 = 289, Operation = "Subtraction", CorrectAnswer = 167, UserAnswer = 167, TimeInSeconds = 2.8, Score = 10.5 },
                    new GameQuestion { Id = 3, Number1 = 123, Number2 = 45, Operation = "Multiplication", CorrectAnswer = 5535, UserAnswer = 5535, TimeInSeconds = 4.1, Score = 15.3 }
                ]
            }
        };
        _games.AddRange(sampleGames);
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

    public async Task<Game?> GetGameWithDetailsByIdAsync(string gameId)
    {
        return await GetGameAsync(gameId);
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

    public async Task<List<LeaderboardEntry>> GetGlobalLeaderboardAsync(int topCount = 10)
    {
        await Task.Delay(50);
        var entries = _leaderboard
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
                return null;
            }
            
            // New score is better, remove the worst existing entry
            var worstUserEntry = userEntries.OrderByDescending(e => e.Score).First();
            _leaderboard.Remove(worstUserEntry);
        }
        
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