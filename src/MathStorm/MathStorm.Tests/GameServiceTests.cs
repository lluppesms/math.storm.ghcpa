using MathStorm.Web.Models;
using MathStorm.Web.Services;

namespace MathStorm.Tests;

[TestClass]
public class GameServiceTests
{
    private IGameService _gameService = default!;

    [TestInitialize]
    public void TestInitialize()
    {
        _gameService = new GameService();
    }

    [TestMethod]
    public void CreateNewGame_ShouldReturnGameSession()
    {
        // Arrange
        var difficulty = Difficulty.Expert;

        // Act
        var gameSession = _gameService.CreateNewGame(difficulty);

        // Assert
        Assert.IsNotNull(gameSession);
        Assert.AreEqual(difficulty, gameSession.Difficulty);
        Assert.AreEqual(10, gameSession.Questions.Count); // Expert has 10 questions
        Assert.IsFalse(gameSession.IsGameStarted);
        Assert.AreEqual(0, gameSession.CurrentQuestionIndex);
    }

    [TestMethod]
    public void CreateNewGame_BeginnerDifficulty_ShouldGenerateCorrectSettings()
    {
        // Arrange
        var difficulty = Difficulty.Beginner;

        // Act
        var gameSession = _gameService.CreateNewGame(difficulty);

        // Assert
        Assert.IsNotNull(gameSession);
        Assert.AreEqual(difficulty, gameSession.Difficulty);
        Assert.AreEqual(5, gameSession.Questions.Count); // Beginner has 5 questions
        
        // All questions should only be addition or subtraction
        foreach (var question in gameSession.Questions)
        {
            Assert.IsTrue(question.Operation == MathOperation.Addition || 
                         question.Operation == MathOperation.Subtraction);
        }
    }

    [TestMethod]
    public void CreateNewGame_BeginnerDifficulty_Addition_ShouldHaveOneDigitAndTwoDigitNumbers()
    {
        // Arrange
        var difficulty = Difficulty.Beginner;

        // Act - Generate multiple games to test randomness
        for (int i = 0; i < 10; i++)
        {
            var gameSession = _gameService.CreateNewGame(difficulty);
            
            // Check all addition questions
            var additionQuestions = gameSession.Questions.Where(q => q.Operation == MathOperation.Addition);
            
            foreach (var question in additionQuestions)
            {
                // One number should be single digit (1-9), the other should be two digits (10-99)
                var number1IsOneDigit = question.Number1 >= 1 && question.Number1 <= 9;
                var number1IsTwoDigit = question.Number1 >= 10 && question.Number1 <= 99;
                var number2IsOneDigit = question.Number2 >= 1 && question.Number2 <= 9;
                var number2IsTwoDigit = question.Number2 >= 10 && question.Number2 <= 99;
                
                // Assert that we have exactly one single digit and one two digit number
                Assert.IsTrue((number1IsOneDigit && number2IsTwoDigit) || (number1IsTwoDigit && number2IsOneDigit),
                    $"Addition question should have one single-digit and one two-digit number. Got {question.Number1} and {question.Number2}");
            }
        }
    }

    [TestMethod]
    public void CreateNewGame_BeginnerDifficulty_Subtraction_ShouldHaveTwoDigitFirstAndOneDigitSecond()
    {
        // Arrange
        var difficulty = Difficulty.Beginner;

        // Act - Generate multiple games to test randomness
        for (int i = 0; i < 10; i++)
        {
            var gameSession = _gameService.CreateNewGame(difficulty);
            
            // Check all subtraction questions
            var subtractionQuestions = gameSession.Questions.Where(q => q.Operation == MathOperation.Subtraction);
            
            foreach (var question in subtractionQuestions)
            {
                // First number should be two digits (10-99), second should be single digit (1-9)
                Assert.IsTrue(question.Number1 >= 10 && question.Number1 <= 99,
                    $"First number in subtraction should be two digits (10-99). Got {question.Number1}");
                Assert.IsTrue(question.Number2 >= 1 && question.Number2 <= 9,
                    $"Second number in subtraction should be single digit (1-9). Got {question.Number2}");
                
                // Ensure result is positive
                Assert.IsTrue(question.Number1 > question.Number2,
                    $"First number should be larger than second. Got {question.Number1} - {question.Number2}");
            }
        }
    }

    [TestMethod]
    public void CreateNewGame_NoviceDifficulty_ShouldGenerateCorrectSettings()
    {
        // Arrange
        var difficulty = Difficulty.Novice;

        // Act
        var gameSession = _gameService.CreateNewGame(difficulty);

        // Assert
        Assert.IsNotNull(gameSession);
        Assert.AreEqual(difficulty, gameSession.Difficulty);
        Assert.AreEqual(5, gameSession.Questions.Count); // Novice has 5 questions
        
        // Should have all operations
        var operations = gameSession.Questions.Select(q => q.Operation).Distinct().ToList();
        Assert.IsTrue(operations.Count >= 1); // At least one operation type
    }

    [TestMethod]
    public void StartQuestion_ShouldSetGameStartedAndQuestionStartTime()
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
    public void SubmitAnswer_CorrectAnswer_ShouldCalculateScore()
    {
        // Arrange
        var gameSession = _gameService.CreateNewGame(Difficulty.Expert);
        _gameService.StartQuestion(gameSession);
        System.Threading.Thread.Sleep(100); // Ensure some time passes
        var currentQuestion = gameSession.CurrentQuestion;
        Assert.IsNotNull(currentQuestion);

        // Act
        _gameService.SubmitAnswer(gameSession, currentQuestion.CorrectAnswer);

        // Assert
        Assert.AreEqual(currentQuestion.CorrectAnswer, currentQuestion.UserAnswer);
        Assert.AreEqual(0.0, currentQuestion.PercentageDifference); // Perfect answer
        Assert.IsTrue(currentQuestion.TimeInSeconds > 0);
        Assert.IsTrue(currentQuestion.Score > 0); // Score is calculated as (0 * time) + (time * timeFactor)
    }

    [TestMethod]
    public void SubmitAnswer_IncorrectAnswer_ShouldCalculatePercentageDifference()
    {
        // Arrange
        var gameSession = _gameService.CreateNewGame(Difficulty.Expert);
        _gameService.StartQuestion(gameSession);
        System.Threading.Thread.Sleep(100); // Ensure some time passes
        var currentQuestion = gameSession.CurrentQuestion;
        Assert.IsNotNull(currentQuestion);
        var wrongAnswer = currentQuestion.CorrectAnswer + 10; // Wrong by 10

        // Act
        _gameService.SubmitAnswer(gameSession, wrongAnswer);

        // Assert
        Assert.AreEqual(wrongAnswer, currentQuestion.UserAnswer);
        Assert.IsTrue(currentQuestion.PercentageDifference > 0);
        Assert.IsTrue(currentQuestion.Score > 0);
    }

    [TestMethod]
    public void NextQuestion_ShouldIncrementQuestionIndex()
    {
        // Arrange
        var gameSession = _gameService.CreateNewGame(Difficulty.Expert);
        var initialIndex = gameSession.CurrentQuestionIndex;

        // Act
        _gameService.NextQuestion(gameSession);

        // Assert
        Assert.AreEqual(initialIndex + 1, gameSession.CurrentQuestionIndex);
        Assert.IsNull(gameSession.QuestionStartTime);
    }
}