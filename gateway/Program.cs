using CyberResilience.Gateway.Middleware;
using Prometheus;
using Serilog;
using Serilog.Formatting.Compact;


Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter())
    .Enrich.FromLogContext()
    .Enrich.WithProperty("service", "yarp-gateway")
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.Console(new CompactJsonFormatter())
        .Enrich.FromLogContext()
        .Enrich.WithProperty("service", "yarp-gateway"));

    // YARP reverse proxy — routes defined in appsettings.json
    builder.Services
        .AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

    // Downstream health checks
    builder.Services
        .AddHealthChecks()
        .AddUrlGroup(
            new Uri("http://ingestion-service:8081/actuator/health"),
            name: "ingestion-service",
            tags: ["downstream"])
        .AddUrlGroup(
            new Uri("http://analysis-service:8082/health"),
            name: "analysis-service",
            tags: ["downstream"]);

    builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
        p.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [])
         .AllowAnyHeader()
         .WithMethods("GET", "POST")));

    var app = builder.Build();

    app.UseSerilogRequestLogging(o =>
    {
        o.EnrichDiagnosticContext = (diag, ctx) =>
        {
            diag.Set("RemoteIpAddress", ctx.Connection.RemoteIpAddress);
            diag.Set("UserAgent", ctx.Request.Headers.UserAgent.ToString());
        };
    });

    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseCors();
    app.UseHttpMetrics();

    app.MapHealthChecks("/health");
    app.MapMetrics("/metrics");
    app.MapReverseProxy();

    Log.Information("YARP Gateway starting on {Urls}", builder.Configuration["ASPNETCORE_URLS"]);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Gateway terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
