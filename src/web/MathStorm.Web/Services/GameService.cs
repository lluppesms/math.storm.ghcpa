using MathStorm.Common.Models;
using MathStorm.Common.DTOs;
using MathStorm.Common.Services;

namespace MathStorm.Web.Services;

public class GameService : IGameService
{
    private readonly IRemoteFunctionsService _functionService;
    private readonly Dictionary<string, GameSession> _activeSessions = new();

    public GameService(IRemoteFunctionsService functionService)
    {
        _functionService = functionService;
    }

    public GameSession CreateNewGame(Difficulty difficulty = Difficulty.Expert)
    {
        // For now, we'll create the game session locally and get questions from the function
        // In the future, we could modify this to be fully stateless
        var gameSession = new GameSession { Difficulty = difficulty };

        // We'll populate questions when the game starts
        return gameSession;
    }

    public async Task<GameSession> CreateNewGameAsync(Difficulty difficulty = Difficulty.Expert)
    {
        try
        {
            var gameResponse = await _functionService.GetGameAsync(difficulty);
            if (gameResponse != null)
            {
                var gameSession = new GameSession
                {
                    Difficulty = difficulty,
                    Questions = gameResponse.Questions.Select(q => new MathQuestion
                    {
                        Id = q.Id,
                        Number1 = q.Number1,
                        Number2 = q.Number2,
                        Operation = Enum.Parse<MathOperation>(q.Operation),
                        CorrectAnswer = q.CorrectAnswer
                    }).ToList()
                };

                // Store the game ID for later submission
                _activeSessions[gameResponse.GameId] = gameSession;

                return gameSession;
            }
        }
        catch (Exception)
        {
            // Fall back to local game generation when function service is unavailable
        }

        // Fallback: Create game locally for testing
        return CreateLocalGame(difficulty);
    }

    private GameSession CreateLocalGame(Difficulty difficulty)
    {
        var random = new Random();
        var questions = new List<MathQuestion>();
        
        var questionCount = difficulty switch
        {
            Difficulty.Beginner => 5,
            Difficulty.Novice => 5,
            Difficulty.Intermediate => 10,
            Difficulty.Expert => 10,
            _ => 5
        };

        var maxNumber = difficulty switch
        {
            Difficulty.Beginner => 99,
            Difficulty.Novice => 99,
            Difficulty.Intermediate => 999,
            Difficulty.Expert => 9999,
            _ => 99
        };

        var operations = difficulty switch
        {
            Difficulty.Beginner => new[] { MathOperation.Addition, MathOperation.Subtraction },
            _ => new[] { MathOperation.Addition, MathOperation.Subtraction, MathOperation.Multiplication, MathOperation.Division }
        };

        for (int i = 0; i < questionCount; i++)
        {
            var operation = operations[random.Next(operations.Length)];
            var number1 = random.Next(1, maxNumber);
            var number2 = random.Next(1, maxNumber);

            // Ensure division results in reasonable numbers
            if (operation == MathOperation.Division)
            {
                var product = number1 * number2;
                number1 = product;
                // number2 stays the same, so number1 / number2 = original number1
            }

            var correctAnswer = operation switch
            {
                MathOperation.Addition => number1 + number2,
                MathOperation.Subtraction => number1 - number2,
                MathOperation.Multiplication => number1 * number2,
                MathOperation.Division => (double)number1 / number2,
                _ => 0
            };

            questions.Add(new MathQuestion
            {
                Id = i + 1,
                Number1 = number1,
                Number2 = number2,
                Operation = operation,
                CorrectAnswer = correctAnswer
            });
        }

        return new GameSession
        {
            Difficulty = difficulty,
            Questions = questions
        };
    }

    public void StartQuestion(GameSession gameSession)
    {
        gameSession.QuestionStartTime = DateTime.Now;
        gameSession.IsGameStarted = true;
    }

    public void SubmitAnswer(GameSession gameSession, double userAnswer)
    {
        var currentQuestion = gameSession.CurrentQuestion;
        if (currentQuestion == null || gameSession.QuestionStartTime == null)
            return;

        // Calculate time taken
        var timeElapsed = DateTime.Now - gameSession.QuestionStartTime.Value;
        currentQuestion.TimeInSeconds = Math.Round(timeElapsed.TotalSeconds, 1);

        // Store user answer
        currentQuestion.UserAnswer = userAnswer;

        // Calculate percentage difference
        var correctAnswer = currentQuestion.CorrectAnswer;
        var difference = Math.Abs(correctAnswer - userAnswer);
        var percentageDifference = correctAnswer == 0 ?
            (userAnswer == 0 ? 0 : Math.Abs(userAnswer) * 100) :
            Math.Round((difference / Math.Abs(correctAnswer)) * 100, 1);

        currentQuestion.PercentageDifference = percentageDifference;

        // Calculate score using same formula as the original
        var timeFactor = 10.0;
        if (currentQuestion.TimeInSeconds <= 1) { timeFactor = 100.0; }

        currentQuestion.Score = Math.Round(
            (currentQuestion.PercentageDifference * currentQuestion.TimeInSeconds) +
            (currentQuestion.TimeInSeconds * timeFactor), 1);
    }

    public void NextQuestion(GameSession gameSession)
    {
        gameSession.CurrentQuestionIndex++;
        gameSession.QuestionStartTime = null;
    }

    public async Task<GameResultsResponseDto?> SubmitGameResultsAsync(GameSession gameSession, string username)
    {
        var gameId = _activeSessions.FirstOrDefault(kvp => kvp.Value == gameSession).Key;
        if (string.IsNullOrEmpty(gameId))
        {
            gameId = Guid.NewGuid().ToString(); // Generate new ID if not found
        }

        var request = new GameResultsRequestDto
        {
            GameId = gameId,
            Username = username,
            Difficulty = gameSession.Difficulty.ToString(),
            Questions = gameSession.Questions.Select(q => new QuestionResultDto
            {
                Id = q.Id,
                Number1 = q.Number1,
                Number2 = q.Number2,
                Operation = q.Operation.ToString(),
                CorrectAnswer = q.CorrectAnswer,
                UserAnswer = q.UserAnswer ?? 0,
                TimeInSeconds = q.TimeInSeconds,
                PercentageDifference = q.PercentageDifference,
                Score = q.Score
            }).ToList()
        };

        var result = await _functionService.SubmitGameResultsAsync(request);

        // Clean up the session
        if (!string.IsNullOrEmpty(gameId))
        {
            _activeSessions.Remove(gameId);
        }

        return result;
    }
}