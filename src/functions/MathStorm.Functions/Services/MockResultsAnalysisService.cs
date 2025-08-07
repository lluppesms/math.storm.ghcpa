namespace MathStorm.Functions.Services;

public class MockResultsAnalysisService : IResultsAnalysisService
{
    private readonly ILogger<MockResultsAnalysisService> _logger;

    public MockResultsAnalysisService(ILogger<MockResultsAnalysisService> logger)
    {
        _logger = logger;
    }

    public Task<string> AnalyzeGameResultsAsync(ResultsAnalysisRequestDto request)
    {
        _logger.LogInformation($"Mock analysis for {request.Username} with {request.Personality} personality");

        var correctAnswers = request.Questions.Count(q => Math.Abs(q.UserAnswer - q.CorrectAnswer) < 0.01);
        var accuracy = (double)correctAnswers / request.Questions.Count * 100;

        var analysis = request.Personality.ToLowerInvariant() switch
        {
            "comedyroast" => $"Oh {request.Username}, {accuracy:F1}% accuracy? I've seen calculators with more personality! But hey, at least you're consistent... consistently confusing addition with subtraction!",
            "pirate" => $"Arrr, {request.Username}! Ye answered {correctAnswers} out of {request.Questions.Count} correctly, matey! That be {accuracy:F1}% - not bad for a landlubber navigating the treacherous waters of mathematics!",
            "limerick" => $"A player named {request.Username} tried math one day,\nWith {accuracy:F1}% in quite their own way,\nThe numbers did dance,\nGiven more than a chance,\nBut practice makes perfect, I'd say!",
            "sportsbroadcaster" => $"Ladies and gentlemen, what a performance by {request.Username}! {correctAnswers} correct answers out of {request.Questions.Count} attempts - that's a {accuracy:F1}% completion rate! The crowd is on their feet!",
            "haiku" => $"{request.Username} tries,\n{correctAnswers} of {request.Questions.Count} answers correct,\nMath journey continues.",
            "australian" => $"G'day {request.Username}! Fair dinkum, you knocked out {correctAnswers} correct answers - that's {accuracy:F1}% mate! Not too shabby for a day's work in the mathematical outback!",
            "yourmother" => $"Oh sweetheart {request.Username}, you got {correctAnswers} right! I'm so proud of you for trying your best. Remember, even when the numbers get tricky, Mommy believes in you!",
            _ => $"Great job {request.Username}! You achieved {accuracy:F1}% accuracy with {correctAnswers} correct answers out of {request.Questions.Count} questions. Keep practicing and you'll continue to improve!"
        };

        return Task.FromResult(analysis);
    }
}