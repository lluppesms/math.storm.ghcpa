using MathStorm.Core.Models;

namespace MathStorm.Core.Services;

public class StoryQuestionService : IStoryQuestionService
{
    public StoryQuestion CreateStoryQuestion(MathQuestion question)
    {
        ArgumentNullException.ThrowIfNull(question);

        return new StoryQuestion("Story Time", StoryProblemFormatter.Format(question));
    }
}
