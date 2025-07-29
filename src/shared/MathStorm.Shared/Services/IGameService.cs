using MathStorm.Shared.Models;

namespace MathStorm.Shared.Services;

public interface IGameService
{
    GameSession CreateNewGame(Difficulty difficulty = Difficulty.Expert);
    Task<GameSession> CreateNewGameAsync(Difficulty difficulty = Difficulty.Expert);
    void StartQuestion(GameSession gameSession);
    void SubmitAnswer(GameSession gameSession, double userAnswer);
    void NextQuestion(GameSession gameSession);
}