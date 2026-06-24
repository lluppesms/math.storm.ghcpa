namespace MathStorm.Core.Models;

public static class StoryProblemFormatter
{
    public static string Format(MathQuestion question)
    {
        ArgumentNullException.ThrowIfNull(question);

        return Format(question.Id, question.Number1, question.Number2, question.Operation);
    }

    public static string Format(int questionId, int number1, int number2, MathOperation operation)
    {
        var variant = Math.Abs(questionId + number1 + number2 + (int)operation) % 3;

        return operation switch
        {
            MathOperation.Addition => additionPrompts[variant](number1, number2),
            MathOperation.Subtraction => subtractionPrompts[variant](number1, number2),
            MathOperation.Multiplication => multiplicationPrompts[variant](number1, number2),
            MathOperation.Division => divisionPrompts[variant](number1, number2),
            _ => $"{number1} and {number2} are in play. What answer fits the clue?"
        };
    }

    private static readonly Func<int, int, string>[] additionPrompts =
    [
        (number1, number2) => $"Mina packed {number1} glow sticks for the storm parade and found {number2} more in her backpack. How many glow sticks does she have altogether?",
        (number1, number2) => $"A cloud racer collected {number1} star tokens on the first lap and {number2} more on the next lap. How many star tokens were collected in total?",
        (number1, number2) => $"The weather station tracked {number1} bright sparks before lunch and {number2} after lunch. How many sparks were tracked altogether?"
    ];

    private static readonly Func<int, int, string>[] subtractionPrompts =
    [
        (number1, number2) => $"Kai started with {number1} energy crystals and used {number2} to power the shield. How many energy crystals are left?",
        (number1, number2) => $"A storm pilot carried {number1} repair bolts and spent {number2} fixing the engine. How many repair bolts remain?",
        (number1, number2) => $"The arena lights had {number1} charge cells, then {number2} burned out. How many charge cells are still working?"
    ];

    private static readonly Func<int, int, string>[] multiplicationPrompts =
    [
        (number1, number2) => $"{number1} teams each found {number2} thunder badges after the match. How many thunder badges did they find altogether?",
        (number1, number2) => $"A supply drone delivers {number2} snack packs to each of {number1} cabins. How many snack packs are delivered in all?",
        (number1, number2) => $"{number1} rows of seats each hold {number2} fans for Story Time night. How many fans can sit down altogether?"
    ];

    private static readonly Func<int, int, string>[] divisionPrompts =
    [
        (number1, number2) => $"{number1} comet chips are packed equally into {number2} supply boxes. How many comet chips go in each box?",
        (number1, number2) => $"A coach splits {number1} practice minutes evenly across {number2} teams. How many minutes does each team get?",
        (number1, number2) => $"{number1} storm cards are shared evenly among {number2} players. How many cards does each player receive?"
    ];
}
