namespace MathStorm.Core.Models;

public class GameSession
{
    public List<MathQuestion> Questions { get; set; } = [];
    public int CurrentQuestionIndex { get; set; } = 0;
    public bool IsGameComplete => CurrentQuestionIndex >= Questions.Count;
    public bool IsGameStarted { get; set; } = false;
    public DateTime? QuestionStartTime { get; set; }
    public double TotalScore => Questions.Sum(q => q.Score);
    public Difficulty Difficulty { get; set; } = Difficulty.Expert;
    public GameMode GameMode { get; set; } = GameMode.Classic;

    public MathQuestion? CurrentQuestion =>
        CurrentQuestionIndex < Questions.Count ? Questions[CurrentQuestionIndex] : null;
}
