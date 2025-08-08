namespace MathStorm.Functions.Services;

public interface IResultsAnalysisService
{
    Task<string> AnalyzeGameResultsAsync(ResultsAnalysisRequestDto request);
}