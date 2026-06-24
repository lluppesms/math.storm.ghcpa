namespace MathStorm.Core.Models;

public enum Difficulty
{
    Beginner,
    Novice,
    Intermediate,
    Expert
}

public class DifficultySettings
{
    public int QuestionCount { get; set; }
    public int MaxDigits { get; set; }
    public MathOperation[] AllowedOperations { get; set; } = [];

    public static DifficultySettings GetSettings(Difficulty difficulty)
    {
        return difficulty switch
        {
            Difficulty.Beginner => new DifficultySettings
            {
                QuestionCount = 5,
                MaxDigits = 2,
                AllowedOperations = [MathOperation.Addition, MathOperation.Subtraction]
            },
            Difficulty.Novice => new DifficultySettings
            {
                QuestionCount = 5,
                MaxDigits = 2,
                AllowedOperations = [MathOperation.Addition, MathOperation.Subtraction, MathOperation.Multiplication, MathOperation.Division]
            },
            Difficulty.Intermediate => new DifficultySettings
            {
                QuestionCount = 10,
                MaxDigits = 3,
                AllowedOperations = [MathOperation.Addition, MathOperation.Subtraction, MathOperation.Multiplication, MathOperation.Division]
            },
            Difficulty.Expert => new DifficultySettings
            {
                QuestionCount = 10,
                MaxDigits = 4,
                AllowedOperations = [MathOperation.Addition, MathOperation.Subtraction, MathOperation.Multiplication, MathOperation.Division]
            },
            _ => throw new ArgumentOutOfRangeException(nameof(difficulty))
        };
    }
}
