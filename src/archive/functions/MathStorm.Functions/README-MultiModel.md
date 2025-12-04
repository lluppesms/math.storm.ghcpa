# Multi-Model OpenAI Configuration

The MathStorm Results Analysis Service now supports multiple OpenAI models with different endpoints and API keys. This allows you to choose between different AI models for generating game result analysis.

## Configuration

### local.settings.json Structure

```json
{
  "OpenAI": {
    "Models": {
      "gpt_5_mini": {
        "DeploymentName": "gpt-5-mini", 
        "Endpoint": "https://your-openai-resource-premium.openai.azure.com/",
        "ApiKey": "your-premium-api-key-here"
      },
      "gpt_4o_mini": {
        "DeploymentName": "gpt-4o-mini",
        "Endpoint": "https://your-openai-resource.openai.azure.com/",
        "ApiKey": "your-api-key-here"
      },
      "gpt_35_turbo": {
        "DeploymentName": "gpt-35-turbo",
        "Endpoint": "https://your-openai-resource-legacy.openai.azure.com/",
        "ApiKey": "your-legacy-api-key-here"
      }
    },
    "DefaultModel": "gpt_4o_mini",
    "Temperature": "0.8"
  }
}
```

### Configuration Properties

- **Models**: Dictionary of available models, where the key is the model name and the value contains the model configuration
- **DeploymentName**: The deployment name for the model in Azure OpenAI
- **Endpoint**: The Azure OpenAI endpoint URL for this model
- **ApiKey**: The API key for accessing this specific endpoint
- **DefaultModel**: The default model to use when no model is specified in requests
- **Temperature**: The creativity/randomness setting for AI responses (0.0 to 1.0)

## Usage

### API Endpoint

The Results Analysis API endpoint supports both personality and model selection:

```
POST /api/game/analysis?personality=pirate&model=gpt-4o
```

### Request Body

```json
{
  "gameId": "uuid-here",
  "username": "player123",
  "difficulty": "Expert",
  "totalScore": 85.5,
  "questions": [...],
  "personality": "pirate",
  "model": "gpt-4o"
}
```

### Available Models

- **gpt-4o-mini**: Fast, cost-effective model (default)
- **gpt-4o**: More advanced model with better reasoning
- **gpt-4**: Previous generation model

### Available Personalities

- **default**: Friendly and encouraging
- **comedyroast**: Funny but not mean-spirited
- **pirate**: Maritime-themed with pirate language
- **limerick**: Responses in limerick form
- **sportsbroadcaster**: Energetic sports commentary style
- **haiku**: Responses in haiku form
- **australian**: Australian slang and expressions
- **yourmother**: Supportive but slightly embarrassing mother figure

### Response

```json
{
  "gameId": "uuid-here",
  "personality": "pirate",
  "model": "gpt-4o",
  "analysis": "Arrr, matey! Ye did fine with yer numbers!",
  "generatedAt": "2025-08-08T10:30:00Z"
}
```

## Fallback Behavior

- If the specified model is not configured or unavailable, the service falls back to the default model
- If the default model is also unavailable, appropriate error handling provides fallback responses
- Invalid model names are automatically replaced with the default model

## Error Handling

The service includes comprehensive error handling:

1. **Configuration Errors**: Invalid endpoints or API keys are logged and skipped during initialization
2. **Runtime Errors**: If a model fails, the service attempts to use the default model
3. **Network Errors**: Appropriate fallback responses are provided for each personality type

## Example Configurations

### Development (Single Model)
```json
{
  "OpenAI": {
    "Models": {
      "gpt_4o_mini": {
        "DeploymentName": "gpt-4o-mini",
        "Endpoint": "https://dev-openai.openai.azure.com/",
        "ApiKey": "dev-api-key"
      }
    },
    "DefaultModel": "gpt_4o_mini",
    "Temperature": "0.8"
  }
}
```

### Production (Multiple Models)
```json
{
  "OpenAI": {
    "Models": {
      "gpt_4o_mini": {
        "DeploymentName": "gpt-4o-mini",
        "Endpoint": "https://prod-openai-basic.openai.azure.com/",
        "ApiKey": "basic-api-key"
      },
      "gpt_4o": {
        "DeploymentName": "gpt-4o",
        "Endpoint": "https://prod-openai-premium.openai.azure.com/",
        "ApiKey": "premium-api-key"
      },
      "gpt_4": {
        "DeploymentName": "gpt-4", 
        "Endpoint": "https://prod-openai-legacy.openai.azure.com/",
        "ApiKey": "legacy-api-key"
      }
    },
    "DefaultModel": "gpt_4o_mini",
    "Temperature": "0.7"
  }
}
```

## Benefits

1. **Cost Optimization**: Use cheaper models for basic analysis, premium models for special cases
2. **Performance Flexibility**: Choose faster models for real-time responses, slower but better models for detailed analysis
3. **Redundancy**: Multiple endpoints provide fallback options if one service is unavailable
4. **A/B Testing**: Easy to test different models and compare results
5. **Regional Distribution**: Different endpoints can be in different regions for better latency
