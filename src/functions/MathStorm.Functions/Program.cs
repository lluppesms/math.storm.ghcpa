using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Cosmos;
using MathStorm.Shared.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        
        // Add game service
        services.AddScoped<IGameService, GameService>();
        
        // Add Cosmos DB services
        services.AddSingleton<CosmosClient>(provider =>
        {
            var connectionString = Environment.GetEnvironmentVariable("CosmosDb__ConnectionString");
            return new CosmosClient(connectionString);
        });
        
        services.AddScoped<ICosmosDbService>(provider => 
        {
            // For now, we'll use a mock service - we'll implement the real one later
            return new MockCosmosDbService();
        });
    })
    .Build();

host.Run();
