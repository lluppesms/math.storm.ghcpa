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
    [OpenApiOperation(operationId: "SubmitGameResults", tags: new[] { "Game" }, Summary = "Submit game results", Description = "Processes completed game results, saves scores, and updates the leaderboard.")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(GameResultsRequestDto), Required = true, Description = "Game results including player answers and scores")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(GameResultsResponseDto), Summary = "Results submitted successfully", Description = "Returns the processed game results including total score and leaderboard information.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/plain", bodyType: typeof(string), Summary = "Bad request", Description = "Invalid request body or missing required fields.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "text/plain", bodyType: typeof(string), Summary = "Internal server error", Description = "An error occurred while processing the game results.")]
    public async Task<HttpResponseData> SubmitGameResults([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "game/results")] HttpRequestData req)
    {
        _logger.LogInformation("SubmitGameResults function triggered.");

        try
        {
            // Parse request body
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonConvert.DeserializeObject<GameResultsRequestDto>(body);

            if (request == null)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Invalid request body");
                return badRequest;
            }

            // Get user by UserId (since authentication is now done before game submission)
            var user = await _cosmosDbService.GetUserByUsernameAsync(request.Username);
            if (user == null)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("User not found. Please authenticate first.");
                return badRequest;
            }

            // Create game record
            var game = new Game
            {
                Id = request.GameId,
                UserId = request.UserId,
                Username = request.Username,
                Difficulty = request.Difficulty,
                TotalScore = request.Questions.Sum(q => q.Score),
                CompletedAt = DateTime.UtcNow,
                Analysis = request.Analysis,
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

            var jsonResponse = JsonConvert.SerializeObject(response);

            await httpResponse.WriteStringAsync(jsonResponse);
            return httpResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Func: Error in SubmitGameResults function");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }
}