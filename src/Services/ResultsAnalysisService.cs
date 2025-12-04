using MathStorm.Common.Helpers;

namespace MathStorm.Services;

public class ResultsAnalysisService : IResultsAnalysisService
{
    private readonly ILogger<ResultsAnalysisService> _logger;
    private readonly IConfiguration _configuration;
    private readonly float _temperature;
    private readonly string _defaultModel;
    private readonly Dictionary<string, AzureOpenAIClient> _openAIClients;
    private readonly string _promptsBasePath;

    public ResultsAnalysisService(ILogger<ResultsAnalysisService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _temperature = float.TryParse(configuration["OpenAI:Temperature"], out var temp) ? temp : 0.8f;
        _defaultModel = configuration["OpenAI:DefaultModel"] ?? "gpt-4o-mini";
        _openAIClients = InitializeOpenAIClients();
        _promptsBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Prompts");
    }

    private Dictionary<string, AzureOpenAIClient> InitializeOpenAIClients()
    {
        var clients = new Dictionary<string, AzureOpenAIClient>();
        var modelsSection = _configuration.GetSection("OpenAI:Models");

        foreach (var modelSection in modelsSection.GetChildren())
        {
            var modelName = modelSection.Key;
            var endpoint = modelSection["Endpoint"];
            var apiKey = modelSection["ApiKey"];

            if (!string.IsNullOrEmpty(endpoint) && !string.IsNullOrEmpty(apiKey))
            {
                try
                {
                    var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
                    clients[modelName] = client;
                    _logger.LogInformation($"Initialized OpenAI client for model: {modelName} endpoint {endpoint}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to initialize OpenAI client for model: {modelName}");
                }
            }
        }

        return clients;
    }

