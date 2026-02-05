namespace WeddingPhotos.Api.Middleware;

/// <summary>
/// Middleware that adds security headers to all responses
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;

    public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Remove server header to hide implementation details
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");

        // X-Content-Type-Options: Prevent MIME type sniffing
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // X-Frame-Options: Prevent clickjacking
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // X-XSS-Protection: Enable XSS filter
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

        // Referrer-Policy: Control referrer information
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Content-Security-Policy: Prevent XSS and injection attacks
        var csp = "default-src 'self'; " +
                  "img-src 'self' data: https: blob:; " +
                  "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                  "style-src 'self' 'unsafe-inline'; " +
                  "font-src 'self' data: https://fonts.gstatic.com; " +
                  "connect-src 'self' https:; " +
                  "frame-ancestors 'none';";

        context.Response.Headers.Append("Content-Security-Policy", csp);

        // Strict-Transport-Security (HSTS): Force HTTPS (only in production with HTTPS)
        if (context.Request.IsHttps)
        {
            context.Response.Headers.Append(
                "Strict-Transport-Security",
                "max-age=31536000; includeSubDomains; preload"
            );
        }

        // Permissions-Policy: Disable unnecessary browser features
        context.Response.Headers.Append(
            "Permissions-Policy",
            "geolocation=(), microphone=(), camera=(), payment=()"
        );

        await _next(context);
    }
}