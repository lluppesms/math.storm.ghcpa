namespace MathStorm.Core.Services;

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

    public async Task<GameSession> CreateNewGameAsync(Difficulty difficulty = Difficulty.Expert)
    {
        // For the base implementation, just return the synchronous version
        // This is overridden in FunctionBasedGameService to call Azure Functions
        return await Task.FromResult(CreateNewGame(difficulty));
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

        // Fixed: Handle division by zero case properly
        var percentageDifference = (correctAnswer == 0 && userAnswer == 0) ? 0 :
            (correctAnswer == 0) ? 200 : // Cap at 200% when correct is 0 but user entered something
            Math.Round((difference / Math.Abs(correctAnswer)) * 100, 1);

        // Cap percentage difference at 200% to avoid enormous scores for one bad answer
        percentageDifference = Math.Min(percentageDifference, 200);

        currentQuestion.PercentageDifference = percentageDifference;

        // New scoring algorithm:
        // Score = (Accuracy Component) + (Time Component)
        // Lower score is better
        // Accuracy is weighted more heavily than time

        // Accuracy Component (weighted 3x): ranges from 0 (perfect) to 600 (200% error)
        var accuracyScore = percentageDifference * 3;

        // Time multiplier based on difficulty:
        // Beginner: 5 points per second (more forgiving)
        // Novice: 10 points per second
        // Intermediate/Expert: 15 points per second (more challenging)
        var timeMultiplier = gameSession.Difficulty switch
        {
            Difficulty.Beginner => 5,
            Difficulty.Novice => 10,
            Difficulty.Intermediate or Difficulty.Expert => 15,
            _ => 10 // Default fallback
        };

        // Time Component:
        // 0-10 seconds: timeMultiplier points per second (precise to 1/10 second)
        // >10 seconds: diminishing penalty (half rate)
        var timeScore = 0.0;
        var timeInSeconds = currentQuestion.TimeInSeconds;

        if (timeInSeconds <= 10)
        {
            // First 10 seconds: timeMultiplier points per second
            timeScore = timeInSeconds * timeMultiplier;
        }
        else
        {
            // First 10 seconds at full rate
            timeScore = 10 * timeMultiplier;

            // Additional time beyond 10 seconds at half rate
            var additionalTime = timeInSeconds - 10;
            timeScore += additionalTime * (timeMultiplier / 2.0);
        }

        // Store individual components
        currentQuestion.AccuracyScore = Math.Round(accuracyScore, 1);
        currentQuestion.TimeScore = Math.Round(timeScore, 1);
        currentQuestion.Score = Math.Round(accuracyScore + timeScore, 1);
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
        var gameSession = new GameSession { Difficulty = GetDifficultyFromSettings(settings) };

        switch (operation)
        {
            case MathOperation.Addition:
                if (GetDifficultyFromSettings(settings) == Difficulty.Beginner)
                {
                    // For Beginner: one number should be single digit (1-9), the other should be two digits (10-99)
                    if (_random.Next(0, 2) == 0)
                    {
                        question.Number1 = _random.Next(1, 10); // Single digit (1-9)
                        question.Number2 = _random.Next(10, 100); // Two digits (10-99)
                    }
                    else
                    {
                        question.Number1 = _random.Next(10, 100); // Two digits (10-99)
                        question.Number2 = _random.Next(1, 10); // Single digit (1-9)
                    }
                }
                else
                {
                    question.Number1 = _random.Next(1, maxValue + 1);
                    question.Number2 = _random.Next(1, maxValue + 1);
                }
                question.CorrectAnswer = question.Number1 + question.Number2;
                break;

            case MathOperation.Subtraction:
                if (GetDifficultyFromSettings(settings) == Difficulty.Beginner)
                {
                    // For Beginner: first number should be two digits (10-99), second should be single digit (1-9)
                    question.Number1 = _random.Next(10, 100); // Two digits (10-99)
                    question.Number2 = _random.Next(1, 10); // Single digit (1-9)
                }
                else
                {
                    question.Number1 = _random.Next(1, maxValue + 1);
                    question.Number2 = _random.Next(1, Math.Min(question.Number1, maxValue) + 1);
                }
                question.CorrectAnswer = question.Number1 - question.Number2;
                break;

            case MathOperation.Multiplication:
                if (GetDifficultyFromSettings(settings) == Difficulty.Novice)
                {
                    // For Novice: second number should be single digit, first number should be larger
                    question.Number2 = _random.Next(1, 10); // Single digit (1-9)
                    var minNumber1 = question.Number2 + 1; // Ensure Number1 > Number2
                    question.Number1 = _random.Next(minNumber1, Math.Min(maxValue + 1, 100));
                }
                else
                {
                    var multiplicationMax = settings.MaxDigits <= 2 ? 99 : 100;
                    question.Number1 = _random.Next(1, multiplicationMax + 1);
                    question.Number2 = _random.Next(1, multiplicationMax + 1);
                }
                question.CorrectAnswer = question.Number1 * question.Number2;
                break;

            case MathOperation.Division:
                if (GetDifficultyFromSettings(settings) == Difficulty.Novice)
                {
                    // For Novice: ensure even division and first number > second number
                    var divisionMaxDivisor = Math.Min(9, maxValue); // Keep divisor reasonable
                    question.Number2 = _random.Next(2, divisionMaxDivisor + 1); // Start from 2 to avoid division by 1

                    // Generate a multiplier to ensure even division
                    var maxMultiplier = maxValue / question.Number2;
                    var multiplier = _random.Next(2, Math.Min(maxMultiplier + 1, 10)); // Start from 2 to ensure Number1 > Number2
                    question.Number1 = question.Number2 * multiplier;
                    question.CorrectAnswer = multiplier; // Will be a whole number
                }
                else
                {
                    var divisionMaxDividend = settings.MaxDigits <= 2 ? 99 : 1000;
                    var divisionMaxDivisor = settings.MaxDigits <= 2 ? 9 : 100;
                    question.Number1 = _random.Next(1, divisionMaxDividend + 1);
                    question.Number2 = _random.Next(1, divisionMaxDivisor + 1);
                    question.CorrectAnswer = Math.Round((double)question.Number1 / question.Number2, 1);
                }
                break;
        }

        return question;
    }

    private Difficulty GetDifficultyFromSettings(DifficultySettings settings)
    {
        // Determine difficulty based on settings characteristics
        if (settings.QuestionCount == 5 && settings.MaxDigits == 2)
        {
            return settings.AllowedOperations.Length == 2 ? Difficulty.Beginner : Difficulty.Novice;
        }
        else if (settings.QuestionCount == 10 && settings.MaxDigits == 3)
        {
            return Difficulty.Intermediate;
        }
        else
        {
            return Difficulty.Expert;
        }
    }
}