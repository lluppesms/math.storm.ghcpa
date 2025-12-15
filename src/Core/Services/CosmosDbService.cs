namespace MathStorm.Core.Services;

public class CosmosDbService : ICosmosDbService
{
    private readonly Container _usersContainer;
    private readonly Container _gamesContainer;
    private readonly Container _leaderboardContainer;
    private readonly ILogger<CosmosDbService> _logger;

    public CosmosDbService(CosmosClient cosmosClient, IConfiguration configuration, ILogger<CosmosDbService> logger)
    {
        _logger = logger;

        if (_gamesContainer == null)
        {
            _logger.Log(LogLevel.Information, "CosmosDbService.Init: Starting");
            var endpoint = configuration["CosmosDb:Endpoint"];
            var connectionString = configuration["CosmosDb:ConnectionString"];
            if (string.IsNullOrEmpty(endpoint) && string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("CosmosDbService.Init: Either Endpoint or ConnectionString must be provided in configuration.");
            }
            // problems with Managed Identity auth right now, so wipe this if there is a connection string
            if (!string.IsNullOrEmpty(connectionString))
            {
                endpoint = string.Empty;
            }

            // Extract account name for logging purposes
            var accountName = endpoint;
            _logger.Log(LogLevel.Information, $"CosmosDbService.Init: AccountName-EP: {accountName}");
            if (!string.IsNullOrEmpty(connectionString))
            {
                var accountKeyLocation = connectionString.IndexOf("AccountKey");
                _logger.Log(LogLevel.Information, $"CosmosDbService.Init: AccountName-CS-AKL: {accountKeyLocation}");
                accountName = accountKeyLocation > 0 ? connectionString?[..accountKeyLocation] : "UNKNOWN";
                _logger.Log(LogLevel.Information, $"CosmosDbService.Init: AccountName-CS-RAW: {accountName}");
                accountName = accountName?.Replace("https://", "").Replace(".documents.azure.com:443/", "").Replace("/;", "").Replace(";", "");
                _logger.Log(LogLevel.Information, $"CosmosDbService.Init: AccountName-CS: {accountName}");
            }

            var databaseName = configuration["CosmosDb:DatabaseName"];
            var usersContainer = configuration["CosmosDb:ContainerNames:Users"];
            var gamesContainer = configuration["CosmosDb:ContainerNames:Games"];
            var leaderboardContainer = configuration["CosmosDb:ContainerNames:Leaderboard"];

            if (!string.IsNullOrEmpty(connectionString)) { _logger.Log(LogLevel.Information, $"CosmosDbService.Init: Using Account: {accountName} Database: {databaseName}"); }
            if (!string.IsNullOrEmpty(endpoint)) { _logger.Log(LogLevel.Information, $"CosmosDbService.Init: Using Endpoint: {accountName} Database: {databaseName}"); }

            cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName).GetAwaiter().GetResult();
            var database = cosmosClient.GetDatabase(databaseName);

            database.CreateContainerIfNotExistsAsync(usersContainer, "/id").GetAwaiter().GetResult();
            _usersContainer = database.GetContainer(usersContainer);

            database.CreateContainerIfNotExistsAsync(gamesContainer, "/id").GetAwaiter().GetResult();
            _gamesContainer = database.GetContainer(gamesContainer);

            database.CreateContainerIfNotExistsAsync(leaderboardContainer, "/id").GetAwaiter().GetResult();
            _leaderboardContainer = database.GetContainer(leaderboardContainer);

            _logger.Log(LogLevel.Information, "CosmosDbService.Init: Complete!");
        }
        else
        {
            _logger.Log(LogLevel.Information, "CosmosDbService.Init: Using existing connection...");
        }
    }

    public async Task<GameUser?> GetUserByUsernameAsync(string username)
    {
        try
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.username = @username")
                .WithParameter("@username", username);

            var iterator = _usersContainer.GetItemQueryIterator<GameUser>(query);
            var response = await iterator.ReadNextAsync();

            return response.FirstOrDefault();
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cosmos: Error getting user by username: {Username}", username);
            throw;
        }
    }

    public async Task<GameUser> CreateUserAsync(string username)
    {
        try
        {
            var user = new GameUser
            {
                Username = username,
                CreatedAt = DateTime.UtcNow
            };

            var response = await _usersContainer.CreateItemAsync(user);
            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cosmos: Error creating user: {Username}", username);
            throw;
        }
    }

    public async Task<GameUser> UpdateUserAsync(GameUser user)
    {
        try
        {
            var response = await _usersContainer.ReplaceItemAsync(user, user.Id);
            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cosmos: Error updating user: {UserId}", user.Id);
            throw;
        }
    }

    public async Task<Game> CreateGameAsync(Game game)
    {
        try
        {
            var response = await _gamesContainer.CreateItemAsync(game);
            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cosmos: Error creating game");
            throw;
        }
    }

    public async Task<Game?> GetGameAsync(string gameId)
    {
        try
        {
            var response = await _gamesContainer.ReadItemAsync<Game>(gameId, new PartitionKey(gameId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cosmos: Error getting game: {GameId}", gameId);
            throw;
        }
    }

    public async Task<Game?> GetGameByIdAsync(string gameId)
    {
        // This is an alias to GetGameAsync for consistency with the web service interface
        return await GetGameAsync(gameId);
    }

    public async Task<bool> UpdateGameAnalysisAsync(string gameId, string analysis)
    {
        try
        {
            var game = await GetGameAsync(gameId);
            if (game == null)
            {
                return false;
            }

            game.Analysis = analysis;
            var response = await _gamesContainer.ReplaceItemAsync(game, game.Id);
            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cosmos: Error updating game analysis: {GameId}", gameId);
            return false;
        }
    }

    public async Task<List<LeaderboardEntry>> GetLeaderboardAsync(string difficulty, int topCount = 10)
    {
        try
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.difficulty = @difficulty ORDER BY c.score ASC OFFSET 0 LIMIT @topCount")
                .WithParameter("@difficulty", difficulty)
                .WithParameter("@topCount", topCount);

            var iterator = _leaderboardContainer.GetItemQueryIterator<LeaderboardEntry>(query);
            var results = new List<LeaderboardEntry>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
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
            _logger.LogError(ex, "Cosmos: Error getting leaderboard for difficulty: {Difficulty}", difficulty);
            throw;
        }
    }

    public async Task<List<LeaderboardEntry>> GetGlobalLeaderboardAsync(int topCount = 10)
    {
        try
        {
            var query = new QueryDefinition("SELECT * FROM c ORDER BY c.score ASC OFFSET 0 LIMIT @topCount")
                .WithParameter("@topCount", topCount);

            var iterator = _leaderboardContainer.GetItemQueryIterator<LeaderboardEntry>(query);
            var results = new List<LeaderboardEntry>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
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
            _logger.LogError(ex, "Cosmos: Error getting global leaderboard");
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
                // Check if new score is better (lower) than any existing score
                var worstUserScore = userEntries.Max(e => e.Score);
                if (score >= worstUserScore)
                {
                    // New score is not better than any existing entry, don't add it
                    return null;
                }
                
                // New score is better, remove the worst existing entry
                var worstUserEntry = userEntries.OrderByDescending(e => e.Score).First();
                await _leaderboardContainer.DeleteItemAsync<LeaderboardEntry>(worstUserEntry.Id, new PartitionKey(worstUserEntry.Id));
            }
            
            // Check if this score qualifies for top 10
            var currentLeaderboard = await GetLeaderboardAsync(difficulty, 10);

            // If leaderboard has less than 10 entries or this score is better than the worst score
            if (currentLeaderboard.Count < 10 || score < currentLeaderboard.Last().Score)
            {
                var entry = new LeaderboardEntry
                {
                    UserId = userId,
                    Username = username,
                    GameId = gameId,
                    Difficulty = difficulty,
                    Score = score,
                    AchievedAt = DateTime.UtcNow
                };

                var response = await _leaderboardContainer.CreateItemAsync(entry);

                // If we now have more than 10 entries, remove the worst one
                if (currentLeaderboard.Count >= 10)
                {
                    var updatedLeaderboard = await GetLeaderboardAsync(difficulty, 11);
                    if (updatedLeaderboard.Count > 10)
                    {
                        var worstEntry = updatedLeaderboard.Last();
                        await _leaderboardContainer.DeleteItemAsync<LeaderboardEntry>(worstEntry.Id, new PartitionKey(worstEntry.Id));
                    }
                }

                await UpdateLeaderboardRankingsAsync(difficulty);
                return response.Resource;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cosmos: Error adding to leaderboard");
            throw;
        }
    }

    private async Task<List<LeaderboardEntry>> GetUserEntriesAsync(string username, string difficulty)
    {
        try
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE LOWER(c.username) = LOWER(@username) AND c.difficulty = @difficulty ORDER BY c.score ASC")
                .WithParameter("@username", username)
                .WithParameter("@difficulty", difficulty);

            var iterator = _leaderboardContainer.GetItemQueryIterator<LeaderboardEntry>(query);
            var results = new List<LeaderboardEntry>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cosmos: Error getting user entries for username: {Username}, difficulty: {Difficulty}", username, difficulty);
            throw;
        }
    }

    public async Task UpdateLeaderboardRankingsAsync(string difficulty)
    {
        try
        {
            var leaderboard = await GetLeaderboardAsync(difficulty, 10);

            for (int i = 0; i < leaderboard.Count; i++)
            {
                if (leaderboard[i].Rank != i + 1)
                {
                    leaderboard[i].Rank = i + 1;
                    await _leaderboardContainer.ReplaceItemAsync(leaderboard[i], leaderboard[i].Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cosmos: Error updating leaderboard rankings for difficulty: {Difficulty}", difficulty);
            throw;
        }
    }
}