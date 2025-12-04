namespace MathStorm.Services;

public interface IResultsAnalysisService
{
    Task<string> AnalyzeGameResultsAsync(ResultsAnalysisRequestDto request);
}
