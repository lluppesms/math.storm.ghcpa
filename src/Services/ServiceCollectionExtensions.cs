using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

namespace MathStorm.Services;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds MathStorm services to the service collection.
    /// This configures all necessary services for the game to function without Azure Functions.
    /// </summary>
    public static IServiceCollection AddMathStormServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add core game service
        services.AddScoped<IGameService, GameService>();

        // Add the main MathStorm service
        services.AddScoped<IMathStormService, MathStormService>();

        // Configure Cosmos DB based on available settings
        var cosmosClientOptions = new CosmosClientOptions
        {
            MaxRetryAttemptsOnRateLimitedRequests = 3,
            MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30)
        };

        var cosmosEndpoint = configuration["CosmosDb:Endpoint"];
        var connectionString = configuration["CosmosDb:ConnectionString"];

        if (!string.IsNullOrEmpty(cosmosEndpoint))
        {
            // Use Managed Identity authentication
            services.AddSingleton<CosmosClient>(provider =>
            {
                var creds = new DefaultAzureCredential();
                var visualStudioTenantId = configuration["VisualStudioTenantId"];
                if (!string.IsNullOrEmpty(visualStudioTenantId))
                {
                    creds = new DefaultAzureCredential(new DefaultAzureCredentialOptions
                    {
                        ExcludeEnvironmentCredential = true,
                        ExcludeManagedIdentityCredential = true,
                        TenantId = visualStudioTenantId
                    });
                }
                return new CosmosClient(cosmosEndpoint, creds, cosmosClientOptions);
            });
            services.AddScoped<ICosmosDbService, CosmosDbService>();
        }
        else if (!string.IsNullOrEmpty(connectionString))
        {
            // Use connection string authentication
            services.AddSingleton<CosmosClient>(_ => new CosmosClient(connectionString, cosmosClientOptions));
            services.AddScoped<ICosmosDbService, CosmosDbService>();
        }
        else
        {
            // Use mock service when no Cosmos configuration is found
            services.AddScoped<ICosmosDbService, MockCosmosDbService>();
        }

        // Configure Results Analysis Service
        var openAIModelsSection = configuration.GetSection("OpenAI:Models");
        if (openAIModelsSection.GetChildren().Any())
        {
            services.AddScoped<IResultsAnalysisService, ResultsAnalysisService>();
        }
        else
        {
            // Use mock service when no OpenAI configuration is found
            services.AddScoped<IResultsAnalysisService, MockResultsAnalysisService>();
        }

        return services;
    }
}
