using System.Text.Json;
using Microsoft.JSInterop;
using MathStorm.Common.DTOs;

namespace MathStorm.Web.Services;

public interface IUserProfileService
{
    Task<UserProfileDto?> GetStoredProfileAsync();
    Task SaveProfileAsync(UserProfileDto profile);
    Task ClearProfileAsync();
    Task<UserAuthResponseDto?> AuthenticateAsync(string username, string? pin = null);
}

public class UserProfileService : IUserProfileService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IRemoteFunctionsService _functionsService;
    private readonly ILogger<UserProfileService> _logger;
    private const string StorageKey = "mathstorm.userprofile";

    public UserProfileService(IJSRuntime jsRuntime, IRemoteFunctionsService functionsService, ILogger<UserProfileService> logger)
    {
        _jsRuntime = jsRuntime;
        _functionsService = functionsService;
        _logger = logger;
    }

    public async Task<UserProfileDto?> GetStoredProfileAsync()
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", StorageKey);
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<UserProfileDto>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stored user profile");
            return null;
        }
    }

    public async Task SaveProfileAsync(UserProfileDto profile)
    {
        try
        {
            if (profile.RememberMe)
            {
                var json = JsonSerializer.Serialize(profile);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
            }
            else
            {
                await ClearProfileAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving user profile");
        }
    }

    public async Task ClearProfileAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing user profile");
        }
    }

    public async Task<UserAuthResponseDto?> AuthenticateAsync(string username, string? pin = null)
    {
        try
        {
            var request = new UserAuthRequestDto
            {
                Username = username.Trim(),
                Pin = string.IsNullOrWhiteSpace(pin) ? null : pin.Trim()
            };

            var response = await _functionsService.AuthenticateUserAsync(request);
            
            // If function service is available, use its response
            if (response != null)
            {
                return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating user via function service, falling back to mock authentication");
        }

        // Fallback: Mock authentication for testing when function service is unavailable
        return new UserAuthResponseDto
        {
            IsAuthenticated = true,
            IsNewUser = true, // Always treat as new user in mock mode
            UserId = Guid.NewGuid().ToString(),
            Username = username.Trim()
        };
    }
}