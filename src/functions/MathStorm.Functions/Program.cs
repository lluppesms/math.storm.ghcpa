using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Cosmos;
using MathStorm.Core.Services;
using MathStorm.Common.Services;
using MathStorm.Functions.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(
      builder =>
      {
          builder.UseMiddleware<MyExceptionHandler>();
      }
    )
    .ConfigureAppConfiguration((hostContext, config) =>
     {
         if (hostContext.HostingEnvironment.IsDevelopment())
         {
             config.AddJsonFile("local.settings.json");
             config.AddUserSecrets<Program>();
         }
     })
    .ConfigureServices((context, services) => // Added 'context' parameter here
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Add game service
        services.AddScoped<IGameService, GameService>();

        var connectionString = context.Configuration["CosmosDb:ConnectionString"];

        // Add Cosmos DB services based on whether connection string exists
        if (!string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("Using Cosmos DB connection string from environment variable.");
            services.AddSingleton<CosmosClient>(provider =>
            {
                return new CosmosClient(connectionString);
            });
            services.AddScoped<ICosmosDbService, CosmosDbService>();
        }
        else
        {
            Console.WriteLine("Cosmos DB connection string not found -- using mock environment!");
            services.AddScoped<ICosmosDbService, MockCosmosDbService>();
        }
    })
    .Build();

host.Run();
