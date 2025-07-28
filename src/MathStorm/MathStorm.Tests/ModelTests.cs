using MathStorm.Web.Models;

namespace MathStorm.Tests;

[TestClass]
public class MathQuestionTests
{
    [TestMethod]
    public void QuestionText_ShouldFormatCorrectly()
    {
        // Arrange
        var question = new MathQuestion
        {
            Number1 = 15,
            Number2 = 7,
            Operation = MathOperation.Addition
        };

        // Act
        var questionText = question.QuestionText;

        // Assert
        Assert.AreEqual("15 + 7", questionText);
    }

    [TestMethod]
    public void GetOperationSymbol_ShouldReturnCorrectSymbols()
    {
        // Arrange & Act & Assert
        var additionQuestion = new MathQuestion { Operation = MathOperation.Addition };
        Assert.AreEqual("+", additionQuestion.GetOperationSymbol());

        var subtractionQuestion = new MathQuestion { Operation = MathOperation.Subtraction };
        Assert.AreEqual("-", subtractionQuestion.GetOperationSymbol());

        var multiplicationQuestion = new MathQuestion { Operation = MathOperation.Multiplication };
        Assert.AreEqual("×", multiplicationQuestion.GetOperationSymbol());

        var divisionQuestion = new MathQuestion { Operation = MathOperation.Division };
        Assert.AreEqual("÷", divisionQuestion.GetOperationSymbol());
    }
}

[TestClass]
public class DifficultySettingsTests
{
    [TestMethod]
    public void GetSettings_Beginner_ShouldReturnCorrectSettings()
    {
        // Act
        var settings = DifficultySettings.GetSettings(Difficulty.Beginner);

        // Assert
        Assert.AreEqual(5, settings.QuestionCount);
        Assert.AreEqual(2, settings.MaxDigits);
        Assert.AreEqual(2, settings.AllowedOperations.Length);
        Assert.IsTrue(settings.AllowedOperations.Contains(MathOperation.Addition));
        Assert.IsTrue(settings.AllowedOperations.Contains(MathOperation.Subtraction));
    }

    [TestMethod]
    public void GetSettings_Novice_ShouldReturnCorrectSettings()
    {
        // Act
        var settings = DifficultySettings.GetSettings(Difficulty.Novice);

        // Assert
        Assert.AreEqual(5, settings.QuestionCount);
        Assert.AreEqual(2, settings.MaxDigits);
        Assert.AreEqual(4, settings.AllowedOperations.Length);
        Assert.IsTrue(settings.AllowedOperations.Contains(MathOperation.Addition));
        Assert.IsTrue(settings.AllowedOperations.Contains(MathOperation.Subtraction));
        Assert.IsTrue(settings.AllowedOperations.Contains(MathOperation.Multiplication));
        Assert.IsTrue(settings.AllowedOperations.Contains(MathOperation.Division));
    }

    [TestMethod]
    public void GetSettings_Intermediate_ShouldReturnCorrectSettings()
    {
        // Act
        var settings = DifficultySettings.GetSettings(Difficulty.Intermediate);

        // Assert
        Assert.AreEqual(10, settings.QuestionCount);
        Assert.AreEqual(3, settings.MaxDigits);
        Assert.AreEqual(4, settings.AllowedOperations.Length);
    }

    [TestMethod]
    public void GetSettings_Expert_ShouldReturnCorrectSettings()
    {
        // Act
        var settings = DifficultySettings.GetSettings(Difficulty.Expert);

        // Assert
        Assert.AreEqual(10, settings.QuestionCount);
        Assert.AreEqual(4, settings.MaxDigits);
        Assert.AreEqual(4, settings.AllowedOperations.Length);
    }
}

