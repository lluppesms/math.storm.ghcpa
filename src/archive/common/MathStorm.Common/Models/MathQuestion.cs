namespace MathStorm.Common.Models;

public class MathQuestion
{
    public int Id { get; set; }
    public int Number1 { get; set; }
    public int Number2 { get; set; }
    public MathOperation Operation { get; set; }
    public double CorrectAnswer { get; set; }
    public double? UserAnswer { get; set; }
    public double TimeInSeconds { get; set; }
    public double PercentageDifference { get; set; }
    public double Score { get; set; }
    
    public string QuestionText => $"{Number1} {GetOperationSymbol()} {Number2}";
    
    public string GetOperationSymbol()
    {
        return Operation switch
        {
            MathOperation.Addition => "+",
            MathOperation.Subtraction => "-",
            MathOperation.Multiplication => "×",
            MathOperation.Division => "÷",
            _ => "?"
        };
    }
}