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

    public ConsoleMathStormService(HttpClient httpClient, ILogger<ConsoleMathStormService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<GameResponseDto?> GetGameAsync(Difficulty difficulty)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/game?difficulty={difficulty}");
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GameResponseDto>(content, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting game from function API");
            AnsiConsole.MarkupLine("[red]Error: Unable to connect to game service. Please try again later.[/]");
            return null;
        }
    }

    public async Task<GameResultsResponseDto?> SubmitGameResultsAsync(GameResultsRequestDto request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/game/results", content);
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GameResultsResponseDto>(responseContent, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting game results to function API");
            AnsiConsole.MarkupLine("[red]Error: Unable to submit results. Your score may not be saved.[/]");
            return null;
        }
    }

    public async Task<LeaderboardResponseDto?> GetLeaderboardAsync(string? difficulty = null, int topCount = 10)
    {
        try
        {
            var url = $"/api/leaderboard?topCount={topCount}";
            if (!string.IsNullOrEmpty(difficulty))
            {
                url += $"&difficulty={difficulty}";
            }
            
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<LeaderboardResponseDto>(content, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leaderboard from function API");
            AnsiConsole.MarkupLine("[red]Error: Unable to load leaderboard. Please try again later.[/]");
            return null;
        }
    }

    public async Task<ResultsAnalysisResponseDto?> AnalyzeGameResultsAsync(ResultsAnalysisRequestDto request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/game/analysis", content);
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ResultsAnalysisResponseDto>(responseContent, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing game results via function API");
            AnsiConsole.MarkupLine("[red]Error: Unable to analyze results. Please try again later.[/]");
            return null;
        }
    }

    public async Task<UserAuthResponseDto?> AuthenticateUserAsync(UserAuthRequestDto request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/user/auth", content);
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserAuthResponseDto>(responseContent, _jsonOptions);
        }
        catch (Exception ex)
        {
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