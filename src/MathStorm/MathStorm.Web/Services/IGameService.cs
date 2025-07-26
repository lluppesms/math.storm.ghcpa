using MathStorm.Web.Models;

namespace MathStorm.Web.Services;

public interface IGameService
{
    GameSession CreateNewGame(Difficulty difficulty = Difficulty.Expert);
    void StartQuestion(GameSession gameSession);
    void SubmitAnswer(GameSession gameSession, double userAnswer);
    void NextQuestion(GameSession gameSession);
}