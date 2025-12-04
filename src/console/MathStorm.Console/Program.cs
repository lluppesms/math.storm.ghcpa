using MathStorm.Services;

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

// Add configuration
services.AddSingleton<IConfiguration>(configuration);

// Add MathStorm services (direct calls instead of Azure Functions HTTP calls)
services.AddMathStormServices(configuration);

// Add our console-specific services
services.AddScoped<IConsoleMathStormService, ConsoleMathStormService>();
services.AddScoped<MathStorm.Console.GameLogic>();

var serviceProvider = services.BuildServiceProvider();

// Start the game
var gameLogic = serviceProvider.GetRequiredService<MathStorm.Console.GameLogic>();
await gameLogic.RunAsync();