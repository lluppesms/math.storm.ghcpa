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
        try
        {
            var response = await _httpClient.GetAsync($"/api/game?difficulty={difficulty}");
            response.EnsureSuccessStatusCode();

            content = await response.Content.ReadAsStringAsync();
            var game = JsonSerializer.Deserialize<GameResponseDto>(content);
            return game;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting game from function API");
            _logger.LogInformation($"Error getting game from function API {BaseFunctionUrl}: {content}");
            return null;
        }
    }

    public async Task<GameResultsResponseDto?> SubmitGameResultsAsync(GameResultsRequestDto request)
    {
        var responseContent = string.Empty;
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/game/results", content);
            response.EnsureSuccessStatusCode();

            responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GameResultsResponseDto>(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting game results to function API");
            _logger.LogInformation($"Error getting game results from function API {BaseFunctionUrl}: {responseContent}");
            return null;
        }
    }

    public async Task<LeaderboardResponseDto?> GetLeaderboardAsync(string? difficulty = null, int topCount = 10)
    {
        var content = string.Empty;
        try
        {
            var url = $"/api/leaderboard?topCount={topCount}";
            if (!string.IsNullOrEmpty(difficulty))
            {
                url += $"&difficulty={difficulty}";
            }

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<LeaderboardResponseDto>(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leaderboard from function API");
            _logger.LogInformation($"Error getting leaderboard from function API {BaseFunctionUrl}: {content}");
            return null;
        }
    }
}