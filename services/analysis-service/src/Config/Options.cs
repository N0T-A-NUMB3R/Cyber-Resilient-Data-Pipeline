namespace CyberResilience.Analysis.Config;

public sealed class RabbitMqOptions
{
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string User { get; init; } = "guest";
    public string Pass { get; init; } = "guest";
    public string Vhost { get; init; } = "/";
}

public sealed class MongoDbOptions
{
    public string ConnectionString { get; init; } = "mongodb://localhost:27017";
    public string Database { get; init; } = "cyber_logs";
}
