namespace MathStorm.Functions.Functions;

public class LeaderboardFunctions
{
    private readonly ILogger<LeaderboardFunctions> _logger;
    private readonly ICosmosDbService _cosmosDbService;

    public LeaderboardFunctions(ILogger<LeaderboardFunctions> logger, ICosmosDbService cosmosDbService)
    {
        _logger = logger;
        _cosmosDbService = cosmosDbService;
    }

    [Function("HelloLeaders")]
    public IActionResult Hello([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function LeaderboardFunctions.Hello");
        return new OkObjectResult("Welcome to LeaderboardFunctions.Hello!");
    }

    [Function("GetLeaderboard")]
    [OpenApiOperation(operationId: "GetLeaderboard", tags: new[] { "Leaderboard" }, Summary = "Get leaderboard entries", Description = "Retrieves leaderboard entries for a specific difficulty level or global leaderboard.")]
    [OpenApiParameter(name: "difficulty", In = ParameterLocation.Query, Required = false, Type = typeof(string), Summary = "Difficulty level", Description = "The difficulty level to filter leaderboard entries (Beginner, Novice, Intermediate, Expert). If not specified, returns global leaderboard.")]
    [OpenApiParameter(name: "topCount", In = ParameterLocation.Query, Required = false, Type = typeof(int), Summary = "Number of entries", Description = "Number of top entries to retrieve. Defaults to 10 if not specified.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(LeaderboardResponseDto), Summary = "Leaderboard retrieved successfully", Description = "Returns the requested leaderboard entries.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "text/plain", bodyType: typeof(string), Summary = "Internal server error", Description = "An error occurred while retrieving the leaderboard.")]
    public async Task<HttpResponseData> GetLeaderboard([HttpTrigger(AuthorizationLevel.Function, "get", Route = "leaderboard")] HttpRequestData req)
    {
        _logger.LogInformation("GetLeaderboard function triggered.");

        try
        {
            // Parse query parameters more safely using built-in query parsing
            string? difficulty = null;
            var topCount = 10;

            _logger.LogInformation($"GetLeaderboard called with query: {req.Url.Query}");

            // Use Microsoft.AspNetCore.WebUtilities.QueryHelpers for safer parsing
            var queryCollection = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(req.Url.Query);

            if (queryCollection.ContainsKey("difficulty"))
            {
                difficulty = queryCollection["difficulty"].FirstOrDefault();
            }

            if (queryCollection.ContainsKey("topCount"))
            {
                if (!int.TryParse(queryCollection["topCount"].FirstOrDefault(), out topCount))
                {
                    topCount = 10;
                }
            }

            _logger.LogInformation($"Parsed parameters - difficulty: '{difficulty}', topCount: {topCount}");

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

            var jsonResponse = JsonConvert.SerializeObject(response);

            await httpResponse.WriteStringAsync(jsonResponse);
            return httpResponse;
        }
        catch (Exception ex)
        {
            var msg = ExceptionHelper.GetExceptionMessage(ex);
            _logger.LogError(ex, $"Unexpected Error in GetLeaderboard: {ex.Message}", msg);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Unexpected Error in GetLeaderboard: {msg}");
            return errorResponse;
        }
    }
}