using CyberResilience.Analysis.Models;

namespace CyberResilience.Analysis.Services;

public interface IAnalysisRepository
{
    Task SaveAsync(AnalysisResult result, CancellationToken ct = default);
}
