using Microsoft.Extensions.Logging;
using MathStorm.Core.Services;
using MathStorm.Common.Models;
using MathStorm.Common.Services;

namespace MathStorm.Functions.Tests;

[TestClass]
public class SharedServicesTests
{
    private IGameService _gameService = default!;
    private ICosmosDbService _cosmosDbService = default!;

    [TestInitialize]
    public void TestInitialize()
    {
        _gameService = new GameService();
        _cosmosDbService = new MockCosmosDbService();
    }

    [TestMethod]
    public void GameService_CreateNewGame_ShouldGenerateQuestions()
    {
        // Act
        var gameSession = _gameService.CreateNewGame(Difficulty.Expert);

        // Assert
        Assert.IsNotNull(gameSession);
        Assert.AreEqual(10, gameSession.Questions.Count); // Expert difficulty has 10 questions
        Assert.AreEqual(Difficulty.Expert, gameSession.Difficulty);
        Assert.IsFalse(gameSession.IsGameStarted);
        Assert.IsFalse(gameSession.IsGameComplete);
    }

    [TestMethod]
    public void GameService_CreateNewGame_BeginnerDifficulty_ShouldHave5Questions()
    {
        // Act
        var gameSession = _gameService.CreateNewGame(Difficulty.Beginner);

        // Assert
        Assert.IsNotNull(gameSession);
        Assert.AreEqual(5, gameSession.Questions.Count); // Beginner difficulty has 5 questions
        Assert.AreEqual(Difficulty.Beginner, gameSession.Difficulty);
    }

    [TestMethod]
    public void GameService_StartQuestion_ShouldSetStartTime()
    {
        // Arrange
        var gameSession = _gameService.CreateNewGame(Difficulty.Expert);

        // Act
        _gameService.StartQuestion(gameSession);

        // Assert
        Assert.IsTrue(gameSession.IsGameStarted);
        Assert.IsNotNull(gameSession.QuestionStartTime);
    }

    [TestMethod]
    public void GameService_SubmitAnswer_ShouldCalculateScore()
    {
        // Arrange
        var gameSession = _gameService.CreateNewGame(Difficulty.Expert);
        _gameService.StartQuestion(gameSession);
        Thread.Sleep(100); // Small delay to have measurable time

        // Act
        _gameService.SubmitAnswer(gameSession, gameSession.CurrentQuestion!.CorrectAnswer);

        // Assert
        var question = gameSession.CurrentQuestion!;
        Assert.IsNotNull(question.UserAnswer);
        Assert.AreEqual(gameSession.CurrentQuestion.CorrectAnswer, question.UserAnswer.Value);
        Assert.IsTrue(question.TimeInSeconds > 0);
        Assert.AreEqual(0, question.PercentageDifference); // Exact answer should have 0% difference
        Assert.IsTrue(question.Score > 0);
    }

    [TestMethod]
    public async Task CosmosDbService_CreateUser_ShouldCreateNewUser()
    {
        // Act
        var user = await _cosmosDbService.CreateUserAsync("TestUser");

        // Assert
        Assert.IsNotNull(user);
        Assert.AreEqual("TestUser", user.Username);
        Assert.AreEqual(0, user.GamesPlayed);
        Assert.AreEqual(0, user.TotalScore);
        Assert.AreEqual(0, user.BestScore);
    }

    [TestMethod]
    public async Task CosmosDbService_AddToLeaderboard_ShouldAddEntry()
    {
        // Arrange
        var user = await _cosmosDbService.CreateUserAsync("TestUser");

        // Act
        var entry = await _cosmosDbService.AddToLeaderboardAsync(
            user.Id, user.Username, "game1", "Expert", 150.5);

        // Assert
        Assert.IsNotNull(entry);
        Assert.AreEqual(user.Username, entry.Username);
        Assert.AreEqual("Expert", entry.Difficulty);
        Assert.AreEqual(150.5, entry.Score);
    }
}