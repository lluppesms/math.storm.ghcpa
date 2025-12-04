using MathStorm.Common.Models;
using MathStorm.Common.Services;
using MathStorm.Core.Services;

namespace MathStorm.Tests;

[TestClass]
public class MockCosmosDbServiceTests
{
    private ICosmosDbService _cosmosDbService = default!;

    [TestInitialize]
    public void TestInitialize()
    {
        _cosmosDbService = new MockCosmosDbService();
    }

    [TestMethod]
    public async Task CreateUserAsync_ShouldCreateNewUser()
    {
        // Act
        var user = await _cosmosDbService.CreateUserAsync("TestUser");

        // Assert
        Assert.IsNotNull(user);
        Assert.AreEqual("TestUser", user.Username);
        Assert.IsNotNull(user.Id);
    }

    [TestMethod]
    public async Task GetUserByUsernameAsync_ExistingUser_ShouldReturnUser()
    {
        // Arrange
        var createdUser = await _cosmosDbService.CreateUserAsync("TestUser");

        // Act
        var retrievedUser = await _cosmosDbService.GetUserByUsernameAsync("TestUser");

        // Assert
        Assert.IsNotNull(retrievedUser);
        Assert.AreEqual(createdUser.Id, retrievedUser.Id);
        Assert.AreEqual("TestUser", retrievedUser.Username);
    }

    [TestMethod]
    public async Task GetUserByUsernameAsync_NonExistingUser_ShouldReturnNull()
    {
        // Act
        var user = await _cosmosDbService.GetUserByUsernameAsync("NonExistentUser");

        // Assert
        Assert.IsNull(user);
    }

    [TestMethod]
    public async Task CreateGameAsync_ShouldStoreGame()
    {
        // Arrange
        var game = new Game
        {
            Id = "test-game-1",
            UserId = "user-1",
            Username = "TestUser",
            Difficulty = "Expert",
            TotalScore = 150.5
        };

        // Act
        var createdGame = await _cosmosDbService.CreateGameAsync(game);

        // Assert
        Assert.IsNotNull(createdGame);
        Assert.AreEqual(game.Id, createdGame.Id);
        Assert.AreEqual(game.TotalScore, createdGame.TotalScore);
    }

    [TestMethod]
    public async Task GetLeaderboardAsync_ShouldReturnLeaderboard()
    {
        // Arrange - Add test data first
        var user = await _cosmosDbService.CreateUserAsync("TestUser");
        await _cosmosDbService.AddToLeaderboardAsync(user.Id, user.Username, "game1", "Expert", 100.0);
        
        // Act
        var leaderboard = await _cosmosDbService.GetLeaderboardAsync("Expert", 10);

        // Assert
        Assert.IsNotNull(leaderboard);
        Assert.IsTrue(leaderboard.Count > 0);
    }

    [TestMethod]
    public async Task GetGlobalLeaderboardAsync_ShouldReturnAllLevelsLeaderboard()
    {
        // Arrange - Add test data first
        var user = await _cosmosDbService.CreateUserAsync("TestUser");
        await _cosmosDbService.AddToLeaderboardAsync(user.Id, user.Username, "game1", "Expert", 100.0);
        await _cosmosDbService.AddToLeaderboardAsync(user.Id, user.Username, "game2", "Beginner", 50.0);
        
        // Act
        var globalLeaderboard = await _cosmosDbService.GetGlobalLeaderboardAsync(10);

        // Assert
        Assert.IsNotNull(globalLeaderboard);
        Assert.IsTrue(globalLeaderboard.Count > 0);
        
        // Should contain entries from multiple difficulty levels
        var difficulties = globalLeaderboard.Select(e => e.Difficulty).Distinct().ToList();
        Assert.IsTrue(difficulties.Count > 1, "Global leaderboard should contain entries from multiple difficulty levels");
        
        // Should be sorted by score (ascending - lower is better)
        for (int i = 1; i < globalLeaderboard.Count; i++)
        {
            Assert.IsTrue(globalLeaderboard[i].Score >= globalLeaderboard[i - 1].Score, 
                "Global leaderboard should be sorted by score in ascending order");
        }
        
        // Should have correct ranks
        for (int i = 0; i < globalLeaderboard.Count; i++)
        {
            Assert.AreEqual(i + 1, globalLeaderboard[i].Rank, "Rank should be sequential starting from 1");
        }
    }
}

[TestClass]
public class ScoringLogicTests
{
    private IGameService _gameService = default!;

    [TestInitialize]
    public void TestInitialize()
    {
        _gameService = new GameService();
    }

    [TestMethod]
    public void ScoreCalculation_PerfectAnswer_ShouldHaveMinimalScore()
    {
        // Arrange
        var gameSession = _gameService.CreateNewGame(Difficulty.Expert);
        _gameService.StartQuestion(gameSession);
        System.Threading.Thread.Sleep(100); // Ensure reasonable time
        var currentQuestion = gameSession.CurrentQuestion;
        Assert.IsNotNull(currentQuestion);

        // Act
        _gameService.SubmitAnswer(gameSession, currentQuestion.CorrectAnswer);

        // Assert
        Assert.AreEqual(0.0, currentQuestion.PercentageDifference);
        // Score should be just time * timeFactor (since percentage difference is 0)
        Assert.IsTrue(currentQuestion.Score > 0);
        Assert.IsTrue(currentQuestion.TimeInSeconds > 0);
    }

    [TestMethod]
    public void ScoreCalculation_VeryFastAnswer_ShouldHavePenalty()
    {
        // Arrange
        var gameSession = _gameService.CreateNewGame(Difficulty.Expert);
        _gameService.StartQuestion(gameSession);
        var currentQuestion = gameSession.CurrentQuestion;
        Assert.IsNotNull(currentQuestion);

        // Act immediately (should trigger penalty for very fast answer)
        _gameService.SubmitAnswer(gameSession, currentQuestion.CorrectAnswer);

        // Assert
        Assert.IsTrue(currentQuestion.TimeInSeconds <= 1);
        // Should have penalty time factor of 100 instead of 10
        var expectedScore = Math.Round(currentQuestion.TimeInSeconds * 100, 1);
        Assert.AreEqual(expectedScore, currentQuestion.Score);
    }

    [TestMethod]
    public void ScoreCalculation_WrongAnswer_ShouldIncludePercentagePenalty()
    {
        // Arrange
        var gameSession = _gameService.CreateNewGame(Difficulty.Expert);
        _gameService.StartQuestion(gameSession);
        System.Threading.Thread.Sleep(100); // Ensure reasonable time
        var currentQuestion = gameSession.CurrentQuestion;
        Assert.IsNotNull(currentQuestion);
        
        // Calculate a wrong answer that's 50% off
        var wrongAnswer = currentQuestion.CorrectAnswer * 1.5;

        // Act
        _gameService.SubmitAnswer(gameSession, wrongAnswer);

        // Assert
        Assert.IsTrue(currentQuestion.PercentageDifference > 0);
        Assert.IsTrue(currentQuestion.Score > 0);
        // Score should include both percentage difference and time penalty
        Assert.IsTrue(currentQuestion.TimeInSeconds > 0);
    }
}