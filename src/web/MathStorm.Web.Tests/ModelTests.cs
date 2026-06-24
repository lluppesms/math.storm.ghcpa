using MathStorm.Core.Models;

namespace MathStorm.Tests;

[TestClass]
public class MathQuestionTests
{
    [TestMethod]
    public void QuestionText_ShouldFormatCorrectly()
    {
        var question = new MathQuestion
        {
            Number1 = 15,
            Number2 = 7,
            Operation = MathOperation.Addition
        };

        Assert.AreEqual("15 + 7", question.QuestionText);
    }

    [TestMethod]
    public void QuestionText_ShouldUsePromptTextWhenProvided()
    {
        var question = new MathQuestion
        {
            Number1 = 15,
            Number2 = 7,
            Operation = MathOperation.Addition,
            PromptText = "Maya places 15 stickers in her album and then adds 7 more. How many stickers does she have now?"
        };

        Assert.AreEqual(question.PromptText, question.QuestionText);
        Assert.AreEqual("15 + 7", question.ExpressionText);
    }

    [TestMethod]
    public void GetOperationSymbol_ShouldReturnCorrectSymbols()
    {
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
        var settings = DifficultySettings.GetSettings(Difficulty.Beginner);

        Assert.AreEqual(5, settings.QuestionCount);
        Assert.AreEqual(2, settings.MaxDigits);
        Assert.AreEqual(2, settings.AllowedOperations.Length);
        Assert.IsTrue(settings.AllowedOperations.Contains(MathOperation.Addition));
        Assert.IsTrue(settings.AllowedOperations.Contains(MathOperation.Subtraction));
    }

    [TestMethod]
    public void GetSettings_Novice_ShouldReturnCorrectSettings()
    {
        var settings = DifficultySettings.GetSettings(Difficulty.Novice);

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
        var settings = DifficultySettings.GetSettings(Difficulty.Intermediate);

        Assert.AreEqual(10, settings.QuestionCount);
        Assert.AreEqual(3, settings.MaxDigits);
        Assert.AreEqual(4, settings.AllowedOperations.Length);
    }

    [TestMethod]
    public void GetSettings_Expert_ShouldReturnCorrectSettings()
    {
        var settings = DifficultySettings.GetSettings(Difficulty.Expert);

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
        var gameSession = new GameSession();

        Assert.IsFalse(gameSession.IsGameStarted);
        Assert.AreEqual(0, gameSession.CurrentQuestionIndex);
        Assert.IsNotNull(gameSession.Questions);
        Assert.AreEqual(0, gameSession.Questions.Count);
        Assert.IsNull(gameSession.QuestionStartTime);
        Assert.AreEqual(GameMode.Classic, gameSession.GameMode);
    }

    [TestMethod]
    public void CurrentQuestion_ShouldReturnCorrectQuestion()
    {
        var gameSession = new GameSession();
        var question1 = new MathQuestion { Id = 1, Number1 = 5, Number2 = 3, Operation = MathOperation.Addition };
        var question2 = new MathQuestion { Id = 2, Number1 = 8, Number2 = 2, Operation = MathOperation.Subtraction };
        gameSession.Questions.Add(question1);
        gameSession.Questions.Add(question2);

        gameSession.CurrentQuestionIndex = 0;
        Assert.AreEqual(question1, gameSession.CurrentQuestion);

        gameSession.CurrentQuestionIndex = 1;
        Assert.AreEqual(question2, gameSession.CurrentQuestion);
    }

    [TestMethod]
    public void CurrentQuestion_InvalidIndex_ShouldReturnNull()
    {
        var gameSession = new GameSession();
        gameSession.CurrentQuestionIndex = 5;

        var currentQuestion = gameSession.CurrentQuestion;

        Assert.IsNull(currentQuestion);
    }

    [TestMethod]
    public void TotalScore_ShouldCalculateCorrectly()
    {
        var gameSession = new GameSession();
        var question1 = new MathQuestion { Score = 10.5 };
        var question2 = new MathQuestion { Score = 15.3 };
        var question3 = new MathQuestion { Score = 8.7 };
        gameSession.Questions.Add(question1);
        gameSession.Questions.Add(question2);
        gameSession.Questions.Add(question3);

        var totalScore = gameSession.TotalScore;

        Assert.AreEqual(34.5, totalScore, 0.1);
    }

    [TestMethod]
    public void IsGameComplete_ShouldReturnCorrectStatus()
    {
        var gameSession = new GameSession();
        gameSession.Questions.Add(new MathQuestion { Id = 1 });
        gameSession.Questions.Add(new MathQuestion { Id = 2 });

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
        var gameSession = new GameSession();
        gameSession.Questions.Add(new MathQuestion { Id = 1 });
        gameSession.Questions.Add(new MathQuestion { Id = 2 });

        gameSession.IsGameStarted = true;
        gameSession.CurrentQuestionIndex = 1;
        gameSession.QuestionStartTime = DateTime.Now;

        var resetGameSession = new GameSession();
        resetGameSession.Questions.Add(new MathQuestion { Id = 1 });
        resetGameSession.Questions.Add(new MathQuestion { Id = 2 });

        Assert.IsFalse(resetGameSession.IsGameStarted);
        Assert.AreEqual(0, resetGameSession.CurrentQuestionIndex);
        Assert.IsNull(resetGameSession.QuestionStartTime);
        Assert.IsFalse(resetGameSession.IsGameComplete);
    }
}
