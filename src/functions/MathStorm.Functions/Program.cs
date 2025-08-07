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

        // Add OpenAI services
        var openAIEndpoint = context.Configuration["OpenAI:Endpoint"];
        if (!string.IsNullOrEmpty(openAIEndpoint))
        {
            Console.WriteLine($"Connecting to OpenAI endpoint {openAIEndpoint} with managed identity...");
            services.AddSingleton<AzureOpenAIClient>(provider =>
            {
                var endpoint = new Uri(openAIEndpoint);
                return new AzureOpenAIClient(endpoint, new DefaultAzureCredential());
            });
            services.AddScoped<IResultsAnalysisService, ResultsAnalysisService>();
        }
        else
        {
            // Fallback for local development or when using API key
            var openAIKey = context.Configuration["OpenAI:ApiKey"];
            if (!string.IsNullOrEmpty(openAIKey))
            {
                Console.WriteLine("Connecting to OpenAI with API key...");
                services.AddSingleton<AzureOpenAIClient>(provider =>
                {
                    var endpoint = new Uri(context.Configuration["OpenAI:Endpoint"] ?? "https://api.openai.com");
                    return new AzureOpenAIClient(endpoint, new AzureKeyCredential(openAIKey));
                });
                services.AddScoped<IResultsAnalysisService, ResultsAnalysisService>();
            }
            else
            {
                // Add a mock service for development/testing when no OpenAI is configured
                Console.WriteLine("No OpenAI configuration found, using mock service...");
                services.AddScoped<IResultsAnalysisService, MockResultsAnalysisService>();
            }
        }

        var cosmosClientOptions = new CosmosClientOptions
        {
            MaxRetryAttemptsOnRateLimitedRequests = 3,
            MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30)
        };
        // Add Cosmos DB services based on whether endpoint name exists
        var cosmosEndpoint = context.Configuration["CosmosDb:Endpoint"];
        if (!string.IsNullOrEmpty(cosmosEndpoint))
        {
            Console.WriteLine($"Connecting to Cosmos endpoint {cosmosEndpoint} with managed identity...");
            var endpointUri = Environment.GetEnvironmentVariable("CosmosDb:Endpoint");
            services.AddSingleton<CosmosClient>(provider =>
            {
                var cosmosClient = new CosmosClient(endpointUri, new DefaultAzureCredential(), cosmosClientOptions);
                return cosmosClient;
            });
            services.AddScoped<ICosmosDbService, CosmosDbService>();
        }
        else
        {
            // Add Cosmos DB services based on whether connection string exists
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
        }
    })
    .Build();

host.Run();
