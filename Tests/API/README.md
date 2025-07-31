# MathStorm Azure Function API Testing

This directory contains manual API testing tools for the MathStorm Azure Functions. Use these tools to test API functionality during development and after deployment.

## Files

- **`test.http`** - REST Client file with comprehensive API tests
- **`README.md`** - This documentation file

## Prerequisites

### For Local Testing
1. **Azure Functions Core Tools** - Install from [Microsoft Docs](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)
2. **VS Code with REST Client Extension** - Install the [REST Client extension](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)
3. **Local Function App Running** - Start the Azure Function locally on port 7071

### For Production Testing
1. **Deployed Azure Function App** - Update the `@prodBaseUrl` variable in `test.http` with your actual Function App URL
2. **REST Client Extension** - Same as local testing

## Getting Started

### 1. Start Local Development Environment

```bash
# Navigate to the Functions project
cd src/functions/MathStorm.Functions

# Start the Function App locally
func start
```

The Function App will start on `http://localhost:7071`

### 2. Open test.http in VS Code

1. Open VS Code
2. Open the file `Tests/API/test.http`
3. Ensure you have the REST Client extension installed
4. The file is configured to use localhost by default

### 3. Run API Tests

In VS Code with the REST Client extension:

1. Click "Send Request" above any HTTP request in the file
2. View the response in the right panel
3. Examine status codes, headers, and response bodies

## API Endpoints Tested

### 1. Game Generation (`GET /api/game`)

**Purpose**: Generate new math questions based on difficulty level.

**Test Cases**:
- Default difficulty (Expert)
- All difficulty levels (Beginner, Novice, Intermediate, Expert)
- Invalid difficulty (should default to Expert)

**Expected Response**:
```json
{
  "gameId": "uuid-string",
  "difficulty": "Expert",
  "questions": [
    {
      "id": 1,
      "number1": 1234,
      "number2": 5678,
      "operation": "Addition",
      "correctAnswer": 6912,
      "questionText": "1234 + 5678 = ?"
    }
  ]
}
```

### 2. Game Results Submission (`POST /api/game/results`)

**Purpose**: Submit completed game results and update leaderboards.

**Test Cases**:
- Perfect scores (all correct answers)
- Partial scores (some wrong answers)
- Different difficulty levels
- Edge cases (long usernames, special characters)
- Invalid requests (missing fields, malformed JSON)

**Expected Response**:
```json
{
  "gameId": "test-game-001",
  "totalScore": 42.5,
  "addedToLeaderboard": true,
  "leaderboardRank": 5
}
```

### 3. Leaderboard Retrieval (`GET /api/leaderboard`)

**Purpose**: Get top players for specific difficulty or global leaderboard.

**Test Cases**:
- Global leaderboard (no difficulty specified)
- Difficulty-specific leaderboards
- Different entry counts
- Invalid parameters

**Expected Response**:
```json
{
  "difficulty": "Expert",
  "entries": [
    {
      "id": "entry-id",
      "difficulty": "Expert",
      "username": "TopPlayer",
      "userId": "user-id",
      "gameId": "game-id",
      "score": 25.5,
      "achievedAt": "2024-01-15T10:30:00Z",
      "rank": 1
    }
  ]
}
```

## Testing Workflow

### Complete Game Flow Test

1. **Generate Game**: Use `GET /api/game?difficulty=Intermediate`
2. **Copy Game ID**: From the response, copy the `gameId` value
3. **Submit Results**: Update the "Step 2" POST request with the actual Game ID
4. **Check Leaderboard**: Verify the score appears in the leaderboard

### Error Testing

Test error scenarios to ensure proper error handling:

- Invalid JSON in POST requests
- Missing required fields
- Invalid difficulty levels
- Invalid topCount values

### Performance Testing

- Test with large topCount values
- Submit multiple game results quickly
- Test concurrent requests

## Common Issues and Troubleshooting

### Function App Not Running
**Error**: Connection refused to localhost:7071
**Solution**: Ensure the Azure Function is running locally with `func start`

### Invalid JSON
**Error**: 400 Bad Request
**Solution**: Check JSON syntax in POST request bodies

### Missing Function Keys
**Error**: 401 Unauthorized (for production testing)
**Solution**: Update authentication settings or use appropriate function keys

### Cosmos DB Connection Issues
**Error**: 500 Internal Server Error
**Solution**: Check Cosmos DB connection string and ensure database/containers exist

## Environment Variables

For local testing, ensure these are set in `local.settings.json`:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "CosmosConnectionString": "your-cosmos-connection-string"
  }
}
```

## Production Testing

1. Update `@prodBaseUrl` in `test.http` with your actual Function App URL
2. Change `@baseUrl = {{prodBaseUrl}}` 
3. Ensure proper authentication if required
4. Test all endpoints to verify deployment

## Swagger UI

When deployed, you can also test APIs using the built-in Swagger UI at:
`https://[your-function-app].azurewebsites.net/api/swagger/ui`

## Advanced Testing

### Load Testing
For load testing, consider using tools like:
- Azure Load Testing
- Apache JMeter
- Artillery.io

### Integration Testing
The `.http` file can be integrated into CI/CD pipelines using tools that support REST Client format or converted to other testing frameworks.