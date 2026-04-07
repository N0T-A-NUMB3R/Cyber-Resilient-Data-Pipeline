using CyberResilience.Analysis.Models;

namespace CyberResilience.Analysis.Services;

public interface IThreatDetector
{
    (ThreatLevel level, bool matched, string? pattern) Evaluate(CyberEventMessage message);
}
