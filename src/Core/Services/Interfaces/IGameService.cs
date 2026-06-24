using MathStorm.Core.Models;

namespace MathStorm.Core.Services;

public interface IGameService
{
    GameSession CreateNewGame(Difficulty difficulty = Difficulty.Expert, GameMode gameMode = GameMode.Classic);
    Task<GameSession> CreateNewGameAsync(Difficulty difficulty = Difficulty.Expert, GameMode gameMode = GameMode.Classic);
    void StartQuestion(GameSession gameSession);
    void SubmitAnswer(GameSession gameSession, double userAnswer);
    void NextQuestion(GameSession gameSession);
}
