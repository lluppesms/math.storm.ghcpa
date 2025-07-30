using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Text.Json;
using System.Web;
using MathStorm.Core.Services;
using MathStorm.Common.Models;
using MathStorm.Common.DTOs;
using MathStorm.Common.Services;

namespace MathStorm.Functions.Functions;

public class GameFunctions
{
    private readonly ILogger _logger;
    private readonly IGameService _gameService;

    public GameFunctions(ILoggerFactory loggerFactory, IGameService gameService)
    {
        _logger = loggerFactory.CreateLogger<GameFunctions>();
        _gameService = gameService;
    }

    [Function("GetGame")]
    [OpenApiOperation(operationId: "GetGame", tags: new[] { "Game" }, Summary = "Generate a new math game", Description = "Creates a new game session with questions based on the specified difficulty level.")]
    [OpenApiParameter(name: "difficulty", In = ParameterLocation.Query, Required = false, Type = typeof(string), Summary = "Difficulty level", Description = "The difficulty level for the game (Beginner, Novice, Intermediate, Expert). Defaults to Expert if not specified.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(GameResponseDto), Summary = "Game created successfully", Description = "Returns a new game session with questions for the specified difficulty level.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "text/plain", bodyType: typeof(string), Summary = "Internal server error", Description = "An error occurred while creating the game.")]
    public async Task<HttpResponseData> GetGame([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "game")] HttpRequestData req)
    {
        _logger.LogInformation("GetGame function triggered.");

        try
        {
            // Parse query parameters more safely
            var query = req.Url.Query;
            var difficultyParam = "Expert";

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

                    queryDict.TryGetValue("difficulty", out difficultyParam);
                    difficultyParam ??= "Expert";
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error parsing query parameters: {Query}", query);
                    // Continue with default values
                }
            }

            if (!Enum.TryParse<Difficulty>(difficultyParam, ignoreCase: true, out var difficulty))
            {
                difficulty = Difficulty.Expert;
            }

            // Create new game session
            var gameSession = _gameService.CreateNewGame(difficulty);

            // Convert to DTO
            var response = new GameResponseDto
            {
                GameId = Guid.NewGuid().ToString(), // Generate unique game ID for this session
                Difficulty = difficulty.ToString(),
                Questions = gameSession.Questions.Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Number1 = q.Number1,
                    Number2 = q.Number2,
                    Operation = q.Operation.ToString(),
                    CorrectAnswer = q.CorrectAnswer,
                    QuestionText = q.QuestionText
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
            _logger.LogError(ex, "Error in GetGame function");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }
}