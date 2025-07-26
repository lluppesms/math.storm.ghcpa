using MathStorm.Web.Models;

namespace MathStorm.Web.Services;

public class GameService : IGameService
{
    private readonly Random _random = new();
    
    public GameSession CreateNewGame(Difficulty difficulty = Difficulty.Expert)
    {
        var gameSession = new GameSession { Difficulty = difficulty };
        var settings = DifficultySettings.GetSettings(difficulty);
        
        // Generate questions based on difficulty settings
        for (int i = 0; i < settings.QuestionCount; i++)
        {
            gameSession.Questions.Add(GenerateRandomQuestion(i + 1, settings));
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
            (userAnswer == 0 ? 0 : Math.Abs(userAnswer) * 100) : 
            Math.Round((difference / Math.Abs(correctAnswer)) * 100, 1);
        
        currentQuestion.PercentageDifference = percentageDifference;
        
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
    
    private MathQuestion GenerateRandomQuestion(int id, DifficultySettings settings)
    {
        var operation = settings.AllowedOperations[_random.Next(0, settings.AllowedOperations.Length)];
        var question = new MathQuestion
        {
            Id = id,
            Operation = operation
        };
        
        var maxValue = (int)Math.Pow(10, settings.MaxDigits) - 1;
        
        switch (operation)
        {
            case MathOperation.Addition:
                question.Number1 = _random.Next(1, maxValue + 1);
                question.Number2 = _random.Next(1, maxValue + 1);
                question.CorrectAnswer = question.Number1 + question.Number2;
                break;
                
            case MathOperation.Subtraction:
                question.Number1 = _random.Next(1, maxValue + 1);
                question.Number2 = _random.Next(1, Math.Min(question.Number1, maxValue) + 1);
                question.CorrectAnswer = question.Number1 - question.Number2;
                break;
                
            case MathOperation.Multiplication:
                var multiplicationMax = settings.MaxDigits <= 2 ? 99 : 100;
                question.Number1 = _random.Next(1, multiplicationMax + 1);
                question.Number2 = _random.Next(1, multiplicationMax + 1);
                question.CorrectAnswer = question.Number1 * question.Number2;
                break;
                
            case MathOperation.Division:
                var divisionMaxDividend = settings.MaxDigits <= 2 ? 99 : 1000;
                var divisionMaxDivisor = settings.MaxDigits <= 2 ? 9 : 100;
                question.Number1 = _random.Next(1, divisionMaxDividend + 1);
                question.Number2 = _random.Next(1, divisionMaxDivisor + 1);
                question.CorrectAnswer = Math.Round((double)question.Number1 / question.Number2, 1);
                break;
        }
        
        return question;
    }
}