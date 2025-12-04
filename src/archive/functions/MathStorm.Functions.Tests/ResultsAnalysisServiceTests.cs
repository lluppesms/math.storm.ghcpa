namespace MathStorm.Functions.Tests;

[TestClass]
public class ResultsAnalysisServiceTests
{
    [TestMethod]
    public async Task MockResultsAnalysisService_ShouldReturnAnalysis_ForValidRequest()
    {
        // Arrange
        var logger = Mock.Of<ILogger<MockResultsAnalysisService>>();
        var service = new MockResultsAnalysisService(logger);

        var request = new ResultsAnalysisRequestDto
        {
            GameId = "test-game-123",
            Username = "TestPlayer",
            Difficulty = "Expert",
            TotalScore = 85.5,
            Personality = "default",
            Questions = new List<QuestionResultDto>
            {
                new() { Id = 1, Number1 = 12, Number2 = 8, Operation = "Addition", CorrectAnswer = 20, UserAnswer = 20, TimeInSeconds = 3.2, Score = 10 },
                new() { Id = 2, Number1 = 15, Number2 = 7, Operation = "Subtraction", CorrectAnswer = 8, UserAnswer = 9, TimeInSeconds = 4.1, Score = 8 }
            }
        };

        // Act
        var result = await service.AnalyzeGameResultsAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Length > 0);
        Assert.IsTrue(result.Contains("TestPlayer"));
    }

    [TestMethod]
    public async Task MockResultsAnalysisService_ShouldReturnPirateAnalysis_ForPiratePersonality()
    {
        // Arrange
        var logger = Mock.Of<ILogger<MockResultsAnalysisService>>();
        var service = new MockResultsAnalysisService(logger);

        var request = new ResultsAnalysisRequestDto
        {
            GameId = "test-game-123",
            Username = "TestPlayer",
            Difficulty = "Expert",
            TotalScore = 85.5,
            Personality = "pirate",
            Questions = new List<QuestionResultDto>
            {
                new() { Id = 1, Number1 = 12, Number2 = 8, Operation = "Addition", CorrectAnswer = 20, UserAnswer = 20, TimeInSeconds = 3.2, Score = 10 }
            }
        };

        // Act
        var result = await service.AnalyzeGameResultsAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("Arrr"));
        Assert.IsTrue(result.Contains("matey") || result.Contains("ye"));
    }

    [TestMethod]
    public async Task MockResultsAnalysisService_ShouldReturnLimerickAnalysis_ForLimerickPersonality()
    {
        // Arrange
        var logger = Mock.Of<ILogger<MockResultsAnalysisService>>();
        var service = new MockResultsAnalysisService(logger);

        var request = new ResultsAnalysisRequestDto
        {
            GameId = "test-game-123",
            Username = "TestPlayer",
            Difficulty = "Expert",
            TotalScore = 85.5,
            Personality = "limerick",
            Questions = new List<QuestionResultDto>
            {
                new() { Id = 1, Number1 = 12, Number2 = 8, Operation = "Addition", CorrectAnswer = 20, UserAnswer = 20, TimeInSeconds = 3.2, Score = 10 }
            }
        };

        // Act
        var result = await service.AnalyzeGameResultsAsync(request);

        // Assert
        Assert.IsNotNull(result);
        // Limerick should have 5 lines
        var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.AreEqual(5, lines.Length);
    }

    [TestMethod]
    public async Task MockResultsAnalysisService_ShouldCalculateAccuracyCorrectly()
    {
        // Arrange
        var logger = Mock.Of<ILogger<MockResultsAnalysisService>>();
        var service = new MockResultsAnalysisService(logger);

        var request = new ResultsAnalysisRequestDto
        {
            GameId = "test-game-123",
            Username = "TestPlayer",
            Difficulty = "Expert",
            TotalScore = 85.5,
            Personality = "default",
            Questions = new List<QuestionResultDto>
            {
                new() { Id = 1, Number1 = 12, Number2 = 8, Operation = "Addition", CorrectAnswer = 20, UserAnswer = 20, TimeInSeconds = 3.2, Score = 10 },
                new() { Id = 2, Number1 = 15, Number2 = 7, Operation = "Subtraction", CorrectAnswer = 8, UserAnswer = 9, TimeInSeconds = 4.1, Score = 8 },
                new() { Id = 3, Number1 = 10, Number2 = 5, Operation = "Multiplication", CorrectAnswer = 50, UserAnswer = 50, TimeInSeconds = 2.8, Score = 12 },
                new() { Id = 4, Number1 = 20, Number2 = 4, Operation = "Division", CorrectAnswer = 5, UserAnswer = 6, TimeInSeconds = 5.2, Score = 6 }
            }
        };

        // Act
        var result = await service.AnalyzeGameResultsAsync(request);

        // Assert
        Assert.IsNotNull(result);
        // Should have 50% accuracy (2 out of 4 correct)
        Assert.IsTrue(result.Contains("50.0%"));
    }
}