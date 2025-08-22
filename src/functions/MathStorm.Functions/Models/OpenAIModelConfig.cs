namespace MathStorm.Functions.Models;

public class OpenAIModelConfig
{
    public string DeploymentName { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}

public class OpenAIConfiguration
{
    public Dictionary<string, OpenAIModelConfig> Models { get; set; } = new();
    public string DefaultModel { get; set; } = "gpt-4o-mini";
    public string Temperature { get; set; } = "0.8";
}
