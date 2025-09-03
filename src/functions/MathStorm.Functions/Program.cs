using Azure.Identity;
using Microsoft.Azure.Cosmos;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(
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
             config.AddEnvironmentVariables();
             config.AddUserSecrets<Program>();
         }
     })
    .ConfigureServices((context, services) => // Added 'context' parameter here
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddScoped<IGameService, GameService>();

        // Add OpenAI services - ResultsAnalysisService will manage multiple clients internally
        services.AddScoped<IResultsAnalysisService, ResultsAnalysisService>();

        var cosmosClientOptions = new CosmosClientOptions
        {
            MaxRetryAttemptsOnRateLimitedRequests = 3,
            MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30)
        };

        // Add Cosmos DB services based on whether endpoint name exists
        Console.WriteLine("Checking for Cosmos DB configuration...");
        var cosmosEndpoint = context.Configuration["CosmosDb:Endpoint"];
        if (!string.IsNullOrEmpty(cosmosEndpoint))
        {
            Console.WriteLine($"Connecting to Cosmos endpoint {cosmosEndpoint} with managed identity...");
            services.AddSingleton<CosmosClient>(provider =>
            {
                var creds = new DefaultAzureCredential();
                // for some local development, you need to specify the AD Tenant to make the creds work...
                var visualStudioTenantId = context.Configuration["VisualStudioTenantId"];
                if (!string.IsNullOrEmpty(visualStudioTenantId))
                {
                    Console.WriteLine($"Overwriting tenant for managed identity credentials...");
                    creds = new DefaultAzureCredential(new DefaultAzureCredentialOptions
                    {
                        ExcludeEnvironmentCredential = true,
                        ExcludeManagedIdentityCredential = true,
                        TenantId = visualStudioTenantId
                    });
                }
                var cosmosClient = new CosmosClient(cosmosEndpoint, creds, cosmosClientOptions);
                return cosmosClient;
            });
            services.AddScoped<ICosmosDbService, CosmosDbService>();
        }
        else
        {
            // Add Cosmos DB services based on whether connection string exists
            Console.WriteLine("No Cosmos Endpoint Found... Looking for Cosmos DB connection string...");
            var connectionString = context.Configuration["CosmosDb:ConnectionString"];
            if (!string.IsNullOrEmpty(connectionString))
            {
                var accountName = connectionString?[..connectionString.IndexOf("AccountKey")].Replace("AccountEndpoint=https://", "").Replace(".documents.azure.com:443/;", "").Replace("/;", "");
                Console.WriteLine($"Connecting to Cosmos DB Account {accountName} with a key...");
                services.AddSingleton<CosmosClient>(provider =>
                {
                    var cosmosClient = new CosmosClient(connectionString, cosmosClientOptions);
                    return cosmosClient;
                });
                services.AddScoped<ICosmosDbService, CosmosDbService>();
            }
            else
            {
                Console.WriteLine("*******  No valid Cosmos DB configuration found!!!! *******");
                Console.WriteLine("*******         Using MOCK Cosmos Service!!!!       *******");
                services.AddScoped<ICosmosDbService, MockCosmosDbService>();
            }
        }
    })
    .Build();

host.Run();
