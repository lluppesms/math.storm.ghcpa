using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using MathStorm.Shared.Services;
using MathStorm.Shared.DTOs;

namespace MathStorm.Functions.Functions;

public class LeaderboardFunctions
{
    private readonly ILogger _logger;
    private readonly ICosmosDbService _cosmosDbService;

    public LeaderboardFunctions(ILoggerFactory loggerFactory, ICosmosDbService cosmosDbService)
    {
        _logger = loggerFactory.CreateLogger<LeaderboardFunctions>();
        _cosmosDbService = cosmosDbService;
    }

    [Function("GetLeaderboard")]
    public async Task<HttpResponseData> GetLeaderboard([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "leaderboard")] HttpRequestData req)
    {
        _logger.LogInformation("GetLeaderboard function triggered.");

        try
        {
            // Parse query parameters
            var query = req.Url.Query;
            string? difficulty = null;
            var topCountParam = "10";
            
            if (!string.IsNullOrEmpty(query))
            {
                var queryDict = query.TrimStart('?')
                    .Split('&')
                    .Select(q => q.Split('='))
                    .Where(kvp => kvp.Length == 2)
                    .ToDictionary(kvp => kvp[0], kvp => Uri.UnescapeDataString(kvp[1]));
                
                queryDict.TryGetValue("difficulty", out difficulty);
                queryDict.TryGetValue("topCount", out topCountParam);
                topCountParam ??= "10";
            }

            if (!int.TryParse(topCountParam, out var topCount))
            {
                topCount = 10;
            }

            // Get leaderboard entries
            var entries = string.IsNullOrEmpty(difficulty) 
                ? await _cosmosDbService.GetGlobalLeaderboardAsync(topCount)
                : await _cosmosDbService.GetLeaderboardAsync(difficulty, topCount);

            // Convert to DTOs
            var response = new LeaderboardResponseDto
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

            var httpResponse = req.CreateResponse(HttpStatusCode.OK);
            httpResponse.Headers.Add("Content-Type", "application/json");
            
            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            await httpResponse.WriteStringAsync(jsonResponse);
            return httpResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetLeaderboard function");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }
}