using System.Text;
using System.Text.Json;
using MathStorm.Common.DTOs;
using MathStorm.Common.Models;

namespace MathStorm.Web.Services;

public interface IMathStormFunctionService
{
    Task<GameResponseDto?> GetGameAsync(Difficulty difficulty);
    Task<GameResultsResponseDto?> SubmitGameResultsAsync(GameResultsRequestDto request);
    Task<LeaderboardResponseDto?> GetLeaderboardAsync(string? difficulty = null, int topCount = 10);
}

public class MathStormFunctionService : IMathStormFunctionService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MathStormFunctionService> _logger;

    public MathStormFunctionService(HttpClient httpClient, ILogger<MathStormFunctionService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<GameResponseDto?> GetGameAsync(Difficulty difficulty)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/game?difficulty={difficulty}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var game = JsonSerializer.Deserialize<GameResponseDto>(content);
            return game;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting game from function API");
            return null;
        }
    }

    public async Task<GameResultsResponseDto?> SubmitGameResultsAsync(GameResultsRequestDto request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/game/results", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GameResultsResponseDto>(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting game results to function API");
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
            return JsonSerializer.Deserialize<LeaderboardResponseDto>(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leaderboard from function API");
            return null;
        }
    }
}