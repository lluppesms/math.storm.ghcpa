using Microsoft.Azure.Cosmos;

namespace MathStorm.Web.Services;

public class CosmosDbInitializationService
{
    private readonly CosmosClient _cosmosClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CosmosDbInitializationService> _logger;

    public CosmosDbInitializationService(CosmosClient cosmosClient, IConfiguration configuration, ILogger<CosmosDbInitializationService> logger)
    {
        _cosmosClient = cosmosClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            var databaseName = _configuration["CosmosDb:DatabaseName"];
            var database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);
            
            // Create Users container
            await database.Database.CreateContainerIfNotExistsAsync(
                _configuration["CosmosDb:ContainerNames:Users"],
                "/id"
            );
            
            // Create Games container 
            await database.Database.CreateContainerIfNotExistsAsync(
                _configuration["CosmosDb:ContainerNames:Games"],
                "/id"
            );
            
            // Create Leaderboard container
            await database.Database.CreateContainerIfNotExistsAsync(
                _configuration["CosmosDb:ContainerNames:Leaderboard"],
                "/id"
            );
            
            _logger.LogInformation("Cosmos DB initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing Cosmos DB");
            throw;
        }
    }
}