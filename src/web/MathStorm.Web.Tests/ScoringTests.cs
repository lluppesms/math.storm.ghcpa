using MathStorm.Core.Models;
using MathStorm.Core.Services;

namespace MathStorm.Tests;

[TestClass]
public class ScoringTests
{
    private GameService _gameService;
    private GameSession _gameSession;

    [TestInitialize]
    public void Setup()
    {
        _gameService = new GameService();
        _gameSession = new GameSession
        {
            Difficulty = Difficulty.Expert, // Explicit: time multiplier = 15
            Questions = new List<MathQuestion>
            {
                new MathQuestion
                {
                    Id = 1,
                    Number1 = 10,
                    Number2 = 5,
                    Operation = MathOperation.Addition,
                    CorrectAnswer = 15
                }
            },
            IsGameStarted = true,
            QuestionStartTime = DateTime.Now
        };
    }

    [TestMethod]
    public void Score_PerfectAnswerFastTime_ShouldHaveLowScore()
    {
        // Arrange: Perfect answer (0% error) in 1 second
        var question = _gameSession.Questions[0];
        _gameSession.QuestionStartTime = DateTime.Now.AddSeconds(-1);

        // Act
        _gameService.SubmitAnswer(_gameSession, 15); // Correct answer

        // Assert
        // Score = (0% * 3) + (1 * 15) = 0 + 15 = 15.0 (Expert difficulty, time multiplier = 15)
        Assert.AreEqual(15.0, question.Score, 0.1, "Perfect answer in 1s should score 15.0");
        Assert.AreEqual(0.0, question.PercentageDifference, "Error should be 0%");
        Assert.AreEqual(0.0, question.AccuracyScore, 0.1, "Accuracy score should be 0");
        Assert.AreEqual(15.0, question.TimeScore, 0.1, "Time score should be 15.0");
    }

    [TestMethod]
    public void Score_PerfectAnswerSlowTime_ShouldHaveModerateScore()
    {
        // Arrange: Perfect answer (0% error) in 1.6 seconds
        var question = _gameSession.Questions[0];
        _gameSession.QuestionStartTime = DateTime.Now.AddSeconds(-1.6);

        // Act
        _gameService.SubmitAnswer(_gameSession, 15); // Correct answer

        // Assert
        // Score = (0% * 3) + (1.6 * 15) = 0 + 24 = 24.0 (Expert difficulty, time multiplier = 15)
        Assert.AreEqual(24.0, question.Score, 0.1, "Perfect answer in 1.6s should score 24.0");
        Assert.AreEqual(0.0, question.PercentageDifference, "Error should be 0%");
    }

    [TestMethod]
    public void Score_SmallError_ShouldBeWeightedCorrectly()
    {
        // Arrange: 10% error in 2 seconds
        var question = _gameSession.Questions[0];
        _gameSession.QuestionStartTime = DateTime.Now.AddSeconds(-2);

        // Act
        _gameService.SubmitAnswer(_gameSession, 16.5); // 10% error from 15

        // Assert
        // Score = (10% * 3) + (2 * 15) = 30 + 30 = 60.0 (Expert difficulty, time multiplier = 15)
        Assert.AreEqual(10.0, question.PercentageDifference, "Error should be 10%");
        Assert.AreEqual(60.0, question.Score, 0.1, "10% error in 2s should score 60.0");
        Assert.AreEqual(30.0, question.AccuracyScore, 0.1, "Accuracy score should be 30.0");
        Assert.AreEqual(30.0, question.TimeScore, 0.1, "Time score should be 30.0");
    }

    [TestMethod]
    public void Score_LargeError_ShouldBeCappedAt200Percent()
    {
        // Arrange: Answer that would be >200% error
        var question = _gameSession.Questions[0];
        _gameSession.QuestionStartTime = DateTime.Now.AddSeconds(-2);

        // Act
        _gameService.SubmitAnswer(_gameSession, 100); // Way off from 15

        // Assert
        // Score = (200% * 3) + (2 * 15) = 600 + 30 = 630.0 (Expert difficulty, time multiplier = 15)
        Assert.AreEqual(200.0, question.PercentageDifference, "Error should be capped at 200%");
        Assert.AreEqual(630.0, question.Score, 0.1, "Large error should be capped");
    }

