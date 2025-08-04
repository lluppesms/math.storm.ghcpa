Console.OutputEncoding = Encoding.UTF8;

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Setup dependency injection
var services = new ServiceCollection();

// Add logging
services.AddLogging(builder =>
{
    builder.AddConfiguration(configuration.GetSection("Logging"));
    builder.AddConsole();
});

// Add HttpClient
var baseUrl = configuration.GetValue<string>("FunctionService:BaseUrl") ?? "https://localhost:7071";
services.AddSingleton(new HttpClient
{
    BaseAddress = new Uri(baseUrl)
});

// Add our services
services.AddScoped<IConsoleMathStormService, ConsoleMathStormService>();
services.AddScoped<MathStorm.Console.GameLogic>();

var serviceProvider = services.BuildServiceProvider();

// Start the game
var gameLogic = serviceProvider.GetRequiredService<MathStorm.Console.GameLogic>();
await gameLogic.RunAsync();