namespace CyberResilience.Analysis.Models;

public sealed record CyberEventMessage(
    string EventId,
    string SourceIp,
    string EventType,
    DateTimeOffset Timestamp,
    string? Payload);
