# Local Development Setup

This document explains how to run the Math Storm application locally for development purposes.

## Architecture Overview

The Math Storm application has been architected with a true API-first approach:

- **Web Application**: Blazor Server application that provides the user interface
- **Azure Functions**: Serverless backend that handles all game logic and data operations
- **Shared Library**: Common models, DTOs, and services used by both projects

## Prerequisites

- .NET 8 SDK
- Azure Functions Core Tools (`npm install -g azure-functions-core-tools@4 --unsafe-perm true`)
- Visual Studio 2022 or VS Code with C# extension

## Running Locally

### Important: Function App Dependency

**The web application requires the Azure Function to be running locally to work properly.** The web app calls the Function APIs for:

- **Game Creation**: Getting questions for each difficulty level
- **Results Submission**: Saving completed games and updating leaderboards  
- **Leaderboard Display**: Retrieving top scores for each difficulty

### Step 1: Start the Azure Function

```bash
cd src/functions/MathStorm.Functions
func start
```

The Function app will start on `http://localhost:7071` and provide these endpoints:
- `GET /api/game?difficulty={level}` - Get game questions
- `POST /api/game/results` - Submit game results  
- `GET /api/leaderboard?difficulty={level}` - Get leaderboard entries

### Step 2: Start the Web Application

In a separate terminal:

```bash
cd src/web/MathStorm.Web
dotnet run
```

The web application will start on `https://localhost:5001` and automatically connect to the Function app running on port 7071.

## Testing the Integration

1. Navigate to `https://localhost:5001`
2. Select a difficulty level and start a game
3. Complete the game and submit your score
4. Verify that your score appears on the leaderboard

## Development Notes

### Function Service Configuration

The web application is configured to call `http://localhost:7071` for Function APIs (see `Program.cs`):

```csharp
builder.Services.AddHttpClient<IMathStormFunctionService, MathStormFunctionService>(client =>
{
    // For development, point to local function app
    client.BaseAddress = new Uri("http://localhost:7071");
});
```

### Fallback Behavior

The `FunctionBasedGameService` includes fallback logic - if the Function API is unavailable, it will fall back to local game generation. However, results submission and leaderboard functionality will not work without the Function app running.

### Mock Services

For development convenience, the Function app uses mock implementations of external dependencies (like Cosmos DB) so it can run without requiring a full Azure environment.

## Troubleshooting

### Web App Loads But Games Don't Work
- Ensure the Azure Function is running on port 7071
- Check the browser's developer tools for HTTP errors
- Verify that the Function endpoints are accessible at `http://localhost:7071/api/game`

### "No scores recorded yet" Message
- This indicates the Function app is not running or not accessible
- The leaderboard API call is failing, so no scores can be retrieved

### Build Errors
- Ensure all NuGet packages are restored: `dotnet restore`
- Make sure you're using .NET 8 SDK
- Check that the MathStorm.Shared project builds successfully first