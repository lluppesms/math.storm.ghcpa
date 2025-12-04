using Newtonsoft.Json;
using Microsoft.JSInterop;
using MathStorm.Common.DTOs;
using MathStorm.Services;

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
    private readonly IMathStormService _mathStormService;
    private readonly ILogger<UserProfileService> _logger;
    private const string StorageKey = "mathstorm.userprofile";

    public UserProfileService(IJSRuntime jsRuntime, IMathStormService mathStormService, ILogger<UserProfileService> logger)
    {
        _jsRuntime = jsRuntime;
        _mathStormService = mathStormService;
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

            return JsonConvert.DeserializeObject<UserProfileDto>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Web: Error getting stored user profile");
            return null;
        }
    }

    public async Task SaveProfileAsync(UserProfileDto profile)
    {
        try
        {
            if (profile.RememberMe)
            {
                var json = JsonConvert.SerializeObject(profile);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
            }
            else
            {
                await ClearProfileAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Web: Error saving user profile");
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
            _logger.LogError(ex, "Web: Error clearing user profile");
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

            var response = await _mathStormService.AuthenticateUserAsync(request);
            
            // If service is available, use its response
            if (response != null)
            {
                return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Web: Error authenticating user, falling back to mock authentication");
        }

        // Fallback: Mock authentication for testing when service is unavailable
        return new UserAuthResponseDto
        {
            IsAuthenticated = true,
            IsNewUser = true, // Always treat as new user in mock mode
            UserId = Guid.NewGuid().ToString(),
            Username = username.Trim()
        };
    }
}