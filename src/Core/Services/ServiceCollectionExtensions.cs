using Microsoft.Extensions.DependencyInjection;

namespace MathStorm.Core;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds MathStorm services to the service collection.
    /// This configures all necessary services for the game to function.
    /// </summary>
    public static IServiceCollection AddMathStormServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add core game service
        services.AddScoped<IGameService, GameService>();

        // Add the main MathStorm service
        services.AddScoped<IMathStormService, MathStormService>();

        // Configure SQL Server data service based on available connection string
        var sqlConnectionString = configuration.GetConnectionString("SqlDb");

        if (!string.IsNullOrEmpty(sqlConnectionString))
        {
            // Use SQL Server when a connection string is configured
            services.AddScoped<IDataService, SqlDbService>();
        }
        else
        {
            // Use in-memory mock service for local development without a database
            services.AddScoped<IDataService, MockDataService>();
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
