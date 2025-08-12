using Newtonsoft.Json;

namespace MathStorm.Web.Services;

public interface IBuildInfoService
{
    Task<BuildInfo?> GetBuildInfoAsync();
}

public class BuildInfoService : IBuildInfoService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BuildInfoService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private BuildInfo? _cachedBuildInfo;

    public BuildInfoService(HttpClient httpClient, ILogger<BuildInfoService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<BuildInfo?> GetBuildInfoAsync()
    {
        if (_cachedBuildInfo is not null)
        {
            return _cachedBuildInfo;
        }

        try
        {
            var baseUrl = GetBaseUrl();
            var url = $"{baseUrl}buildinfo.json";
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                _cachedBuildInfo = JsonConvert.DeserializeObject<BuildInfo>(json);
                return _cachedBuildInfo;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load build info");
        }

        return null;
    }

    private string GetBaseUrl()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is not null)
        {
            var request = httpContext.Request;
            return $"{request.Scheme}://{request.Host}/";
        }
        return "http://localhost:5000/"; // Fallback for development
    }
}