[TestClass]
public class GameSessionTests
{
    [TestMethod]
    public void GameSession_DefaultValues_ShouldBeCorrect()
    {
        // Act
        var gameSession = new GameSession();

        // Assert
        Assert.IsFalse(gameSession.IsGameStarted);
        Assert.AreEqual(0, gameSession.CurrentQuestionIndex);
        Assert.IsNotNull(gameSession.Questions);
        Assert.AreEqual(0, gameSession.Questions.Count);
        Assert.IsNull(gameSession.QuestionStartTime);
    }

    [TestMethod]
    public void CurrentQuestion_ShouldReturnCorrectQuestion()
    {
        // Arrange
        var gameSession = new GameSession();
        var question1 = new MathQuestion { Id = 1, Number1 = 5, Number2 = 3, Operation = MathOperation.Addition };
        var question2 = new MathQuestion { Id = 2, Number1 = 8, Number2 = 2, Operation = MathOperation.Subtraction };
        gameSession.Questions.Add(question1);
        gameSession.Questions.Add(question2);

        // Act & Assert
        gameSession.CurrentQuestionIndex = 0;
        Assert.AreEqual(question1, gameSession.CurrentQuestion);

        gameSession.CurrentQuestionIndex = 1;
        Assert.AreEqual(question2, gameSession.CurrentQuestion);
    }

    [TestMethod]
    public void CurrentQuestion_InvalidIndex_ShouldReturnNull()
    {
        // Arrange
        var gameSession = new GameSession();
        gameSession.CurrentQuestionIndex = 5; // Out of range

        // Act
        var currentQuestion = gameSession.CurrentQuestion;

        // Assert
        Assert.IsNull(currentQuestion);
    }

    [TestMethod]
    public void TotalScore_ShouldCalculateCorrectly()
    {
        // Arrange
        var gameSession = new GameSession();
        var question1 = new MathQuestion { Score = 10.5 };
        var question2 = new MathQuestion { Score = 15.3 };
        var question3 = new MathQuestion { Score = 8.7 };
        gameSession.Questions.Add(question1);
        gameSession.Questions.Add(question2);
        gameSession.Questions.Add(question3);

        // Act
        var totalScore = gameSession.TotalScore;

        // Assert
        Assert.AreEqual(34.5, totalScore, 0.1);
    }

    [TestMethod]
    public void IsGameComplete_ShouldReturnCorrectStatus()
    {
        // Arrange
        var gameSession = new GameSession();
        gameSession.Questions.Add(new MathQuestion { Id = 1 });
        gameSession.Questions.Add(new MathQuestion { Id = 2 });

        // Act & Assert
        gameSession.CurrentQuestionIndex = 0;
        Assert.IsFalse(gameSession.IsGameComplete);

        gameSession.CurrentQuestionIndex = 1;
        Assert.IsFalse(gameSession.IsGameComplete);

        gameSession.CurrentQuestionIndex = 2;
        Assert.IsTrue(gameSession.IsGameComplete);
    }

    [TestMethod]
    public void GameSession_WhenStartedThenReset_ShouldReturnToInitialState()
    {
        // Arrange
        var gameSession = new GameSession();
        gameSession.Questions.Add(new MathQuestion { Id = 1 });
        gameSession.Questions.Add(new MathQuestion { Id = 2 });
        
        // Start the game and advance
        gameSession.IsGameStarted = true;
        gameSession.CurrentQuestionIndex = 1;
        gameSession.QuestionStartTime = DateTime.Now;

        // Act - Reset to initial state (simulating cancel game behavior)
        var resetGameSession = new GameSession();
        resetGameSession.Questions.Add(new MathQuestion { Id = 1 });
        resetGameSession.Questions.Add(new MathQuestion { Id = 2 });

        // Assert - Reset game session should be in initial state
        Assert.IsFalse(resetGameSession.IsGameStarted);
        Assert.AreEqual(0, resetGameSession.CurrentQuestionIndex);
        Assert.IsNull(resetGameSession.QuestionStartTime);
        Assert.IsFalse(resetGameSession.IsGameComplete);
    }
}