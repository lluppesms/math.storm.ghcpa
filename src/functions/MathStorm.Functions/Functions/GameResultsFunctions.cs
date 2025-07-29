using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using MathStorm.Shared.Services;
using MathStorm.Shared.Models;
using MathStorm.Shared.DTOs;

namespace MathStorm.Functions.Functions;

public class GameResultsFunctions
{
    private readonly ILogger _logger;
    private readonly ICosmosDbService _cosmosDbService;

    public GameResultsFunctions(ILoggerFactory loggerFactory, ICosmosDbService cosmosDbService)
    {
        _logger = loggerFactory.CreateLogger<GameResultsFunctions>();
        _cosmosDbService = cosmosDbService;
    }

    [Function("SubmitGameResults")]
    public async Task<HttpResponseData> SubmitGameResults([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "game/results")] HttpRequestData req)
    {
        _logger.LogInformation("SubmitGameResults function triggered.");

        try
        {
            // Parse request body
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonSerializer.Deserialize<GameResultsRequestDto>(body, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (request == null)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Invalid request body");
                return badRequest;
            }

            // Get or create user
            var user = await _cosmosDbService.GetUserByUsernameAsync(request.Username);
            if (user == null)
            {
                user = await _cosmosDbService.CreateUserAsync(request.Username);
            }

            // Create game record
            var game = new Game
            {
                Id = request.GameId,
                UserId = user.Id,
                Username = request.Username,
                Difficulty = request.Difficulty,
                TotalScore = request.Questions.Sum(q => q.Score),
                CompletedAt = DateTime.UtcNow,
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

            // Add to leaderboard
            var leaderboardEntry = await _cosmosDbService.AddToLeaderboardAsync(
                user.Id, request.Username, request.GameId, request.Difficulty, game.TotalScore);

            // Update rankings
            await _cosmosDbService.UpdateLeaderboardRankingsAsync(request.Difficulty);

            // Prepare response
            var response = new GameResultsResponseDto
            {
                GameId = request.GameId,
                TotalScore = game.TotalScore,
                AddedToLeaderboard = leaderboardEntry != null,
                LeaderboardRank = leaderboardEntry?.Rank
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
            _logger.LogError(ex, "Error in SubmitGameResults function");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }
}