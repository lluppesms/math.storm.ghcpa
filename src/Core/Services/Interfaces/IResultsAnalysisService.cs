namespace MathStorm.Core;

public interface IResultsAnalysisService
{
    Task<string> AnalyzeGameResultsAsync(ResultsAnalysisRequestDto request);
}
