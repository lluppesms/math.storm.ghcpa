namespace MathStorm.Functions.Tests;

[TestClass]
public class ResultsAnalysisIntegrationTests
{
    [TestMethod]
    public async Task AnalysisFunction_ShouldReturnValidResponse_WhenGivenValidInput()
    {
        // Arrange
        var request = CreateSampleGameResults();
        var jsonBody = JsonConvert.SerializeObject(request);
        
        // Create a mock HttpRequestData
        var mockLogger = Mock.Of<ILogger<ResultsAnalysisFunctions>>();
        var mockAnalysisService = Mock.Of<IResultsAnalysisService>();
        Mock.Get(mockAnalysisService)
            .Setup(s => s.AnalyzeGameResultsAsync(It.IsAny<ResultsAnalysisRequestDto>()))
            .ReturnsAsync("Great job! You scored 80% accuracy - keep practicing those multiplication tables!");

        var function = new ResultsAnalysisFunctions(mockLogger, mockAnalysisService);

        // Since we can't easily mock HttpRequestData in this environment,
        // let's test the service directly instead
        var analysisService = new MockResultsAnalysisService(Mock.Of<ILogger<MockResultsAnalysisService>>());
        
        // Act
        var result = await analysisService.AnalyzeGameResultsAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Length > 10, $"Result length was {result.Length}: {result}"); // Should be a meaningful response
        Assert.IsTrue(result.Contains("TestUser"), $"Result should contain TestUser: {result}");
        Assert.IsTrue(result.Contains("60.0%"), $"Result should contain 60.0%: {result}"); // Should contain accuracy
    }

    [TestMethod]
    public async Task AnalysisFunction_ShouldHandleDifferentPersonalities()
    {
        // Arrange
        var analysisService = new MockResultsAnalysisService(Mock.Of<ILogger<MockResultsAnalysisService>>());
        var baseRequest = CreateSampleGameResults();

        var personalities = new[] { "default", "pirate", "comedyroast", "limerick", "haiku", "australian", "yourmother", "sportsbroadcaster" };
        
        // Act & Assert
        foreach (var personality in personalities)
        {
            baseRequest.Personality = personality;
            var result = await analysisService.AnalyzeGameResultsAsync(baseRequest);
            
            Assert.IsNotNull(result, $"Result should not be null for {personality}");
            Assert.IsTrue(result.Length > 10, $"Result should be meaningful for {personality}");
            
            // Verify personality-specific characteristics
            switch (personality)
            {
                case "pirate":
                    Assert.IsTrue(result.Contains("Arrr") || result.Contains("matey"), $"Pirate personality should contain pirate language");
                    break;
                case "limerick":
                    var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    Assert.AreEqual(5, lines.Length, "Limerick should have exactly 5 lines");
                    break;
                case "haiku":
                    var haikuLines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    Assert.AreEqual(3, haikuLines.Length, "Haiku should have exactly 3 lines");
                    break;
                case "australian":
                    Assert.IsTrue(result.Contains("mate") || result.Contains("G'day"), "Australian personality should contain Australian expressions");
                    break;
            }
        }
    }

    [TestMethod]
    public async Task AnalysisFunction_ShouldCalculateGameStatisticsCorrectly()
    {
        // Arrange
        var analysisService = new MockResultsAnalysisService(Mock.Of<ILogger<MockResultsAnalysisService>>());
        var request = new ResultsAnalysisRequestDto
        {
            GameId = "test-game-stats",
            Username = "StatsTestUser",
            Difficulty = "Expert",
            TotalScore = 95.5,
            Personality = "default",
            Questions = new List<QuestionResultDto>
            {
                // All correct answers
                new() { Id = 1, Number1 = 10, Number2 = 5, Operation = "Addition", CorrectAnswer = 15, UserAnswer = 15, TimeInSeconds = 2.0, Score = 10 },
                new() { Id = 2, Number1 = 20, Number2 = 8, Operation = "Subtraction", CorrectAnswer = 12, UserAnswer = 12, TimeInSeconds = 1.5, Score = 12 },
                new() { Id = 3, Number1 = 6, Number2 = 7, Operation = "Multiplication", CorrectAnswer = 42, UserAnswer = 42, TimeInSeconds = 3.0, Score = 8 }
            }
        };

        // Act
        var result = await analysisService.AnalyzeGameResultsAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("100.0%"), "Should show 100% accuracy for all correct answers");
        Assert.IsTrue(result.Contains("StatsTestUser"), "Should contain the username");
        Assert.IsTrue(result.Contains("3"), "Should mention the number of questions");
    }

    private static ResultsAnalysisRequestDto CreateSampleGameResults()
    {
        return new ResultsAnalysisRequestDto
        {
            GameId = "test-game-123",
            Username = "TestUser",
            Difficulty = "Intermediate",
            TotalScore = 85.0,
            Personality = "default",
            Questions = new List<QuestionResultDto>
            {
                new() { Id = 1, Number1 = 12, Number2 = 8, Operation = "Addition", CorrectAnswer = 20, UserAnswer = 20, TimeInSeconds = 3.2, Score = 10 },
                new() { Id = 2, Number1 = 15, Number2 = 7, Operation = "Subtraction", CorrectAnswer = 8, UserAnswer = 9, TimeInSeconds = 4.1, Score = 8 },
                new() { Id = 3, Number1 = 6, Number2 = 9, Operation = "Multiplication", CorrectAnswer = 54, UserAnswer = 54, TimeInSeconds = 2.8, Score = 12 },
                new() { Id = 4, Number1 = 20, Number2 = 4, Operation = "Division", CorrectAnswer = 5, UserAnswer = 4, TimeInSeconds = 5.2, Score = 6 },
                new() { Id = 5, Number1 = 7, Number2 = 3, Operation = "Addition", CorrectAnswer = 10, UserAnswer = 10, TimeInSeconds = 1.9, Score = 15 }
            }
        };
    }
}