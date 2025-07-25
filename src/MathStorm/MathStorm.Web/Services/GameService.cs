using MathStorm.Web.Models;

namespace MathStorm.Web.Services;

public class GameService : IGameService
{
    private readonly Random _random = new();
    
    public GameSession CreateNewGame()
    {
        var gameSession = new GameSession();
        
        // Generate 10 random math questions
        for (int i = 0; i < 10; i++)
        {
            gameSession.Questions.Add(GenerateRandomQuestion(i + 1));
        }
        
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
            (userAnswer == 0 ? 0 : 100) : 
            Math.Round((difference / Math.Abs(correctAnswer)) * 100);
        
        currentQuestion.PercentageDifference = Math.Min(100, (int)percentageDifference);
        
        // Calculate score using new formula: (Percentage difference * Time) + (Time * Time_Factor)
        // where Time_Factor = 10
        const double timeFactor = 10.0;
        currentQuestion.Score = Math.Round(
            (currentQuestion.PercentageDifference * currentQuestion.TimeInSeconds) + 
            (currentQuestion.TimeInSeconds * timeFactor), 1);
    }
    
    public void NextQuestion(GameSession gameSession)
    {
        gameSession.CurrentQuestionIndex++;
        gameSession.QuestionStartTime = null;
    }
    
    private MathQuestion GenerateRandomQuestion(int id)
    {
        var operation = (MathOperation)_random.Next(0, 4);
        var question = new MathQuestion
        {
            Id = id,
            Operation = operation
        };
        
        switch (operation)
        {
            case MathOperation.Addition:
                question.Number1 = _random.Next(1, 10000);
                question.Number2 = _random.Next(1, 10000);
                question.CorrectAnswer = question.Number1 + question.Number2;
                break;
                
            case MathOperation.Subtraction:
                question.Number1 = _random.Next(1, 10000);
                question.Number2 = _random.Next(1, Math.Min(question.Number1, 10000));
                question.CorrectAnswer = question.Number1 - question.Number2;
                break;
                
            case MathOperation.Multiplication:
                question.Number1 = _random.Next(1, 100);
                question.Number2 = _random.Next(1, 100);
                question.CorrectAnswer = question.Number1 * question.Number2;
                break;
                
            case MathOperation.Division:
                question.Number2 = _random.Next(1, 100);
                var quotient = _random.Next(1, 100);
                question.Number1 = question.Number2 * quotient;
                question.CorrectAnswer = quotient;
                break;
        }
        
        return question;
    }
}