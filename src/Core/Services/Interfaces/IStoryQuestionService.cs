using MathStorm.Core.Models;

namespace MathStorm.Core.Services;

public interface IStoryQuestionService
{
    StoryQuestion CreateStoryQuestion(MathQuestion question);
}
