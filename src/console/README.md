# Math Storm Console Application

A beautiful command-line interface for the Math Storm game built with [Spectre.Console](https://spectreconsole.net/).

## Features

- 🎮 **Interactive Gameplay**: Answer math questions with real-time scoring
- 🏆 **Leaderboard**: View top scores across all difficulty levels  
- ⚡ **Four Difficulty Levels**: From Beginner to Intermediate
- 🎨 **Rich Console UI**: Colorful interface with emojis and formatted text
- 📊 **Detailed Statistics**: Track accuracy, speed, and performance
- 🔄 **Continuous Play**: Play multiple games in one session

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Math Storm Functions API running (see [Functions README](../functions/README.md))

## Configuration

Update `appsettings.json` to point to your Functions API:

```json
{
  "FunctionService": {
    "BaseUrl": "https://your-function-app.azurewebsites.net"
  }
}
```

For local development, use:
```json
{
  "FunctionService": {
    "BaseUrl": "https://localhost:7071"
  }
}
```

## Running the Application

1. **Navigate to the console directory:**
   ```bash
   cd src/console
   ```

2. **Build the project:**
   ```bash
   dotnet build
   ```

3. **Run the application:**
   ```bash
   dotnet run --project MathStorm.Console
   ```

## Game Flow

### 1. Welcome Screen
```
__  __           _     _         ____    _                              
|  \/  |   __ _  | |_  | |__     / ___|  | |_    ___    _ __   _ __ ___  
| |\/| |  / _` | | __| | '_ \    \___ \  | __|  / _ \  | '__| | '_ ` _ \ 
| |  | | | (_| | | |_  | | | |    ___) | | |_  | (_) | | |    | | | | | |
|_|  |_|  \__,_|  \__| |_| |_|   |____/   \__|  \___/  |_|    |_| |_| |_|

Welcome to Math Storm! ⚡ Test your mathematical skills!
```

### 2. Player Name Entry
```
What's your name? > [Enter your name]
```

### 3. Main Menu
```
What would you like to do?
❯ 🎮 Play a Game
  🏆 View Leaderboard  
  🚪 Exit
```

### 4. Difficulty Selection
```
Select difficulty level:
❯ 🌱 Beginner - 5 questions, 2-digit numbers, Addition & Subtraction only
  🚀 Novice - 5 questions, 2-digit numbers, All operations
  ⚡ Expert - 10 questions, 3-digit numbers, All operations
  🔥 Intermediate - 10 questions, 3-digit numbers, All operations
```

### 5. Game Questions
```
════════════════ Question 1 of 5 ═══════════════

╔════════════════════════════════════════════════════╗
║                  Math Problem                       ║
╠════════════════════════════════════════════════════╣
║                                                    ║
║                   47 + 23 = ?                     ║
║                                                    ║
╚════════════════════════════════════════════════════╝

Your answer: > [Enter your answer]
✓ Correct! Time: 2.3s, Score: 95.7
```

### 6. Game Results
```
═══════════════════ Game Complete! ══════════════════

╔═══════════════════════════════════════════════════╗
║ 📊 Game Summary        │ Results                   ║
╞═══════════════════════════════════════════════════╡
║ Player                 │ YourName                  ║
║ Difficulty            │ Beginner                  ║
║ Questions             │ 5                         ║
║ Total Score           │ 467.3                     ║
║ Accuracy              │ 100.0% (5/5)              ║
║ Average Time          │ 3.2s                      ║
╚═══════════════════════════════════════════════════╝

✓ Results submitted successfully! 🎉
🏆 ADDED TO LEADERBOARD! (Rank #3) Congratulations! 🏆
```

### 7. Leaderboard
```
🏆 Leaderboard 🏆

╔══════╤══════════╤═══════╤════════════╤════════╗
║ Rank │ Player   │ Score │ Difficulty │ Date   ║
╞══════╪══════════╪═══════╪════════════╪════════╡
║ 🥇 1st│ Player1  │ 892.3 │ ⚡ Expert  │ 12/15  ║
║ 🥈 2nd│ Player2  │ 756.8 │ 🚀 Novice  │ 12/14  ║
║ 🥉 3rd│ Player3  │ 634.2 │ 🌱 Beginner│ 12/13  ║
╚══════╧══════════╧═══════╧════════════╧════════╝
```

## Difficulty Levels

| Level | Questions | Number Range | Operations | Description |
|-------|-----------|--------------|------------|-------------|
| 🌱 **Beginner** | 5 | 2-digit | +, - | Perfect for learning |
| 🚀 **Novice** | 5 | 2-digit | +, -, ×, ÷ | All operations intro |
| ⚡ **Expert** | 10 | 3-digit | +, -, ×, ÷ | Challenge yourself |
| 🔥 **Intermediate** | 10 | 3-digit | +, -, ×, ÷ | Maximum difficulty |

## Scoring System

Your score is calculated based on:
- **Accuracy**: How close your answer is to the correct one
- **Speed**: Bonus points for quick answers (up to 50 points)
- **Formula**: `(accuracy_score + time_bonus) / 1.5`

## Architecture

The console app follows the same architecture as the web application:

```
Console App → Functions API → Cosmos DB
```

- **No direct database access** - all data operations go through the Functions API
- **Shared models** - uses the same DTOs and models as the web application
- **Dependency injection** - proper DI container with HttpClient and logging
- **Configuration-driven** - Function service URL configurable via appsettings.json

## Dependencies

- **Spectre.Console** (0.50.0) - Rich console UI framework
- **Microsoft.Extensions.*** - Dependency injection, configuration, logging
- **MathStorm.Common** - Shared models and DTOs

## Development

To modify the console app:

1. **Service layer**: Edit `Services/ConsoleMathStormService.cs` for API communication
2. **UI flow**: Edit `Program.cs` for game flow and user interaction  
3. **Configuration**: Edit `appsettings.json` for environment settings
4. **Dependencies**: Edit `globalUsings.cs` for common namespace imports

## Troubleshooting

### "Unable to connect to game service"
- Ensure the Functions API is running
- Check the `BaseUrl` in `appsettings.json`
- Verify network connectivity

### "Unable to submit results"  
- Functions API may be down
- Check API endpoint `/api/game/results`
- Results will still be displayed locally

### "Unable to load leaderboard"
- Functions API may be down  
- Check API endpoint `/api/leaderboard`
- Game can still be played without leaderboard

## Contributing

When adding new features:
1. Follow the existing Spectre.Console patterns
2. Add proper error handling with user-friendly messages
3. Maintain the colorful, emoji-rich UI style
4. Test with both working and non-working API connections