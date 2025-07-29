using MathStorm.Shared.Models;
using MathStorm.Shared.DTOs;
using MathStorm.Shared.Services;

namespace MathStorm.Web.Services;

public class FunctionBasedGameService : IGameService
{
    private readonly IMathStormFunctionService _functionService;
    private readonly Dictionary<string, GameSession> _activeSessions = new();

    public FunctionBasedGameService(IMathStormFunctionService functionService)
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
        var gameResponse = await _functionService.GetGameAsync(difficulty);
        if (gameResponse == null)
        {
            // Fallback to local generation if function is unavailable
            var localGameService = new GameService();
            return localGameService.CreateNewGame(difficulty);
        }

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