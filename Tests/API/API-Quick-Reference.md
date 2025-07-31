# MathStorm API Quick Reference

## Quick Start
1. Start Function App: `cd src/functions/MathStorm.Functions && func start`
2. Open `test.http` in VS Code with REST Client extension
3. Click "Send Request" above any HTTP test

## API Endpoints

### 🎮 Generate Game
```http
GET http://localhost:7071/api/game?difficulty=Expert
```

### 📊 Submit Results  
```http
POST http://localhost:7071/api/game/results
Content-Type: application/json

{
  "gameId": "test-game-001",
  "username": "TestUser",
  "difficulty": "Expert",
  "questions": [
    {
      "id": 1,
      "number1": 123,
      "number2": 456,
      "operation": "Addition", 
      "correctAnswer": 579,
      "userAnswer": 579,
      "timeInSeconds": 3.5,
      "percentageDifference": 0,
      "score": 10.5
    }
  ]
}
```

### 🏆 Get Leaderboard
```http
GET http://localhost:7071/api/leaderboard?difficulty=Expert&topCount=10
```

## Difficulty Levels
- `Beginner` - 5 questions, 2-digit numbers, addition/subtraction only
- `Novice` - 5 questions, 2-digit numbers, all operations  
- `Intermediate` - 10 questions, 3-digit numbers, all operations
- `Expert` - 10 questions, 4-digit numbers, all operations

## Math Operations
- `Addition` - a + b
- `Subtraction` - a - b  
- `Multiplication` - a × b
- `Division` - a ÷ b

## Response Codes
- `200` - Success
- `400` - Bad Request (invalid JSON/missing fields)
- `500` - Internal Server Error

## Test Workflow
1. **GET** `/api/game` → Copy `gameId` from response
2. **POST** `/api/game/results` → Use copied `gameId`  
3. **GET** `/api/leaderboard` → Verify score was added