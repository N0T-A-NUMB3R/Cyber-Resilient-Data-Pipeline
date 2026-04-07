using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CyberResilience.Analysis.Models;

public sealed class AnalysisResult
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string AnalysisId { get; init; } = Guid.NewGuid().ToString();
    public string EventId { get; init; } = default!;
    public string SourceIp { get; init; } = default!;
    public string EventType { get; init; } = default!;
    public DateTimeOffset EventTimestamp { get; init; }
    public DateTimeOffset AnalyzedAt { get; init; } = DateTimeOffset.UtcNow;
    public ThreatLevel ThreatLevel { get; init; }
    public bool RuleMatched { get; init; }
    public string? MatchedPattern { get; init; }
    public string? LogData { get; init; }
}

public enum ThreatLevel { None, Low, Medium, High, Critical }
