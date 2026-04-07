using CyberResilience.Analysis.Config;
using CyberResilience.Analysis.Models;
using MongoDB.Driver;

namespace CyberResilience.Analysis.Services;

public sealed class MongoAnalysisRepository : IAnalysisRepository
{
    private readonly IMongoCollection<AnalysisResult> _collection;

    public MongoAnalysisRepository(MongoDbOptions options)
    {
        var client = new MongoClient(options.ConnectionString);
        var db = client.GetDatabase(options.Database);
        _collection = db.GetCollection<AnalysisResult>("analysis_results");

        var index = Builders<AnalysisResult>.IndexKeys.Ascending(x => x.EventId);
        _collection.Indexes.CreateOne(new CreateIndexModel<AnalysisResult>(index,
            new CreateIndexOptions { Unique = true, Background = true }));
    }

    public async Task SaveAsync(AnalysisResult result, CancellationToken ct = default)
    {
        await _collection.InsertOneAsync(result, cancellationToken: ct);
    }
}