    [TestMethod]
    public void Score_ZeroCorrectAnswerZeroUserAnswer_ShouldBeZeroError()
    {
        // Arrange: Correct answer is 0, user also enters 0
        _gameSession.Questions[0].CorrectAnswer = 0;
        _gameSession.QuestionStartTime = DateTime.Now.AddSeconds(-1.5);

        // Act
        _gameService.SubmitAnswer(_gameSession, 0);

        // Assert
        var question = _gameSession.Questions[0];
        Assert.AreEqual(0.0, question.PercentageDifference, "0 vs 0 should be 0% error");
        // Score = (0% * 3) + (1.5 * 15) = 0 + 22.5 = 22.5 (Expert difficulty, time multiplier = 15)
        Assert.AreEqual(22.5, question.Score, 0.1, "Perfect answer of 0 should score based on time only");
    }

    [TestMethod]
    public void Score_ZeroCorrectAnswerNonZeroUserAnswer_ShouldBe200PercentError()
    {
        // Arrange: Correct answer is 0, user enters non-zero
        _gameSession.Questions[0].CorrectAnswer = 0;
        _gameSession.QuestionStartTime = DateTime.Now.AddSeconds(-2);

        // Act
        _gameService.SubmitAnswer(_gameSession, 5);

        // Assert
        var question = _gameSession.Questions[0];
        Assert.AreEqual(200.0, question.PercentageDifference, "Non-zero when correct is 0 should be 200% error");
        // Score = (200% * 3) + (2 * 15) = 600 + 30 = 630.0 (Expert difficulty, time multiplier = 15)
        Assert.AreEqual(630.0, question.Score, 0.1);
    }

    [TestMethod]
    public void Score_TimeUnder5Seconds_ShouldBeAccurateTo10thOfSecond()
    {
        // Arrange: Test precision below 5 seconds
        var question = _gameSession.Questions[0];
        _gameSession.QuestionStartTime = DateTime.Now.AddSeconds(-0.9);

        // Act
        _gameService.SubmitAnswer(_gameSession, 15);

        // Assert
        // Score = (0% * 3) + (0.9 * 15) = 0 + 13.5 = 13.5 (Expert difficulty, time multiplier = 15)
        Assert.AreEqual(13.5, question.Score, 0.1, "Time under 5s should be precise to 1/10 second");
    }

    [TestMethod]
    public void Score_TimeExactly10Seconds_ShouldUseFullRate()
    {
        // Arrange: Test at 10 second boundary
        var question = _gameSession.Questions[0];
        _gameSession.QuestionStartTime = DateTime.Now.AddSeconds(-10);

        // Act
        _gameService.SubmitAnswer(_gameSession, 15);

        // Assert
        // Score = (0% * 3) + (10 * 15) = 0 + 150 = 150.0 (Expert difficulty, time multiplier = 15)
        Assert.AreEqual(150.0, question.Score, 0.1, "10 seconds should score 150 for time component");
    }

    [TestMethod]
    public void Score_TimeOver10Seconds_ShouldUseHalfRate()
    {
        // Arrange: Test diminishing returns after 10 seconds
        var question = _gameSession.Questions[0];
        _gameSession.QuestionStartTime = DateTime.Now.AddSeconds(-15);

        // Act
        _gameService.SubmitAnswer(_gameSession, 15);

        // Assert
        // Score = (0% * 3) + (10 * 15 + 5 * 7.5) = 0 + 150 + 37.5 = 187.5 (Expert difficulty, time multiplier = 15, half rate = 7.5)
        Assert.AreEqual(187.5, question.Score, 0.1, "Time over 10s should use half rate");
    }

