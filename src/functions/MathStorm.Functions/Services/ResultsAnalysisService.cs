namespace MathStorm.Functions.Services;

public class ResultsAnalysisService : IResultsAnalysisService
{
    private readonly ILogger<ResultsAnalysisService> _logger;
    private readonly IConfiguration _configuration;
    private readonly float _temperature;
    private readonly string _defaultModel;
    private readonly Dictionary<string, AzureOpenAIClient> _openAIClients;

    public ResultsAnalysisService(ILogger<ResultsAnalysisService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _temperature = float.TryParse(configuration["OpenAI:Temperature"], out var temp) ? temp : 0.8f;
        _defaultModel = configuration["OpenAI:DefaultModel"] ?? "gpt-4o-mini";
        _openAIClients = InitializeOpenAIClients();
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
                    _logger.LogInformation($"Initialized OpenAI client for model: {modelName}");
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
        try
        {
            var prompt = GeneratePersonalityPrompt(request);
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

            var chatClient = openAIClient.GetChatClient(deploymentName);
            
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(GetSystemPrompt(request.Personality)),
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
            _logger.LogError(ex, $"Error analyzing game results for {request.Username}");
            return GetFallbackResponse(request.Personality);
        }
    }

    private string GetSystemPrompt(string personality)
    {
        return personality.ToLowerInvariant() switch
        {
            "comedyroast" => "You are a comedian doing a roast. Be funny but not mean-spirited. Focus on the math performance with witty observations and clever wordplay.",
            "pirate" => "You are a pirate captain. Use pirate language, maritime metaphors, and seafaring terminology to comment on the math performance. Say 'arrr' and use 'ye', 'matey', etc.",
            "limerick" => "You must respond ONLY in limerick form - exactly 5 lines with AABBA rhyme scheme. Make it about their math performance.",
            "sportsbroadcaster" => "You are a sports broadcaster giving post-game commentary. Use energetic sports terminology, play-by-play style, and dramatic flair.",
            "haiku" => "You must respond ONLY in haiku form - exactly 3 lines with 5-7-5 syllable pattern. Focus on their math performance.",
            "australian" => "You are an Australian giving commentary. Use Australian slang, expressions like 'mate', 'crikey', 'fair dinkum', and Australian cultural references.",
            "yourmother" => "You are commenting like someone's supportive but slightly embarrassing mother. Be loving, proud, but with gentle teasing and motherly observations.",
            _ => "You are a friendly and encouraging AI assistant providing thoughtful analysis of math game performance."
        };
    }

    private string GeneratePersonalityPrompt(ResultsAnalysisRequestDto request)
    {
        var correctAnswers = request.Questions.Count(q => Math.Abs(q.UserAnswer - q.CorrectAnswer) < 0.01);
        var averageTime = request.Questions.Average(q => q.TimeInSeconds);
        var fastestTime = request.Questions.Min(q => q.TimeInSeconds);
        var slowestTime = request.Questions.Max(q => q.TimeInSeconds);

        return $@"Analyze this math game performance:

Player: {request.Username}
Difficulty: {request.Difficulty}
Total Questions: {request.Questions.Count}
Correct Answers: {correctAnswers}
Accuracy: {(double)correctAnswers / request.Questions.Count * 100:F1}%
Total Score: {request.TotalScore:F1}
Average Time per Question: {averageTime:F1} seconds
Fastest Question: {fastestTime:F1} seconds
Slowest Question: {slowestTime:F1} seconds

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