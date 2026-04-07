namespace CyberResilience.Gateway.Middleware;

internal sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        ctx.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        ctx.Response.Headers.Append("X-Frame-Options", "DENY");
        ctx.Response.Headers.Append("Referrer-Policy", "no-referrer");
        ctx.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=()");
        ctx.Response.Headers.Append(
            "Content-Security-Policy",
            "default-src 'none'; frame-ancestors 'none'");

        // Remove server fingerprinting headers added by Kestrel
        ctx.Response.Headers.Remove("Server");
        ctx.Response.Headers.Remove("X-Powered-By");

        await next(ctx);
    }
}
