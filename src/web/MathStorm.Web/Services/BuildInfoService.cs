using Newtonsoft.Json;

namespace MathStorm.Web.Services;

public interface IBuildInfoService
{
    Task<BuildInfo?> GetBuildInfoAsync();
}

public class BuildInfoService : IBuildInfoService
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<BuildInfoService> _logger;
    private BuildInfo? _cachedBuildInfo;

    public BuildInfoService(IWebHostEnvironment webHostEnvironment, ILogger<BuildInfoService> logger)
    {
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
    }

    public async Task<BuildInfo?> GetBuildInfoAsync()
    {
        if (_cachedBuildInfo is not null)
        {
            return _cachedBuildInfo;
        }

        try
        {
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "buildinfo.json");
            
            if (File.Exists(filePath))
            {
                var json = await File.ReadAllTextAsync(filePath);
                _cachedBuildInfo = JsonConvert.DeserializeObject<BuildInfo>(json);
                return _cachedBuildInfo;
            }
            else
            {
                _logger.LogInformation("BuildInfo.json file not found at {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load build info from file");
        }

        return null;
    }
}