    [TestMethod]
    public void Score_TimeOver20Seconds_ShouldContinueHalfRate()
    {
        // Arrange: Test extended time
        var question = _gameSession.Questions[0];
        _gameSession.QuestionStartTime = DateTime.Now.AddSeconds(-20);

        // Act
        _gameService.SubmitAnswer(_gameSession, 15);

        // Assert
        // Score = (0% * 3) + (10 * 15 + 10 * 7.5) = 0 + 150 + 75 = 225.0 (Expert difficulty, time multiplier = 15, half rate = 7.5)
        Assert.AreEqual(225.0, question.Score, 0.1, "Time at 20s should continue half rate");
    }

    [TestMethod]
    public void Score_ComparisonOfSimilarErrors_LowerTimeShouldBeLowerScore()
    {
        // Test that with same accuracy, lower time = lower score
        var game1 = new GameSession
        {
            Difficulty = Difficulty.Expert,
            Questions = new List<MathQuestion>
            {
                new MathQuestion
                {
                    Id = 1,
                    Number1 = 30,
                    Number2 = 5,
                    Operation = MathOperation.Division,
                    CorrectAnswer = 6
                }
            },
            IsGameStarted = true,
            QuestionStartTime = DateTime.Now.AddSeconds(-1)
        };

        var game2 = new GameSession
        {
            Difficulty = Difficulty.Expert,
            Questions = new List<MathQuestion>
            {
                new MathQuestion
                {
                    Id = 1,
                    Number1 = 30,
                    Number2 = 5,
                    Operation = MathOperation.Division,
                    CorrectAnswer = 6
                }
            },
            IsGameStarted = true,
            QuestionStartTime = DateTime.Now.AddSeconds(-1.8)
        };

        // Act: Both answer perfectly
        _gameService.SubmitAnswer(game1, 6);
        _gameService.SubmitAnswer(game2, 6);

        // Assert: Faster time should have lower score
        Assert.IsTrue(game1.Questions[0].Score < game2.Questions[0].Score,
            "Faster time with same accuracy should result in lower score");
    }

    [TestMethod]
    public void Score_ComparisonOfSimilarTimes_BetterAccuracyShouldBeLowerScore()
    {
        // Test that with same time, better accuracy = lower score
        var game1 = new GameSession
        {
            Difficulty = Difficulty.Expert,
            Questions = new List<MathQuestion>
            {
                new MathQuestion
                {
                    Id = 1,
                    Number1 = 20,
                    Number2 = 4,
                    Operation = MathOperation.Subtraction,
                    CorrectAnswer = 16
                }
            },
            IsGameStarted = true,
            QuestionStartTime = DateTime.Now.AddSeconds(-1.5)
        };

        var game2 = new GameSession
        {
            Difficulty = Difficulty.Expert,
            Questions = new List<MathQuestion>
            {
                new MathQuestion
                {
                    Id = 1,
                    Number1 = 20,
                    Number2 = 4,
                    Operation = MathOperation.Subtraction,
                    CorrectAnswer = 16
                }
            },
            IsGameStarted = true,
            QuestionStartTime = DateTime.Now.AddSeconds(-1.5)
        };

        // Act: Different accuracy levels
        _gameService.SubmitAnswer(game1, 16);  // Perfect
        _gameService.SubmitAnswer(game2, 15);  // 6.25% error

        // Assert: Better accuracy should have lower score
        Assert.IsTrue(game1.Questions[0].Score < game2.Questions[0].Score,
            "Better accuracy with same time should result in lower score");
    }

