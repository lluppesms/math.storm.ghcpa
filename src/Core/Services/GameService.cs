namespace MathStorm.Core.Services;

public class GameService : IGameService
{
    private readonly Random _random = new();
    private readonly IStoryQuestionService _storyQuestionService;

    public GameService()
        : this(new StoryQuestionService())
    {
    }

    public GameService(IStoryQuestionService storyQuestionService)
    {
        _storyQuestionService = storyQuestionService;
    }

    public GameSession CreateNewGame(Difficulty difficulty = Difficulty.Expert, GameMode gameMode = GameMode.Classic)
    {
        var gameSession = new GameSession
        {
            Difficulty = difficulty,
            GameMode = gameMode
        };

        var settings = DifficultySettings.GetSettings(difficulty);

        for (int i = 0; i < settings.QuestionCount; i++)
        {
            gameSession.Questions.Add(GenerateRandomQuestion(i + 1, difficulty, settings, gameMode));
        }

        return gameSession;
    }

    public Task<GameSession> CreateNewGameAsync(Difficulty difficulty = Difficulty.Expert, GameMode gameMode = GameMode.Classic)
    {
        return Task.FromResult(CreateNewGame(difficulty, gameMode));
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
        {
            return;
        }

        var timeElapsed = DateTime.Now - gameSession.QuestionStartTime.Value;
        currentQuestion.TimeInSeconds = Math.Round(timeElapsed.TotalSeconds, 1);
        currentQuestion.UserAnswer = userAnswer;

        var correctAnswer = currentQuestion.CorrectAnswer;
        var difference = Math.Abs(correctAnswer - userAnswer);

        var percentageDifference = (correctAnswer == 0 && userAnswer == 0) ? 0 :
            (correctAnswer == 0) ? 200 :
            Math.Round((difference / Math.Abs(correctAnswer)) * 100, 1);

        percentageDifference = Math.Min(percentageDifference, 200);
        currentQuestion.PercentageDifference = percentageDifference;

        var accuracyScore = percentageDifference * 3;

        var timeMultiplier = gameSession.Difficulty switch
        {
            Difficulty.Beginner => 5,
            Difficulty.Novice => 10,
            Difficulty.Intermediate or Difficulty.Expert => 15,
            _ => 10
        };

        var timeScore = 0.0;
        var timeInSeconds = currentQuestion.TimeInSeconds;

        if (timeInSeconds <= 10)
        {
            timeScore = timeInSeconds * timeMultiplier;
        }
        else
        {
            timeScore = 10 * timeMultiplier;
            var additionalTime = timeInSeconds - 10;
            timeScore += additionalTime * (timeMultiplier / 2.0);
        }

        currentQuestion.AccuracyScore = Math.Round(accuracyScore, 1);
        currentQuestion.TimeScore = Math.Round(timeScore, 1);
        currentQuestion.Score = Math.Round(accuracyScore + timeScore, 1);
    }

    public void NextQuestion(GameSession gameSession)
    {
        gameSession.CurrentQuestionIndex++;
        gameSession.QuestionStartTime = null;
    }

    private MathQuestion GenerateRandomQuestion(int id, Difficulty difficulty, DifficultySettings settings, GameMode gameMode)
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
                if (difficulty == Difficulty.Beginner)
                {
                    if (_random.Next(0, 2) == 0)
                    {
                        question.Number1 = _random.Next(1, 10);
                        question.Number2 = _random.Next(10, 100);
                    }
                    else
                    {
                        question.Number1 = _random.Next(10, 100);
                        question.Number2 = _random.Next(1, 10);
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
                if (difficulty == Difficulty.Beginner)
                {
                    question.Number1 = _random.Next(10, 100);
                    question.Number2 = _random.Next(1, 10);
                }
                else
                {
                    question.Number1 = _random.Next(1, maxValue + 1);
                    question.Number2 = _random.Next(1, Math.Min(question.Number1, maxValue) + 1);
                }

                question.CorrectAnswer = question.Number1 - question.Number2;
                break;

            case MathOperation.Multiplication:
                if (difficulty == Difficulty.Novice)
                {
                    question.Number2 = _random.Next(1, 10);
                    var minNumber1 = question.Number2 + 1;
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
                if (difficulty == Difficulty.Novice)
                {
                    var divisionMaxDivisor = Math.Min(9, maxValue);
                    question.Number2 = _random.Next(2, divisionMaxDivisor + 1);

                    var maxMultiplier = maxValue / question.Number2;
                    var multiplier = _random.Next(2, Math.Min(maxMultiplier + 1, 10));
                    question.Number1 = question.Number2 * multiplier;
                    question.CorrectAnswer = multiplier;
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

        if (gameMode == GameMode.StoryTime)
        {
            question.PromptText = _storyQuestionService.CreateStoryQuestion(question).PromptText;
        }

        return question;
    }
}
