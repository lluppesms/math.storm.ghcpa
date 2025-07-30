using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Text.Json;
using System.Web;
using MathStorm.Core.Services;
using MathStorm.Common.DTOs;

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
    [OpenApiOperation(operationId: "GetLeaderboard", tags: new[] { "Leaderboard" }, Summary = "Get leaderboard entries", Description = "Retrieves leaderboard entries for a specific difficulty level or global leaderboard.")]
    [OpenApiParameter(name: "difficulty", In = ParameterLocation.Query, Required = false, Type = typeof(string), Summary = "Difficulty level", Description = "The difficulty level to filter leaderboard entries (Beginner, Novice, Intermediate, Expert). If not specified, returns global leaderboard.")]
    [OpenApiParameter(name: "topCount", In = ParameterLocation.Query, Required = false, Type = typeof(int), Summary = "Number of entries", Description = "Number of top entries to retrieve. Defaults to 10 if not specified.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(LeaderboardResponseDto), Summary = "Leaderboard retrieved successfully", Description = "Returns the requested leaderboard entries.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "text/plain", bodyType: typeof(string), Summary = "Internal server error", Description = "An error occurred while retrieving the leaderboard.")]
    public async Task<HttpResponseData> GetLeaderboard([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "leaderboard")] HttpRequestData req)
    {
        _logger.LogInformation("GetLeaderboard function triggered.");

        try
        {
            // Parse query parameters more safely
            var query = req.Url.Query;
            string? difficulty = null;
            var topCountParam = "10";
            
            if (!string.IsNullOrEmpty(query))
            {
                try
                {
                    var queryDict = query.TrimStart('?')
                        .Split('&')
                        .Select(q => q.Split('=', 2))
                        .Where(kvp => kvp.Length == 2)
                        .ToDictionary(kvp => kvp[0], kvp => 
                        {
                            try
                            {
                                // Use HttpUtility.UrlDecode which is more robust than Uri.UnescapeDataString
                                return HttpUtility.UrlDecode(kvp[1]);
                            }
                            catch
                            {
                                // If URL decoding fails, return the original string
                                return kvp[1];
                            }
                        });
                    
                    queryDict.TryGetValue("difficulty", out difficulty);
                    queryDict.TryGetValue("topCount", out topCountParam);
                    topCountParam ??= "10";
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error parsing query parameters: {Query}", query);
                    // Continue with default values
                }
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
            _logger.LogError(ex, "Unexpected Error in GetLeaderboard: {ErrorMessage}", ex.Message);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Unexpected Error in GetLeaderboard: {ex.Message}");
            return errorResponse;
        }
    }
}