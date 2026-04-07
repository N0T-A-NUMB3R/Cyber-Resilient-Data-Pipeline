using CyberResilience.Analysis.Models;

namespace CyberResilience.Analysis.Services;

public sealed class ThreatDetector : IThreatDetector
{
    private static readonly Dictionary<string, (ThreatLevel level, string pattern)> Rules = new(StringComparer.OrdinalIgnoreCase)
    {
        ["BRUTE_FORCE"]         = (ThreatLevel.Critical, "BRUTE_FORCE_PATTERN"),
        ["PORT_SCAN"]           = (ThreatLevel.High,     "PORT_SCAN_PATTERN"),
        ["UNAUTHORIZED_ACCESS"] = (ThreatLevel.High,     "UNAUTHORIZED_ACCESS_PATTERN"),
        ["SQL_INJECTION"]       = (ThreatLevel.Critical, "INJECTION_PATTERN"),
        ["XSS"]                 = (ThreatLevel.Medium,   "XSS_PATTERN"),
        ["RECON"]               = (ThreatLevel.Low,      "RECON_PATTERN"),
    };

    public (ThreatLevel level, bool matched, string? pattern) Evaluate(CyberEventMessage message)
    {
        if (Rules.TryGetValue(message.EventType, out var rule))
            return (rule.level, true, rule.pattern);

        return (ThreatLevel.None, false, null);
    }
}
