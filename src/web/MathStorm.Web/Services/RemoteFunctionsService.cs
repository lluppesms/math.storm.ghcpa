using System.Text;
using Newtonsoft.Json;
using MathStorm.Common.DTOs;
using MathStorm.Common.Models;

namespace MathStorm.Web.Services;

public interface IRemoteFunctionsService
{
    Task<GameResponseDto?> GetGameAsync(Difficulty difficulty);
    Task<Game?> GetGameByIdAsync(string gameId);
    Task<bool> UpdateGameAnalysisAsync(string gameId, string analysis);
    Task<GameResultsResponseDto?> SubmitGameResultsAsync(GameResultsRequestDto request);
    Task<LeaderboardResponseDto?> GetLeaderboardAsync(string? difficulty = null, int topCount = 10);
    Task<ResultsAnalysisResponseDto?> AnalyzeGameResultsAsync(ResultsAnalysisRequestDto request);
    Task<UserAuthResponseDto?> AuthenticateUserAsync(UserAuthRequestDto request);
    string GetBaseURL();
}

public class RemoteFunctionsService : IRemoteFunctionsService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RemoteFunctionsService> _logger;
    private readonly string? _apiKey;
    public string? BaseFunctionUrl { get; set; }

    public RemoteFunctionsService(HttpClient httpClient, ILogger<RemoteFunctionsService> logger, IConfiguration config)
    {
        _httpClient = httpClient;
        _logger = logger;
        BaseFunctionUrl = config.GetValue<string>("FunctionService:BaseUrl");
        _apiKey = config.GetValue<string>("FunctionService:APIKey");
    }

    public string GetBaseURL() => string.IsNullOrEmpty(BaseFunctionUrl) ? BaseFunctionUrl : "";

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
        var content = string.Empty;
        var apiUrl = $"/api/game?difficulty={difficulty}";
        try
        {
            var request = CreateRequestWithAuth(HttpMethod.Get, apiUrl);
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            content = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(content) && content.Trim().StartsWith("{"))
            {
                var game = JsonConvert.DeserializeObject<GameResponseDto>(content);
                return game;
            }
            _logger.LogError($"Web: Error getting game from API {BaseFunctionUrl}{apiUrl}! Status: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            var msg = $"Web: Error getting game from API {BaseFunctionUrl}{apiUrl}: Ex: {ExceptionHelper.GetExceptionMessage(ex)}";
            _logger.LogError(msg);
            return null;
        }
    }

    public async Task<Game?> GetGameByIdAsync(string gameId)
    {
        var content = string.Empty;
        var apiUrl = $"/api/game/{gameId}";
        try
        {
            var request = CreateRequestWithAuth(HttpMethod.Get, apiUrl);
            var response = await _httpClient.SendAsync(request);
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"Web: Game with ID {gameId} not found. API URL: {BaseFunctionUrl}{apiUrl}");
                return null;
            }
            
            response.EnsureSuccessStatusCode();

            content = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(content) && content.Trim().StartsWith("{"))
            {
                var game = JsonConvert.DeserializeObject<Game>(content);
                return game;
            }
            _logger.LogError($"Web: Error getting game by ID from API {BaseFunctionUrl}{apiUrl}! Status: {response.StatusCode}");
            return null;
        }
        catch (HttpRequestException ex)
        {
            var msg = $"Web: Network error getting game by ID from API {BaseFunctionUrl}{apiUrl}. Check if Function App is running and BaseUrl is correct: {ExceptionHelper.GetExceptionMessage(ex)}";
            _logger.LogError(msg);
            return null;
        }
        catch (Exception ex)
        {
            var msg = $"Web: Error getting game by ID from API {BaseFunctionUrl}{apiUrl}: Ex: {ExceptionHelper.GetExceptionMessage(ex)}";
            _logger.LogError(msg);
            return null;
        }
    }

    public async Task<bool> UpdateGameAnalysisAsync(string gameId, string analysis)
    {
        var responseContent = string.Empty;
        var apiUrl = $"/api/game/{gameId}/analysis";
        try
        {
            var request = CreateRequestWithAuth(HttpMethod.Put, apiUrl);
            var requestObj = new { Analysis = analysis };
            var json = JsonConvert.SerializeObject(requestObj);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return true;
        }
        catch (Exception ex)
        {
            var msg = $"Web: Error updating game analysis via API {BaseFunctionUrl}{apiUrl}: Ex: {ExceptionHelper.GetExceptionMessage(ex)}";
            _logger.LogError(msg);
            return false;
        }
    }

    public async Task<GameResultsResponseDto?> SubmitGameResultsAsync(GameResultsRequestDto request)
    {
        var responseContent = string.Empty;
        var apiUrl = "/api/game/results";
        try
        {
            var httpRequest = CreateRequestWithAuth(HttpMethod.Post, apiUrl);
            var json = JsonConvert.SerializeObject(request);
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            responseContent = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(responseContent) && responseContent.Trim().StartsWith("{"))
            {
                return JsonConvert.DeserializeObject<GameResultsResponseDto>(responseContent);
            }
            _logger.LogError($"Web: Error submitting game results to API {BaseFunctionUrl}{apiUrl}! Status: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            var msg = $"Web: Error submitting game results to API {BaseFunctionUrl}{apiUrl}: Ex: {ExceptionHelper.GetExceptionMessage(ex)}";
            _logger.LogError(msg);
            return null;
        }
    }

    public async Task<LeaderboardResponseDto?> GetLeaderboardAsync(string? difficulty = null, int topCount = 10)
    {
        var content = string.Empty;
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

            content = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(content) && content.Trim().StartsWith("{"))
            {
                return JsonConvert.DeserializeObject<LeaderboardResponseDto>(content);
            }
            _logger.LogError($"Web: Error getting leaderboard from API {BaseFunctionUrl}{apiUrl}! Status: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            var msg = $"Web: Error getting leaderboard from API {BaseFunctionUrl}{apiUrl}: Ex: {ExceptionHelper.GetExceptionMessage(ex)}";
            _logger.LogError(msg);
            return null;
        }
    }

    public async Task<ResultsAnalysisResponseDto?> AnalyzeGameResultsAsync(ResultsAnalysisRequestDto request)
    {
        var responseContent = string.Empty;
        var apiUrl = "/api/game/analysis";
        try
        {
            var httpRequest = CreateRequestWithAuth(HttpMethod.Post, apiUrl);
            var json = JsonConvert.SerializeObject(request);
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            responseContent = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(responseContent) && responseContent.Trim().StartsWith("{"))
            {
                return JsonConvert.DeserializeObject<ResultsAnalysisResponseDto>(responseContent);
            }
            _logger.LogError($"Web: Error analyzing game results via API {BaseFunctionUrl}{apiUrl}! Status: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            var msg = $"Web: Error analyzing game results via API {BaseFunctionUrl}{apiUrl}: Ex: {ExceptionHelper.GetExceptionMessage(ex)}";
            _logger.LogError(msg);
            return null;
        }
    }

    public async Task<UserAuthResponseDto?> AuthenticateUserAsync(UserAuthRequestDto request)
    {
        var responseContent = string.Empty;
        var apiUrl = "/api/user/auth";
        try
        {
            var httpRequest = CreateRequestWithAuth(HttpMethod.Post, apiUrl);
            var json = JsonConvert.SerializeObject(request);
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            responseContent = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(responseContent) && responseContent.Trim().StartsWith("{"))
            {
                return JsonConvert.DeserializeObject<UserAuthResponseDto>(responseContent);
            }
            _logger.LogError($"Web: Error authenticating user from API {BaseFunctionUrl}{apiUrl}! Status: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            var msg = $"Web: Error authenticating user from API {BaseFunctionUrl}{apiUrl}: Ex: {ExceptionHelper.GetExceptionMessage(ex)}";
            _logger.LogError(msg);
            return null;
        }
    }
}