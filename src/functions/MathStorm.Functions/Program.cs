using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Cosmos;
using MathStorm.Shared.Services;
using MathStorm.Functions.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(
      builder =>
      {
          builder.UseMiddleware<MyExceptionHandler>();
      }
    )
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Add game service
        services.AddScoped<IGameService, GameService>();

        var connectionString = Environment.GetEnvironmentVariable("CosmosDb__ConnectionString");
        // Add Cosmos DB services
        if (!string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("Using Cosmos DB connection string from environment variable.");
            services.AddSingleton<CosmosClient>(provider =>
            {
                return new CosmosClient(connectionString);
            });
        }
        else
        {
            Console.WriteLine("Cosmos DB connection string not found -- using mock environment!");
            services.AddSingleton<MockCosmosDbService>();
        }

        services.AddScoped<ICosmosDbService, MathStorm.Functions.Services.CosmosDbService>();
    })
    .Build();

host.Run();