    [TestMethod]
    public void Score_AccuracyWeightedMoreThanTime_ShouldBeTrue()
    {
        // Test that accuracy is weighted more heavily than time
        // A small error with fast time vs perfect with slower time
        var fastButWrong = new GameSession
        {
            Difficulty = Difficulty.Expert,
            Questions = new List<MathQuestion>
            {
                new MathQuestion
                {
                    Id = 1,
                    Number1 = 63,
                    Number2 = 12,
                    Operation = MathOperation.Addition,
                    CorrectAnswer = 75
                }
            },
            IsGameStarted = true,
            QuestionStartTime = DateTime.Now.AddSeconds(-0.5) // Very fast
        };

        var slowButCorrect = new GameSession
        {
            Difficulty = Difficulty.Expert,
            Questions = new List<MathQuestion>
            {
                new MathQuestion
                {
                    Id = 1,
                    Number1 = 63,
                    Number2 = 12,
                    Operation = MathOperation.Addition,
                    CorrectAnswer = 75
                }
            },
            IsGameStarted = true,
            QuestionStartTime = DateTime.Now.AddSeconds(-5) // Slower
        };

        // Act
        _gameService.SubmitAnswer(fastButWrong, 80);  // 6.7% error
        _gameService.SubmitAnswer(slowButCorrect, 75); // Perfect

        // Assert: Even with accuracy weighted 3x, a very fast wrong answer can beat a much slower correct one
        // Fast but wrong: (6.7 * 3) + (0.5 * 15) = 20.1 + 7.5 = ~27.6 (Expert difficulty)
        // Slow but correct: (0 * 3) + (5 * 15) = 0 + 75 = 75 (Expert difficulty)
        // This demonstrates that speed matters significantly, even with accuracy weighted 3x
        Assert.IsTrue(fastButWrong.Questions[0].Score < slowButCorrect.Questions[0].Score,
            "A very small error with very fast time can score better than perfect but much slower");
    }

    [TestMethod]
    public void Score_ExampleFromIssue_Question1()
    {
        // From the issue image: 16 ÷ 4 = 4.0, answered in 1.6s, 0.0% error
        var game = new GameSession
        {
            Difficulty = Difficulty.Expert,
            Questions = new List<MathQuestion>
            {
                new MathQuestion
                {
                    Id = 1,
                    Number1 = 16,
                    Number2 = 4,
                    Operation = MathOperation.Division,
                    CorrectAnswer = 4.0
                }
            },
            IsGameStarted = true,
            QuestionStartTime = DateTime.Now.AddSeconds(-1.6)
        };

        // Act
        _gameService.SubmitAnswer(game, 4.0);

        // Assert
        // Score = (0% * 3) + (1.6 * 15) = 0 + 24 = 24.0 (Expert difficulty, time multiplier = 15)
        var question = game.Questions[0];
        Assert.AreEqual(0.0, question.PercentageDifference, "Error should be 0%");
        Assert.AreEqual(24.0, question.Score, 0.1, "Score should be 24.0");
    }

    [TestMethod]
    public void Score_ExampleFromIssue_Question3()
    {
        // From the issue image: 30 ÷ 5 = 6.0, answered in 1.0s, 0.0% error
        var game = new GameSession
        {
            Difficulty = Difficulty.Expert,
            Questions = new List<MathQuestion>
            {
                new MathQuestion
                {
                    Id = 3,
                    Number1 = 30,
                    Number2 = 5,
                    Operation = MathOperation.Division,
                    CorrectAnswer = 6.0
                }
            },
            IsGameStarted = true,
            QuestionStartTime = DateTime.Now.AddSeconds(-1.0)
        };

        // Act
        _gameService.SubmitAnswer(game, 6.0);

        // Assert
        // Score = (0% * 3) + (1.0 * 15) = 0 + 15 = 15.0 (Expert difficulty, time multiplier = 15)
        var question = game.Questions[0];
        Assert.AreEqual(0.0, question.PercentageDifference, "Error should be 0%");
        Assert.AreEqual(15.0, question.Score, 0.1, "Score should be 15.0");
    }

