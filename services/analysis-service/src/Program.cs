using CyberResilience.Analysis.Config;
using CyberResilience.Analysis.Consumers;
using CyberResilience.Analysis.Services;
using MassTransit;
using Prometheus;
using Serilog;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter())
    .Enrich.WithProperty("service", "analysis-service")
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, _, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console(new CompactJsonFormatter())
        .Enrich.FromLogContext()
        .Enrich.WithProperty("service", "analysis-service"));

    var rmq = builder.Configuration.GetSection("RabbitMQ").Get<RabbitMqOptions>()!;
    var mongo = builder.Configuration.GetSection("MongoDB").Get<MongoDbOptions>()!;

    builder.Services.AddSingleton(mongo);
    builder.Services.AddSingleton<IAnalysisRepository, MongoAnalysisRepository>();
    builder.Services.AddSingleton<IThreatDetector, ThreatDetector>();

    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<CyberEventConsumer>();

        x.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host(new Uri($"amqp://{rmq.Host}:{rmq.Port}{(rmq.Vhost == "/" ? "/" : "/" + rmq.Vhost)}"), h =>
            {
                h.Username(rmq.User);
                h.Password(rmq.Pass);
            });

            cfg.UseMessageRetry(r => r.Exponential(5,
                TimeSpan.FromMilliseconds(200),
                TimeSpan.FromSeconds(30),
                TimeSpan.FromMilliseconds(500)));

            cfg.ReceiveEndpoint("cyber.events.raw", e =>
            {
                e.PrefetchCount = 16;
                e.SetQueueArgument("x-message-ttl", 86_400_000);
                e.SetQueueArgument("x-dead-letter-exchange", "cyber.events.dlx");

                // Disabilita la topologia automatica di MassTransit e si lega
                // all'exchange topic già dichiarato dall'Ingestion Service Java.
                e.ConfigureConsumeTopology = false;
                e.Bind("cyber.events.exchange", b =>
                {
                    b.ExchangeType = "topic";
                    b.RoutingKey   = "cyber.event.raw";
                });

                e.ConfigureConsumer<CyberEventConsumer>(ctx);
            });
        });
    });

    builder.Services
        .AddHealthChecks()
        .AddRabbitMQ(rabbitConnectionString: $"amqp://{rmq.User}:{rmq.Pass}@{rmq.Host}:{rmq.Port}/{rmq.Vhost}")
        .AddMongoDb(mongo.ConnectionString, name: "mongodb");

    var app = builder.Build();

    app.UseHttpMetrics();
    app.MapHealthChecks("/health");
    app.MapMetrics("/metrics");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Analysis service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
