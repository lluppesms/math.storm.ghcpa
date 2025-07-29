namespace MathStorm.Shared.DTOs;

public class GameRequestDto
{
    public string Difficulty { get; set; } = "Expert";
}

public class GameResponseDto
{
    public string GameId { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public List<QuestionDto> Questions { get; set; } = [];
}

public class QuestionDto
{
    public int Id { get; set; }
    public int Number1 { get; set; }
    public int Number2 { get; set; }
    public string Operation { get; set; } = string.Empty;
    public double CorrectAnswer { get; set; }
    public string QuestionText { get; set; } = string.Empty;
}