namespace MathStorm.Console.Services;

public interface IConsoleMathStormService
{
    Task<GameResponseDto?> GetGameAsync(Difficulty difficulty);
    Task<GameResultsResponseDto?> SubmitGameResultsAsync(GameResultsRequestDto request);
    Task<LeaderboardResponseDto?> GetLeaderboardAsync(string? difficulty = null, int topCount = 10);
    Task<ResultsAnalysisResponseDto?> AnalyzeGameResultsAsync(ResultsAnalysisRequestDto request);
    Task<UserAuthResponseDto?> AuthenticateUserAsync(UserAuthRequestDto request);
    GameSession CreateLocalGame(Difficulty difficulty);
}

public class ConsoleMathStormService : IConsoleMathStormService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ConsoleMathStormService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string? _apiKey;

    public ConsoleMathStormService(HttpClient httpClient, ILogger<ConsoleMathStormService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration.GetValue<string>("FunctionService:APIKey");
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    private HttpRequestMessage CreateRequestWithAuth(HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, url);
        if (!string.IsNullOrEmpty(_apiKey))
        {
            request.Headers.Add("x-functions-key", _apiKey);
        }
        return request;
    }

    public async Task<GameResponseDto?> GetGameAsync(Difficulty difficulty)
    {
        var apiUrl = $"/api/game?difficulty={difficulty}";
        try
        {
            var request = CreateRequestWithAuth(HttpMethod.Get, apiUrl);
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GameResponseDto>(content, _jsonOptions);
        }
        catch (Exception ex)
        {
            var msg = $"Error getting game from function API {apiUrl}";
            _logger.LogWarning(msg);
            _logger.LogError(ex, "Error getting game from function API");
            AnsiConsole.MarkupLine("[red]Error: Unable to connect to game service. Please try again later.[/]");
            return null;
        }
    }

    public async Task<GameResultsResponseDto?> SubmitGameResultsAsync(GameResultsRequestDto request)
    {
        var apiUrl = "/api/game/results";
        try
        {
            var httpRequest = CreateRequestWithAuth(HttpMethod.Post, apiUrl);
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GameResultsResponseDto>(responseContent, _jsonOptions);
        }
        catch (Exception ex)
        {
            var msg = $"Error submitting game results to function API {apiUrl}";
            _logger.LogWarning(msg);
            _logger.LogError(ex, "Error submitting game results to function API");
            AnsiConsole.MarkupLine("[red]Error: Unable to submit results. Your score may not be saved.[/]");
            return null;
        }
    }

    public async Task<LeaderboardResponseDto?> GetLeaderboardAsync(string? difficulty = null, int topCount = 10)
    {
        var apiUrl = $"/api/leaderboard?topCount={topCount}";
        try
        {
            if (!string.IsNullOrEmpty(difficulty))
            {
                apiUrl += $"&difficulty={difficulty}";
            }

            var request = CreateRequestWithAuth(HttpMethod.Get, apiUrl);
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<LeaderboardResponseDto>(content, _jsonOptions);
        }
        catch (Exception ex)
        {
            var msg = $"Error getting leaderboard from function API {apiUrl}";
            _logger.LogWarning(msg);
            _logger.LogError(ex, "Error getting leaderboard from function API");
            AnsiConsole.MarkupLine("[red]Error: Unable to load leaderboard. Please try again later.[/]");
            return null;
        }
    }

    public async Task<ResultsAnalysisResponseDto?> AnalyzeGameResultsAsync(ResultsAnalysisRequestDto request)
    {
        var apiUrl = "/api/game/analysis";
        try
        {
            var httpRequest = CreateRequestWithAuth(HttpMethod.Post, apiUrl);
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ResultsAnalysisResponseDto>(responseContent, _jsonOptions);
        }
        catch (Exception ex)
        {
            var msg = $"Error analyzing game results via function API {apiUrl}";
            _logger.LogWarning(msg);
            _logger.LogError(ex, "Error analyzing game results via function API");
            AnsiConsole.MarkupLine("[red]Error: Unable to analyze results. Please try again later.[/]");
            return null;
        }
    }

    public async Task<UserAuthResponseDto?> AuthenticateUserAsync(UserAuthRequestDto request)
    {
        var apiUrl = "/api/user/auth";
        try
        {
            var httpRequest = CreateRequestWithAuth(HttpMethod.Post, apiUrl);
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserAuthResponseDto>(responseContent, _jsonOptions);
        }
        catch (Exception ex)
        {
            var msg = $"Error authenticating user via function API {apiUrl}";
            _logger.LogWarning(msg);
            _logger.LogError(ex, "Error authenticating user via function API");
            AnsiConsole.MarkupLine("[red]Error: Unable to authenticate user. Please try again later.[/]");
            return null;
        }
    }

    public GameSession CreateLocalGame(Difficulty difficulty)
    {
        var random = new Random();
        var questions = new List<MathQuestion>();
        
        var settings = DifficultySettings.GetSettings(difficulty);

        for (int i = 0; i < settings.QuestionCount; i++)
        {
            var operation = settings.AllowedOperations[random.Next(settings.AllowedOperations.Length)];
            var maxNumber = (int)Math.Pow(10, settings.MaxDigits) - 1;
            var number1 = random.Next(1, maxNumber);
            var number2 = random.Next(1, maxNumber);

            // Ensure division results in reasonable numbers that respect max digits
            if (operation == MathOperation.Division)
            {
                // For division, make sure the result and numbers are within bounds
                var divisor = random.Next(2, Math.Min(maxNumber, 20)); // Keep divisor reasonable
                var quotient = random.Next(1, maxNumber / divisor); // Ensure result fits
                number1 = quotient * divisor;
                number2 = divisor;
            }

            var correctAnswer = operation switch
            {
                MathOperation.Addition => number1 + number2,
                MathOperation.Subtraction => number1 - number2,
                MathOperation.Multiplication => number1 * number2,
                MathOperation.Division => (double)number1 / number2,
                _ => 0
            };

            questions.Add(new MathQuestion
            {
                Id = i + 1,
                Number1 = number1,
                Number2 = number2,
                Operation = operation,
                CorrectAnswer = correctAnswer
            });
        }

        return new GameSession
        {
            Difficulty = difficulty,
            Questions = questions
        };
    }
}