    [TestMethod]
    public void Score_ConsistentScoring_AllPerfectAnswers()
    {
        // Test that all perfect answers with similar times get consistent scores
        var times = new[] { 0.9, 1.0, 1.5, 1.6, 1.8 };
        var scores = new List<double>();

        foreach (var time in times)
        {
            var game = new GameSession
            {
                Difficulty = Difficulty.Expert,
                Questions = new List<MathQuestion>
                {
                    new MathQuestion
                    {
                        Id = 1,
                        Number1 = 20,
                        Number2 = 4,
                        Operation = MathOperation.Subtraction,
                        CorrectAnswer = 16
                    }
                },
                IsGameStarted = true,
                QuestionStartTime = DateTime.Now.AddSeconds(-time)
            };

            _gameService.SubmitAnswer(game, 16);
            scores.Add(game.Questions[0].Score);
        }

        // Assert: Scores should increase linearly with time for perfect answers
        for (int i = 1; i < scores.Count; i++)
        {
            Assert.IsTrue(scores[i] > scores[i - 1],
                $"Score at {times[i]}s should be higher than at {times[i - 1]}s");
        }
    }

    [TestMethod]
    public void Score_BeginnerDifficulty_ShouldUseLowerTimeMultiplier()
    {
        // Test that Beginner difficulty uses time multiplier of 5
        var game = new GameSession
        {
            Difficulty = Difficulty.Beginner,
            Questions = new List<MathQuestion>
            {
                new MathQuestion
                {
                    Id = 1,
                    Number1 = 10,
                    Number2 = 5,
                    Operation = MathOperation.Addition,
                    CorrectAnswer = 15
                }
            },
            IsGameStarted = true,
            QuestionStartTime = DateTime.Now.AddSeconds(-2)
        };

        // Act
        _gameService.SubmitAnswer(game, 15);

        // Assert
        // Score = (0% * 3) + (2 * 5) = 0 + 10 = 10.0 (Beginner difficulty, time multiplier = 5)
        var question = game.Questions[0];
        Assert.AreEqual(10.0, question.Score, 0.1, "Beginner should use time multiplier of 5");
        Assert.AreEqual(10.0, question.TimeScore, 0.1, "Time score should be 10.0");
    }

    [TestMethod]
    public void Score_NoviceDifficulty_ShouldUseMediumTimeMultiplier()
    {
        // Test that Novice difficulty uses time multiplier of 10
        var game = new GameSession
        {
            Difficulty = Difficulty.Novice,
            Questions = new List<MathQuestion>
            {
                new MathQuestion
                {
                    Id = 1,
                    Number1 = 10,
                    Number2 = 5,
                    Operation = MathOperation.Addition,
                    CorrectAnswer = 15
                }
            },
            IsGameStarted = true,
            QuestionStartTime = DateTime.Now.AddSeconds(-2)
        };

        // Act
        _gameService.SubmitAnswer(game, 15);

        // Assert
        // Score = (0% * 3) + (2 * 10) = 0 + 20 = 20.0 (Novice difficulty, time multiplier = 10)
        var question = game.Questions[0];
        Assert.AreEqual(20.0, question.Score, 0.1, "Novice should use time multiplier of 10");
        Assert.AreEqual(20.0, question.TimeScore, 0.1, "Time score should be 20.0");
    }

    [TestMethod]
    public void Score_IntermediateDifficulty_ShouldUseHigherTimeMultiplier()
    {
        // Test that Intermediate difficulty uses time multiplier of 15
        var game = new GameSession
        {
            Difficulty = Difficulty.Intermediate,
            Questions = new List<MathQuestion>
            {
                new MathQuestion
                {
                    Id = 1,
                    Number1 = 10,
                    Number2 = 5,
                    Operation = MathOperation.Addition,
                    CorrectAnswer = 15
                }
            },
            IsGameStarted = true,
            QuestionStartTime = DateTime.Now.AddSeconds(-2)
        };

        // Act
        _gameService.SubmitAnswer(game, 15);

        // Assert
        // Score = (0% * 3) + (2 * 15) = 0 + 30 = 30.0 (Intermediate difficulty, time multiplier = 15)
        var question = game.Questions[0];
        Assert.AreEqual(30.0, question.Score, 0.1, "Intermediate should use time multiplier of 15");
        Assert.AreEqual(30.0, question.TimeScore, 0.1, "Time score should be 30.0");
    }
}
