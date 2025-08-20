# API Configuration Guide

## Function App Base URL Configuration

The Math Storm web application communicates with Azure Functions for game data, leaderboards, and analysis. If you encounter 404 errors when clicking on analysis icons in the leaderboard, this is likely due to incorrect Function App URL configuration.

## Configuration Settings

### Local Development
For local development, the `FunctionService:BaseUrl` in `appsettings.json` should be:
```json
{
  "FunctionService:BaseUrl": "http://localhost:7071"
}
```

### Production/Deployed Environment
For deployed environments, update the `FunctionService:BaseUrl` to point to your Azure Function App:
```json
{
  "FunctionService:BaseUrl": "https://your-function-app-name.azurewebsites.net"
}
```

## Required API Endpoints

The web application expects these Function App endpoints to be available:

- `GET /api/game/{gameId}` - Retrieves complete game records for analysis modal
- `PUT /api/game/{gameId}/analysis` - Updates game analysis data
- `POST /api/game/results` - Submits game results
- `GET /api/leaderboard` - Gets leaderboard data
- `POST /api/game/analysis` - Requests AI analysis

## Troubleshooting 404 Errors

1. **Verify Function App is deployed and running**
2. **Check the `FunctionService:BaseUrl` configuration**
3. **Ensure the Function App contains all required endpoints**
4. **Check Azure Function App logs for errors**
5. **Verify network connectivity from web app to Function App**

## Error Messages

When the Function App is not accessible, users will see:
- Loading spinners that don't resolve in analysis modals
- Error messages in browser console
- "Game details could not be loaded" messages in modals

The enhanced error handling now provides more specific logging to help diagnose these configuration issues.