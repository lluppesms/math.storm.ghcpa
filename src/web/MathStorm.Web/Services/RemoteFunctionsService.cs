using System.Text;
using System.Text.Json;
using MathStorm.Common.DTOs;
using MathStorm.Common.Models;

namespace MathStorm.Web.Services;

public interface IRemoteFunctionsService
{
    Task<GameResponseDto?> GetGameAsync(Difficulty difficulty);
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
    public string? BaseFunctionUrl { get; set; }

    public RemoteFunctionsService(HttpClient httpClient, ILogger<RemoteFunctionsService> logger, IConfiguration config)
    {
        _httpClient = httpClient;
        _logger = logger;
        BaseFunctionUrl = config.GetValue<string>("FunctionService:BaseUrl");
    }

    public string GetBaseURL() => string.IsNullOrEmpty(BaseFunctionUrl) ? BaseFunctionUrl : "";

    public async Task<GameResponseDto?> GetGameAsync(Difficulty difficulty)
    {
        var content = string.Empty;
        var apiUrl = $"/api/game?difficulty={difficulty}";
        try
        {
            var response = await _httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            content = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(content) && content.Trim().StartsWith("{"))
            {
                var game = JsonSerializer.Deserialize<GameResponseDto>(content);
                return game;
            }
            _logger.LogError($"Error getting game from API {BaseFunctionUrl}{apiUrl}! Status: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            var msg = $"Error getting game from API {BaseFunctionUrl}{apiUrl}: Ex: {ExceptionHelper.GetExceptionMessage(ex)}";
            _logger.LogError(msg);
            return null;
        }
    }

    public async Task<GameResultsResponseDto?> SubmitGameResultsAsync(GameResultsRequestDto request)
    {
        var responseContent = string.Empty;
        var apiUrl = "/api/game/results";
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(apiUrl, content);
            response.EnsureSuccessStatusCode();

            responseContent = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(responseContent) && responseContent.Trim().StartsWith("{"))
            {
                return JsonSerializer.Deserialize<GameResultsResponseDto>(responseContent);
            }
            _logger.LogError($"Error submitting game results to API {BaseFunctionUrl}{apiUrl}! Status: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            var msg = $"Error submitting game results to API {BaseFunctionUrl}{apiUrl}: Ex: {ExceptionHelper.GetExceptionMessage(ex)}";
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

            var response = await _httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            content = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(content) && content.Trim().StartsWith("{"))
            {
                return JsonSerializer.Deserialize<LeaderboardResponseDto>(content);
            }
            _logger.LogError($"Error getting leaderboard from API {BaseFunctionUrl}{apiUrl}! Status: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            var msg = $"Error getting leaderboard from API {BaseFunctionUrl}{apiUrl}: Ex: {ExceptionHelper.GetExceptionMessage(ex)}";
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
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(apiUrl, content);
            response.EnsureSuccessStatusCode();

            responseContent = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(responseContent) && responseContent.Trim().StartsWith("{"))
            {
                return JsonSerializer.Deserialize<ResultsAnalysisResponseDto>(responseContent);
            }
            _logger.LogError($"Error analyzing game results via API {BaseFunctionUrl}{apiUrl}! Status: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            var msg = $"Error analyzing game results via API {BaseFunctionUrl}{apiUrl}: Ex: {ExceptionHelper.GetExceptionMessage(ex)}";
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
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(apiUrl, content);
            response.EnsureSuccessStatusCode();

            responseContent = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(responseContent) && responseContent.Trim().StartsWith("{"))
            {
                return JsonSerializer.Deserialize<UserAuthResponseDto>(responseContent);
            }
            _logger.LogError($"Error authenticating user from API {BaseFunctionUrl}{apiUrl}! Status: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            var msg = $"Error authenticating user from API {BaseFunctionUrl}{apiUrl}: Ex: {ExceptionHelper.GetExceptionMessage(ex)}";
            _logger.LogError(msg);
            return null;
        }
    }
}