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
            // Parse query parameters more safely using built-in query parsing
            var difficultyParam = "Expert";
            
            _logger.LogInformation($"GetGame called with query: {req.Url.Query}");

            // Use Microsoft.AspNetCore.WebUtilities.QueryHelpers for safer parsing
            var queryCollection = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(req.Url.Query);
            
            if (queryCollection.ContainsKey("difficulty"))
            {
                difficultyParam = queryCollection["difficulty"].FirstOrDefault() ?? "Expert";
            }

            if (!Enum.TryParse<Difficulty>(difficultyParam, ignoreCase: true, out var difficulty))
            {
                difficulty = Difficulty.Expert;
            }

            _logger.LogInformation($"GetGame using difficulty: {difficulty}");
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