    public async Task<string> AnalyzeGameResultsAsync(ResultsAnalysisRequestDto request)
    {
        var deploymentUsed = string.Empty;
        try
        {
            var prompt = await GetChatMessagePrompt(request);
            var modelToUse = !string.IsNullOrEmpty(request.Model) ? request.Model : _defaultModel;

            _logger.LogInformation($"Analyzing game results for {request.Username} with {request.Personality} personality using model {modelToUse}");

            // Get the appropriate client and deployment name for the requested model
            if (!_openAIClients.TryGetValue(modelToUse, out var openAIClient))
            {
                _logger.LogWarning($"Model {modelToUse} not found, falling back to default model {_defaultModel}");
                if (!_openAIClients.TryGetValue(_defaultModel, out openAIClient))
                {
                    throw new InvalidOperationException($"No OpenAI client available for model {modelToUse} or default model {_defaultModel}");
                }
                modelToUse = _defaultModel;
            }

            var deploymentName = _configuration[$"OpenAI:Models:{modelToUse}:DeploymentName"];
            if (string.IsNullOrEmpty(deploymentName))
            {
                throw new InvalidOperationException($"No deployment name configured for model {modelToUse}");
            }
            deploymentUsed = deploymentName;

            var chatClient = openAIClient.GetChatClient(deploymentName);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(await GetSystemPrompt(request.Personality)),
                new UserChatMessage(prompt)
            };

            var chatCompletionOptions = new ChatCompletionOptions()
            {
                MaxOutputTokenCount = 500,
                Temperature = _temperature
            };

            var response = await chatClient.CompleteChatAsync(messages, chatCompletionOptions);
            var analysis = response.Value.Content[0].Text;

            _logger.LogInformation($"Generated analysis for {request.Username} using {modelToUse}: {analysis?.Length} characters");
            return analysis ?? "I'm speechless! Your performance has left me without words.";
        }
        catch (Exception ex)
        {
            var msg = ExceptionHelper.GetExceptionMessage(ex);
            _logger.LogError(ex, $"Error analyzing game results for user '{request.Username}', Deployment '{deploymentUsed}', {msg}");
            return GetFallbackResponse(request.Personality);
        }
    }

    private async Task<string> ReadPromptFileAsync(string fileName)
    {
        try
        {
            var filePath = Path.Combine(_promptsBasePath, fileName);
            if (File.Exists(filePath))
            {
                var content = await File.ReadAllTextAsync(filePath);
                return content.Trim();
            }
            else
            {
                _logger.LogWarning($"Prompt file not found: {filePath}");
                return GetDefaultSystemPrompt();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error reading prompt file: {fileName}");
            return GetDefaultSystemPrompt();
        }
    }

    private string GetDefaultSystemPrompt()
    {
        return "You are a friendly and encouraging AI assistant providing thoughtful analysis of math game performance.";
    }

    private async Task<string> GetSystemPrompt(string personality)
    {
        var fileName = $"{personality.ToLowerInvariant()}.txt";
        return await ReadPromptFileAsync(fileName);
    }

    private async Task<string> GetChatMessagePrompt(ResultsAnalysisRequestDto request)
    {
        try
        {
            var templatePath = Path.Combine(_promptsBasePath, "ChatMessagePrompt.txt");
            string template;

            if (File.Exists(templatePath))
            {
                template = await File.ReadAllTextAsync(templatePath);
            }
            else
            {
                _logger.LogWarning($"ChatMessagePrompt.txt not found at {templatePath}, using default template");
                template = GetDefaultChatMessageTemplate();
            }

            // Calculate values
            var correctAnswers = request.Questions.Count(q => Math.Abs(q.UserAnswer - q.CorrectAnswer) < 0.01);
            var averageTime = request.Questions.Average(q => q.TimeInSeconds);
            var fastestTime = request.Questions.Min(q => q.TimeInSeconds);
            var slowestTime = request.Questions.Max(q => q.TimeInSeconds);
            var accuracy = (double)correctAnswers / request.Questions.Count * 100;

            // Replace placeholders
            var prompt = template
                .Replace("{Username}", request.Username)
                .Replace("{Difficulty}", request.Difficulty)
                .Replace("{TotalQuestions}", request.Questions.Count.ToString())
                .Replace("{CorrectAnswers}", correctAnswers.ToString())
                .Replace("{Accuracy:F1}", accuracy.ToString("F1"))
                .Replace("{TotalScore:F1}", request.TotalScore.ToString("F1"))
                .Replace("{AverageTime:F1}", averageTime.ToString("F1"))
                .Replace("{FastestTime:F1}", fastestTime.ToString("F1"))
                .Replace("{SlowestTime:F1}", slowestTime.ToString("F1"));

            return prompt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating chat message prompt");
            return GetDefaultChatMessageTemplate();
        }
    }

    private string GetDefaultChatMessageTemplate()
    {
        return @"Analyze this math game performance:

Player: {Username}
Difficulty: {Difficulty}
Total Questions: {TotalQuestions}
Correct Answers: {CorrectAnswers}
Accuracy: {Accuracy:F1}%
Total Score: {TotalScore:F1}
Average Time per Question: {AverageTime:F1} seconds
Fastest Question: {FastestTime:F1} seconds
Slowest Question: {SlowestTime:F1} seconds

Provide an entertaining analysis in your assigned personality style. Keep it to 2-3 sentences maximum unless the format requires otherwise (like limerick or haiku). Be encouraging while pointing out both strengths and areas for improvement.";
    }

    private string GetFallbackResponse(string personality)
    {
        return personality.ToLowerInvariant() switch
        {
            "comedyroast" => "Well, well, well... looks like my AI brain had a math error while analyzing your math errors. How's that for irony?",
            "pirate" => "Arrr, me systems be havin' troubles, matey! But I'm sure ye did fine with yer numbers!",
            "limerick" => "A player once did math so neat,\nBut my analysis can't compete,\nI'm broken today,\nIn a technical way,\nBut your effort was still quite a feat!",
            "sportsbroadcaster" => "Ladies and gentlemen, we're experiencing some technical difficulties in the booth, but what a performance we witnessed today!",
            "haiku" => "My circuits failed hard\nBut your math skills shine so bright\nKeep calculating",
            "australian" => "G'day mate! My systems are having a bit of a wobble, but I reckon you did bonzer with those numbers!",
            "yourmother" => "Oh honey, mommy's computer is acting up again, but I'm so proud of you for trying your best at math!",
            _ => "I apologize, but I'm having technical difficulties analyzing your results. Great job on completing the game!"
        };
    }
}
