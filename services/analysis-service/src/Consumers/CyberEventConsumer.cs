using CyberResilience.Analysis.Models;
using CyberResilience.Analysis.Services;
using MassTransit;

namespace CyberResilience.Analysis.Consumers;

public sealed class CyberEventConsumer(
    IThreatDetector detector,
    IAnalysisRepository repository,
    ILogger<CyberEventConsumer> logger) : IConsumer<CyberEventMessage>
{
    public async Task Consume(ConsumeContext<CyberEventMessage> context)
    {
        var msg = context.Message;
        logger.LogInformation("[RECEIVED] eventId={EventId} type={Type}", msg.EventId, msg.EventType);

        var (level, matched, pattern) = detector.Evaluate(msg);

        var result = new AnalysisResult
        {
            EventId        = msg.EventId,
            SourceIp       = msg.SourceIp,
            EventType      = msg.EventType,
            EventTimestamp = msg.Timestamp,
            ThreatLevel    = level,
            RuleMatched    = matched,
            MatchedPattern = pattern,
            LogData        = msg.Payload,
        };

        await repository.SaveAsync(result, context.CancellationToken);

        logger.LogInformation("[ANALYZED] eventId={EventId} threat={Level} matched={Matched}",
            msg.EventId, level, matched);
    }
}
