using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Cosmos;
using MathStorm.Shared.Services;
using MathStorm.Functions.Services;

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
        
        services.AddScoped<ICosmosDbService, MathStorm.Functions.Services.CosmosDbService>();
    })
    .Build();

host.Run();
