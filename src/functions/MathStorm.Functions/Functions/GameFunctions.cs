namespace MathStorm.Functions.Functions;

public class GameFunctions
{
    private readonly ILogger<GameFunctions> _logger;
    private readonly IGameService _gameService;
    private readonly ICosmosDbService _cosmosDbService;

    public GameFunctions(ILogger<GameFunctions> logger, IGameService gameService, ICosmosDbService cosmosDbService)
    {
        _logger = logger;
        _gameService = gameService;
        _cosmosDbService = cosmosDbService;
    }

    [Function("HelloGame")]
    public IActionResult Hello([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function GameFunctions.Hello");
        return new OkObjectResult("Welcome to GameFunctions.Hello!");
    }

    [Function("GetGame")]
    [OpenApiOperation(operationId: "GetGame", tags: new[] { "Game" }, Summary = "Generate a new math game", Description = "Creates a new game session with questions based on the specified difficulty level.")]
    [OpenApiParameter(name: "difficulty", Required = false, Type = typeof(string), Summary = "Difficulty level", Description = "The difficulty level for the game (Beginner, Novice, Intermediate, Expert). Defaults to Expert if not specified.")]
    // In = ParameterLocation.Query,
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(GameResponseDto), Summary = "Game created successfully", Description = "Returns a new game session with questions for the specified difficulty level.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "text/plain", bodyType: typeof(string), Summary = "Internal server error", Description = "An error occurred while creating the game.")]
    public async Task<HttpResponseData> GetGame([HttpTrigger(AuthorizationLevel.Function, "get", Route = "game")] HttpRequestData req)
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

            var jsonResponse = JsonConvert.SerializeObject(response);

            await httpResponse.WriteStringAsync(jsonResponse);
            return httpResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Func: Error in GetGame function");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }

    [Function("GetGameById")]
    [OpenApiOperation(operationId: "GetGameById", tags: new[] { "Game" }, Summary = "Get game by ID", Description = "Retrieves a specific game record by its ID, including questions, answers, and analysis.")]
    [OpenApiParameter(name: "gameId", Required = true, Type = typeof(string), Summary = "Game ID", Description = "The unique identifier for the game.")] // In = ParameterLocation.Path,
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Game), Summary = "Game retrieved successfully", Description = "Returns the complete game record including questions, answers, scores, and analysis.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "text/plain", bodyType: typeof(string), Summary = "Game not found", Description = "The specified game ID was not found.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "text/plain", bodyType: typeof(string), Summary = "Internal server error", Description = "An error occurred while retrieving the game.")]
    public async Task<HttpResponseData> GetGameById([HttpTrigger(AuthorizationLevel.Function, "get", Route = "game/{gameId}")] HttpRequestData req, string gameId)
    {
        _logger.LogInformation($"GetGameById function triggered for gameId: {gameId}");

        try
        {
            if (string.IsNullOrEmpty(gameId))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Game ID is required");
                return badRequest;
            }

            var game = await _cosmosDbService.GetGameByIdAsync(gameId);
            if (game == null)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync($"Game with ID {gameId} not found");
                return notFound;
            }

            var httpResponse = req.CreateResponse(HttpStatusCode.OK);
            httpResponse.Headers.Add("Content-Type", "application/json");

            var jsonResponse = JsonConvert.SerializeObject(game);

            await httpResponse.WriteStringAsync(jsonResponse);
            return httpResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Func: Error in GetGameById function for gameId: {gameId}");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }

    [Function("UpdateGameAnalysis")]
    [OpenApiOperation(operationId: "UpdateGameAnalysis", tags: new[] { "Game" }, Summary = "Update game analysis", Description = "Updates the analysis field for a specific game record.")]
    [OpenApiParameter(name: "gameId", Required = true, Type = typeof(string), Summary = "Game ID", Description = "The unique identifier for the game.")] // In = ParameterLocation.Path,
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(object), Required = true, Description = "Analysis data containing the analysis text")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(object), Summary = "Analysis updated successfully", Description = "Returns success confirmation.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "text/plain", bodyType: typeof(string), Summary = "Game not found", Description = "The specified game ID was not found.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "text/plain", bodyType: typeof(string), Summary = "Internal server error", Description = "An error occurred while updating the analysis.")]
    public async Task<HttpResponseData> UpdateGameAnalysis([HttpTrigger(AuthorizationLevel.Function, "put", Route = "game/{gameId}/analysis")] HttpRequestData req, string gameId)
    {
        _logger.LogInformation($"UpdateGameAnalysis function triggered for gameId: {gameId}");

        try
        {
            if (string.IsNullOrEmpty(gameId))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Game ID is required");
                return badRequest;
            }

            // Parse request body
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonConvert.DeserializeObject<dynamic>(body);

            if (request?.Analysis == null)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Analysis is required in the request body");
                return badRequest;
            }

            string analysis = request.Analysis.ToString();

            var success = await _cosmosDbService.UpdateGameAnalysisAsync(gameId, analysis);
            if (!success)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync($"Game with ID {gameId} not found or could not be updated");
                return notFound;
            }

            var httpResponse = req.CreateResponse(HttpStatusCode.OK);
            httpResponse.Headers.Add("Content-Type", "application/json");

            var jsonResponse = JsonConvert.SerializeObject(new { success = true, message = "Analysis updated successfully" });

            await httpResponse.WriteStringAsync(jsonResponse);
            return httpResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Func: Error in UpdateGameAnalysis function for gameId: {gameId}");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }
}