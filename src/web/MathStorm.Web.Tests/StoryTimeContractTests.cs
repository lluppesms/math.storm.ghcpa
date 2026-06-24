using MathStorm.Core;
using MathStorm.Core.Models;
using MathStorm.Core.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace MathStorm.Tests;

[TestClass]
public class StoryTimeContractTests
{
    private readonly GameService gameService = new();

    [DataTestMethod]
    [DataRow(Difficulty.Beginner, 5)]
    [DataRow(Difficulty.Novice, 5)]
    [DataRow(Difficulty.Intermediate, 10)]
    [DataRow(Difficulty.Expert, 10)]
    public void CreateNewGame_ShouldKeepExistingQuestionCounts(Difficulty difficulty, int expectedQuestionCount)
    {
        var gameSession = gameService.CreateNewGame(difficulty);

        Assert.AreEqual(difficulty, gameSession.Difficulty);
        Assert.AreEqual(expectedQuestionCount, gameSession.Questions.Count);
    }

    [DataTestMethod]
    [DataRow(Difficulty.Beginner)]
    [DataRow(Difficulty.Novice)]
    [DataRow(Difficulty.Intermediate)]
    [DataRow(Difficulty.Expert)]
    public void CreateNewGame_ShouldOnlyUseConfiguredOperations(Difficulty difficulty)
    {
        var settings = DifficultySettings.GetSettings(difficulty);
        var encounteredOperations = new HashSet<MathOperation>();

        foreach (var _ in Enumerable.Range(0, 100))
        {
            var gameSession = gameService.CreateNewGame(difficulty);

            foreach (var question in gameSession.Questions)
            {
                Assert.IsTrue(
                    settings.AllowedOperations.Contains(question.Operation),
                    $"{difficulty} generated unsupported operation {question.Operation}.");

                encounteredOperations.Add(question.Operation);
            }
        }

        CollectionAssert.AreEquivalent(
            settings.AllowedOperations,
            encounteredOperations.ToArray(),
            $"{difficulty} should keep its existing operation pool.");
    }

    [TestMethod]
    public void CreateNewGame_BeginnerQuestions_ShouldKeepCurrentOperandShapes()
    {
        var sawAddition = false;
        var sawSubtraction = false;

        foreach (var _ in Enumerable.Range(0, 100))
        {
            var gameSession = gameService.CreateNewGame(Difficulty.Beginner);

            foreach (var question in gameSession.Questions)
            {
                switch (question.Operation)
                {
                    case MathOperation.Addition:
                        sawAddition = true;
                        Assert.AreNotEqual(IsSingleDigit(question.Number1), IsSingleDigit(question.Number2), "Beginner addition should mix one single-digit and one two-digit operand.");
                        Assert.AreNotEqual(IsTwoDigit(question.Number1), IsTwoDigit(question.Number2), "Beginner addition should mix one single-digit and one two-digit operand.");
                        Assert.AreEqual(question.Number1 + question.Number2, question.CorrectAnswer);
                        break;

                    case MathOperation.Subtraction:
                        sawSubtraction = true;
                        Assert.IsTrue(IsTwoDigit(question.Number1), "Beginner subtraction minuend should stay two digits.");
                        Assert.IsTrue(IsSingleDigit(question.Number2), "Beginner subtraction subtrahend should stay single digit.");
                        Assert.IsTrue(question.Number1 >= question.Number2, "Beginner subtraction should not go negative.");
                        Assert.AreEqual(question.Number1 - question.Number2, question.CorrectAnswer);
                        break;

                    default:
                        Assert.Fail($"Unexpected Beginner operation {question.Operation}.");
                        break;
                }
            }
        }

        Assert.IsTrue(sawAddition, "Beginner coverage never exercised addition.");
        Assert.IsTrue(sawSubtraction, "Beginner coverage never exercised subtraction.");
    }

    [TestMethod]
    public void CreateNewGame_NoviceMultiplicationAndDivision_ShouldKeepCurrentConstraints()
    {
        var sawMultiplication = false;
        var sawDivision = false;

        foreach (var _ in Enumerable.Range(0, 100))
        {
            var gameSession = gameService.CreateNewGame(Difficulty.Novice);

            foreach (var question in gameSession.Questions)
            {
                switch (question.Operation)
                {
                    case MathOperation.Multiplication:
                        sawMultiplication = true;
                        Assert.IsTrue(IsSingleDigit(question.Number2), "Novice multiplication multiplier should stay single digit.");
                        Assert.IsTrue(question.Number1 > question.Number2, "Novice multiplication should keep the first operand larger.");
                        Assert.AreEqual(question.Number1 * question.Number2, question.CorrectAnswer);
                        break;

                    case MathOperation.Division:
                        sawDivision = true;
                        Assert.IsTrue(question.Number2 is >= 2 and <= 9, "Novice division divisor should stay between 2 and 9.");
                        Assert.IsTrue(question.Number1 > question.Number2, "Novice division dividend should stay larger than the divisor.");
                        Assert.AreEqual(0, question.Number1 % question.Number2, "Novice division should divide evenly.");
                        Assert.AreEqual(question.Number1 / question.Number2, question.CorrectAnswer);
                        break;
                }
            }
        }

        Assert.IsTrue(sawMultiplication, "Novice coverage never exercised multiplication.");
        Assert.IsTrue(sawDivision, "Novice coverage never exercised division.");
    }

    [DataTestMethod]
    [DataRow(Difficulty.Beginner)]
    [DataRow(Difficulty.Novice)]
    [DataRow(Difficulty.Intermediate)]
    [DataRow(Difficulty.Expert)]
    public void CreateNewGame_StoryTime_ShouldKeepDifficultyRulesAndAddWordProblems(Difficulty difficulty)
    {
        var classicSession = gameService.CreateNewGame(difficulty);
        var storySession = gameService.CreateNewGame(difficulty, GameMode.StoryTime);

        Assert.AreEqual(GameMode.StoryTime, storySession.GameMode);
        Assert.AreEqual(classicSession.Questions.Count, storySession.Questions.Count);

        foreach (var question in storySession.Questions)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(question.PromptText), "Story Time questions should include a prompt.");
            Assert.AreNotEqual(question.ExpressionText, question.QuestionText, "Story Time should present a word problem instead of the raw equation.");
        }
    }

    [TestMethod]
    public void StoryQuestionService_ShouldBeDeterministicForSameQuestion()
    {
        var service = new StoryQuestionService();
        var question = new MathQuestion
        {
            Id = 3,
            Number1 = 24,
            Number2 = 6,
            Operation = MathOperation.Division
        };

        var first = service.CreateStoryQuestion(question);
        var second = service.CreateStoryQuestion(question);

        Assert.AreEqual(first, second);
    }

    [TestMethod]
    public async Task CreateGame_StoryTimeDto_ShouldExposeModeAndPromptText()
    {
        var service = new MathStormService(
            NullLogger<MathStormService>.Instance,
            new GameService(),
            new MockCosmosDbService(),
            new MockResultsAnalysisService(NullLogger<MockResultsAnalysisService>.Instance));

        var response = await service.CreateGame(Difficulty.Beginner, GameMode.StoryTime);

        Assert.AreEqual(GameMode.StoryTime.ToString(), response.GameMode);
        Assert.IsTrue(response.Questions.All(q => !string.IsNullOrWhiteSpace(q.QuestionText)));
    }

    private static bool IsSingleDigit(int value) => value is >= 1 and <= 9;

    private static bool IsTwoDigit(int value) => value is >= 10 and <= 99;
}
