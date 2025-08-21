namespace MathStorm.Functions.Functions;

public class ResultsAnalysisFunctions
{
    private readonly ILogger<ResultsAnalysisFunctions> _logger;
    private readonly IResultsAnalysisService _analysisService;

    public ResultsAnalysisFunctions(ILogger<ResultsAnalysisFunctions> logger, IResultsAnalysisService analysisService)
    {
        _logger = logger;
        _analysisService = analysisService;
    }

    [Function("AnalyzeGameResults")]
    [OpenApiOperation(operationId: "AnalyzeGameResults", tags: new[] { "Game" }, Summary = "Analyze game results with AI commentary", Description = "Analyzes completed game results and provides personalized commentary using various AI personalities.")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(ResultsAnalysisRequestDto), Required = true, Description = "Game results data for analysis")]
    [OpenApiParameter(name: "personality", In = ParameterLocation.Query, Required = false, Type = typeof(string), Summary = "AI personality style", Description = "The personality style for analysis (default, comedyroast, pirate, limerick, sportsbroadcaster, haiku, australian, yourmother). Defaults to 'default' if not specified.")]
    [OpenApiParameter(name: "model", In = ParameterLocation.Query, Required = false, Type = typeof(string), Summary = "AI model to use", Description = "The AI model to use for analysis (gpt-4o-mini, gpt-4o, gpt-4). Defaults to the configured default model if not specified.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ResultsAnalysisResponseDto), Summary = "Analysis completed successfully", Description = "Returns AI-generated analysis and commentary on the game performance.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/plain", bodyType: typeof(string), Summary = "Bad request", Description = "Invalid request body or missing required fields.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "text/plain", bodyType: typeof(string), Summary = "Internal server error", Description = "An error occurred while analyzing the game results.")]
    public async Task<HttpResponseData> AnalyzeGameResults([HttpTrigger(AuthorizationLevel.Function, "post", Route = "game/analysis")] HttpRequestData req)
    {
        _logger.LogInformation("AnalyzeGameResults function triggered.");

        try
        {
            // Parse request body
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonConvert.DeserializeObject<ResultsAnalysisRequestDto>(body);

            if (request == null)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Invalid request body");
                return badRequest;
            }

            // Get personality and model from query string if provided
            var queryCollection = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(req.Url.Query);
            if (queryCollection.ContainsKey("personality"))
            {
                request.Personality = queryCollection["personality"].FirstOrDefault() ?? "default";
            }

            if (queryCollection.ContainsKey("model"))
            {
                request.Model = queryCollection["model"].FirstOrDefault() ?? "gpt_4o_mini";
            }

            // Validate personality
            if (!IsValidPersonality(request.Personality))
            {
                request.Personality = "default";
            }

            // Validate model (basic validation - actual validation happens in the service)
            if (!IsValidModel(request.Model))
            {
                request.Model = "gpt-4o-mini"; // fallback to default
            }

            _logger.LogInformation($"Analyzing game results for {request.Username} with {request.Personality} personality using model {request.Model}");

            // Generate analysis
            var analysis = await _analysisService.AnalyzeGameResultsAsync(request);

            // Prepare response
            var response = new ResultsAnalysisResponseDto
            {
                GameId = request.GameId,
                Personality = request.Personality,
                Model = request.Model,
                Analysis = analysis,
                GeneratedAt = DateTime.UtcNow
            };

            var httpResponse = req.CreateResponse(HttpStatusCode.OK);
            httpResponse.Headers.Add("Content-Type", "application/json");

            var jsonResponse = JsonConvert.SerializeObject(response);
            await httpResponse.WriteStringAsync(jsonResponse);
            return httpResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Func: Error in AnalyzeGameResults function");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }

    private static bool IsValidPersonality(string personality)
    {
        var validPersonalities = new[] { "default", "comedyroast", "pirate", "limerick", "sportsbroadcaster", "haiku", "australian", "yourmother" };
        return validPersonalities.Contains(personality.ToLowerInvariant());
    }

    private static bool IsValidModel(string model)
    {
        var validModels = new[] { "gpt_4o_mini", "gpt_4o", "gpt_4", "gpt_5_mini", "gpt_35_turbo" };
        return validModels.Contains(model.ToLowerInvariant());
    }
}