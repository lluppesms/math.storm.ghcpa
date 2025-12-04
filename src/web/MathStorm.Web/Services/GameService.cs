using MathStorm.Common.Models;
using MathStorm.Common.DTOs;
using MathStorm.Common.Services;
using MathStorm.Services;

namespace MathStorm.Web.Services;

public class GameService : IGameService
{
    private readonly IMathStormService _mathStormService;
    private readonly Dictionary<string, GameSession> _activeSessions = new();

    public GameService(IMathStormService mathStormService)
    {
        _mathStormService = mathStormService;
    }

    public GameSession CreateNewGame(Difficulty difficulty = Difficulty.Expert)
    {
        // Create game session using direct service call (no HTTP)
        var gameResponse = _mathStormService.CreateGame(difficulty);
        
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

    public Task<GameSession> CreateNewGameAsync(Difficulty difficulty = Difficulty.Expert)
    {
        // CreateGame is synchronous, just wrap it
        return Task.FromResult(CreateNewGame(difficulty));
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

        var result = await _mathStormService.SubmitGameResultsAsync(request);

        // Clean up the session
        if (!string.IsNullOrEmpty(gameId))
        {
            _activeSessions.Remove(gameId);
        }

        return result;
    }
}