using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace MathStorm.Core.Services;

/// <summary>
/// SQL Server implementation of the data service using the mathstorm schema.
/// </summary>
public class SqlDbService : IDataService
{
    private readonly string _connectionString;
    private readonly ILogger<SqlDbService> _logger;

    public SqlDbService(IConfiguration configuration, ILogger<SqlDbService> logger)
    {
        _logger = logger;
        _connectionString = configuration.GetConnectionString("SqlDb")
            ?? throw new ArgumentException("SqlDbService.Init: ConnectionStrings:SqlDb must be provided in configuration.");
        _logger.LogInformation("SqlDbService.Init: Complete!");
    }

    // -------------------------------------------------------------------------
    // Users
    // -------------------------------------------------------------------------

    public async Task<GameUser?> GetUserByUsernameAsync(string username)
    {
        try
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(
                "SELECT Id, Username, GamesPlayed, TotalScore, BestScore, CreatedAt, LastPlayedAt " +
                "FROM [mathstorm].[GameUser] WHERE Username = @username", conn);
            cmd.Parameters.AddWithValue("@username", username);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapGameUser(reader);
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQL: Error getting user by username: {Username}", username);
            throw;
        }
    }

    public async Task<GameUser> CreateUserAsync(string username)
    {
        try
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

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(
                "INSERT INTO [mathstorm].[GameUser] (Id, Username, GamesPlayed, TotalScore, BestScore, CreatedAt, LastPlayedAt) " +
                "VALUES (@id, @username, @gamesPlayed, @totalScore, @bestScore, @createdAt, @lastPlayedAt)", conn);
            cmd.Parameters.AddWithValue("@id", user.Id);
            cmd.Parameters.AddWithValue("@username", user.Username);
            cmd.Parameters.AddWithValue("@gamesPlayed", user.GamesPlayed);
            cmd.Parameters.AddWithValue("@totalScore", user.TotalScore);
            cmd.Parameters.AddWithValue("@bestScore", user.BestScore);
            cmd.Parameters.AddWithValue("@createdAt", user.CreatedAt);
            cmd.Parameters.AddWithValue("@lastPlayedAt", user.LastPlayedAt);
            await cmd.ExecuteNonQueryAsync();
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQL: Error creating user: {Username}", username);
            throw;
        }
    }

    public async Task<GameUser> UpdateUserAsync(GameUser user)
    {
        try
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(
                "UPDATE [mathstorm].[GameUser] SET GamesPlayed=@gamesPlayed, TotalScore=@totalScore, " +
                "BestScore=@bestScore, LastPlayedAt=@lastPlayedAt WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", user.Id);
            cmd.Parameters.AddWithValue("@gamesPlayed", user.GamesPlayed);
            cmd.Parameters.AddWithValue("@totalScore", user.TotalScore);
            cmd.Parameters.AddWithValue("@bestScore", user.BestScore);
            cmd.Parameters.AddWithValue("@lastPlayedAt", user.LastPlayedAt);
            await cmd.ExecuteNonQueryAsync();
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQL: Error updating user: {UserId}", user.Id);
            throw;
        }
    }

    // -------------------------------------------------------------------------
    // Games
    // -------------------------------------------------------------------------

    public async Task<Game> CreateGameAsync(Game game)
    {
        try
        {
            var questionsJson = JsonSerializer.Serialize(game.Questions);
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(
                "INSERT INTO [mathstorm].[Game] (Id, UserId, Username, Difficulty, TotalScore, CompletedAt, Analysis, QuestionsJson) " +
                "VALUES (@id, @userId, @username, @difficulty, @totalScore, @completedAt, @analysis, @questionsJson)", conn);
            cmd.Parameters.AddWithValue("@id", game.Id);
            cmd.Parameters.AddWithValue("@userId", game.UserId);
            cmd.Parameters.AddWithValue("@username", game.Username);
            cmd.Parameters.AddWithValue("@difficulty", game.Difficulty);
            cmd.Parameters.AddWithValue("@totalScore", game.TotalScore);
            cmd.Parameters.AddWithValue("@completedAt", game.CompletedAt);
            cmd.Parameters.AddWithValue("@analysis", (object?)game.Analysis ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@questionsJson", questionsJson);
            await cmd.ExecuteNonQueryAsync();
            return game;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQL: Error creating game");
            throw;
        }
    }

    public async Task<Game?> GetGameAsync(string gameId)
    {
        try
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(
                "SELECT Id, UserId, Username, Difficulty, TotalScore, CompletedAt, Analysis, QuestionsJson " +
                "FROM [mathstorm].[Game] WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", gameId);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapGame(reader);
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQL: Error getting game: {GameId}", gameId);
            throw;
        }
    }

    public async Task<Game?> GetGameByIdAsync(string gameId)
    {
        return await GetGameAsync(gameId);
    }

    public async Task<bool> UpdateGameAnalysisAsync(string gameId, string analysis)
    {
        try
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(
                "UPDATE [mathstorm].[Game] SET Analysis=@analysis WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", gameId);
            cmd.Parameters.AddWithValue("@analysis", analysis);
            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQL: Error updating game analysis: {GameId}", gameId);
            return false;
        }
    }

    // -------------------------------------------------------------------------
    // Leaderboard
    // -------------------------------------------------------------------------

    public async Task<List<LeaderboardEntry>> GetLeaderboardAsync(string difficulty, int topCount = 10)
    {
        try
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(
                "SELECT TOP (@topCount) Id, Difficulty, Username, UserId, GameId, Score, AchievedAt, Rank " +
                "FROM [mathstorm].[LeaderboardEntry] " +
                "WHERE Difficulty = @difficulty " +
                "ORDER BY Score ASC", conn);
            cmd.Parameters.AddWithValue("@topCount", topCount);
            cmd.Parameters.AddWithValue("@difficulty", difficulty);
            var results = new List<LeaderboardEntry>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(MapLeaderboardEntry(reader));
            }
            // Update ranks
            for (int i = 0; i < results.Count; i++)
            {
                results[i].Rank = i + 1;
            }
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQL: Error getting leaderboard for difficulty: {Difficulty}", difficulty);
            throw;
        }
    }

    public async Task<List<LeaderboardEntry>> GetGlobalLeaderboardAsync(int topCount = 10)
    {
        try
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(
                "SELECT TOP (@topCount) Id, Difficulty, Username, UserId, GameId, Score, AchievedAt, Rank " +
                "FROM [mathstorm].[LeaderboardEntry] " +
                "ORDER BY Score ASC", conn);
            cmd.Parameters.AddWithValue("@topCount", topCount);
            var results = new List<LeaderboardEntry>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(MapLeaderboardEntry(reader));
            }
            // Update ranks
            for (int i = 0; i < results.Count; i++)
            {
                results[i].Rank = i + 1;
            }
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQL: Error getting global leaderboard");
            throw;
        }
    }

    public async Task<LeaderboardEntry?> AddToLeaderboardAsync(string userId, string username, string gameId, string difficulty, double score)
    {
        try
        {
            // Get all entries for this user in this difficulty (case-insensitive)
            var userEntries = await GetUserEntriesAsync(username, difficulty);

            // Check if user already has 3 or more entries
            if (userEntries.Count >= 3)
            {
                var worstUserScore = userEntries.Max(e => e.Score);
                if (score >= worstUserScore)
                {
                    return null;
                }

                // New score is better, remove the worst existing entry
                var worstUserEntry = userEntries.OrderByDescending(e => e.Score).First();
                await DeleteLeaderboardEntryAsync(worstUserEntry.Id);
            }

            // Check if this score qualifies for top 10
            var currentLeaderboard = await GetLeaderboardAsync(difficulty, 10);

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
                    AchievedAt = DateTime.UtcNow,
                    Rank = 1
                };

                await using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();
                await using var cmd = new SqlCommand(
                    "INSERT INTO [mathstorm].[LeaderboardEntry] (Id, Difficulty, Username, UserId, GameId, Score, AchievedAt, Rank) " +
                    "VALUES (@id, @difficulty, @username, @userId, @gameId, @score, @achievedAt, @rank)", conn);
                cmd.Parameters.AddWithValue("@id", entry.Id);
                cmd.Parameters.AddWithValue("@difficulty", entry.Difficulty);
                cmd.Parameters.AddWithValue("@username", entry.Username);
                cmd.Parameters.AddWithValue("@userId", entry.UserId);
                cmd.Parameters.AddWithValue("@gameId", entry.GameId);
                cmd.Parameters.AddWithValue("@score", entry.Score);
                cmd.Parameters.AddWithValue("@achievedAt", entry.AchievedAt);
                cmd.Parameters.AddWithValue("@rank", entry.Rank);
                await cmd.ExecuteNonQueryAsync();

                // If we now have more than 10 entries, remove the worst one
                if (currentLeaderboard.Count >= 10)
                {
                    var updatedLeaderboard = await GetLeaderboardAsync(difficulty, 11);
                    if (updatedLeaderboard.Count > 10)
                    {
                        var worstEntry = updatedLeaderboard.Last();
                        await DeleteLeaderboardEntryAsync(worstEntry.Id);
                    }
                }

                await UpdateLeaderboardRankingsAsync(difficulty);
                return entry;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQL: Error adding to leaderboard");
            throw;
        }
    }

    public async Task UpdateLeaderboardRankingsAsync(string difficulty)
    {
        try
        {
            var leaderboard = await GetLeaderboardAsync(difficulty, 10);
            if (leaderboard.Count == 0) return;

            // Build a single batched UPDATE using a CASE expression to avoid N round-trips
            var setClauses = new System.Text.StringBuilder();
            var idList = new System.Text.StringBuilder();
            setClauses.Append("UPDATE [mathstorm].[LeaderboardEntry] SET Rank = CASE Id");
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand();
            cmd.Connection = conn;

            for (int i = 0; i < leaderboard.Count; i++)
            {
                var entry = leaderboard[i];
                var newRank = i + 1;
                var idParam = $"@id{i}";
                var rankParam = $"@rank{i}";
                setClauses.Append($" WHEN {idParam} THEN {rankParam}");
                if (i > 0) idList.Append(',');
                idList.Append(idParam);
                cmd.Parameters.AddWithValue(idParam, entry.Id);
                cmd.Parameters.AddWithValue(rankParam, newRank);
            }
            setClauses.Append($" END WHERE Id IN ({idList})");
            cmd.CommandText = setClauses.ToString();
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQL: Error updating leaderboard rankings for difficulty: {Difficulty}", difficulty);
            throw;
        }
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private async Task<List<LeaderboardEntry>> GetUserEntriesAsync(string username, string difficulty)
    {
        try
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(
                "SELECT Id, Difficulty, Username, UserId, GameId, Score, AchievedAt, Rank " +
                "FROM [mathstorm].[LeaderboardEntry] " +
                "WHERE Username = @username AND Difficulty = @difficulty " +
                "ORDER BY Score ASC", conn);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@difficulty", difficulty);
            var results = new List<LeaderboardEntry>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(MapLeaderboardEntry(reader));
            }
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQL: Error getting user entries for username: {Username}, difficulty: {Difficulty}", username, difficulty);
            throw;
        }
    }

    private async Task DeleteLeaderboardEntryAsync(string id)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("DELETE FROM [mathstorm].[LeaderboardEntry] WHERE Id=@id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    private static GameUser MapGameUser(SqlDataReader reader) => new()
    {
        Id = reader.GetString(0),
        Username = reader.GetString(1),
        GamesPlayed = reader.GetInt32(2),
        TotalScore = reader.GetDouble(3),
        BestScore = reader.GetDouble(4),
        CreatedAt = reader.GetDateTime(5),
        LastPlayedAt = reader.GetDateTime(6)
    };

    private static Game MapGame(SqlDataReader reader)
    {
        var questionsJson = reader.IsDBNull(7) ? "[]" : reader.GetString(7);
        var questions = JsonSerializer.Deserialize<List<GameQuestion>>(questionsJson) ?? [];
        return new Game
        {
            Id = reader.GetString(0),
            UserId = reader.GetString(1),
            Username = reader.GetString(2),
            Difficulty = reader.GetString(3),
            TotalScore = reader.GetDouble(4),
            CompletedAt = reader.GetDateTime(5),
            Analysis = reader.IsDBNull(6) ? null : reader.GetString(6),
            Questions = questions
        };
    }

    private static LeaderboardEntry MapLeaderboardEntry(SqlDataReader reader) => new()
    {
        Id = reader.GetString(0),
        Difficulty = reader.GetString(1),
        Username = reader.GetString(2),
        UserId = reader.GetString(3),
        GameId = reader.GetString(4),
        Score = reader.GetDouble(5),
        AchievedAt = reader.GetDateTime(6),
        Rank = reader.GetInt32(7)
    };
}
