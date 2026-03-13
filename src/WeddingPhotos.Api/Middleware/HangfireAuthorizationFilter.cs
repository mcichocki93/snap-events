using Hangfire.Dashboard;
using System.Security.Cryptography;
using System.Text;

namespace WeddingPhotos.Api.Middleware;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly string? _username;
    private readonly string? _password;

    public HangfireAuthorizationFilter()
    {
        _username = Environment.GetEnvironmentVariable("HANGFIRE_DASHBOARD_USERNAME");
        _password = Environment.GetEnvironmentVariable("HANGFIRE_DASHBOARD_PASSWORD");
    }

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // If credentials not configured, deny access (secure by default)
        if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password))
        {
            SetChallengeResponse(httpContext);
            return false;
        }

        // Get Authorization header
        string authHeader = httpContext.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            // Send 401 Unauthorized with WWW-Authenticate header
            SetChallengeResponse(httpContext);
            return false;
        }

        try
        {
            // Decode Basic Auth credentials
            var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
            var decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
            var colonIndex = decodedCredentials.IndexOf(':');

            if (colonIndex < 0)
            {
                SetChallengeResponse(httpContext);
                return false;
            }

            var username = decodedCredentials[..colonIndex];
            var password = decodedCredentials[(colonIndex + 1)..];

            // Timing-safe comparison to prevent timing attacks
            var usernameMatch = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(username),
                Encoding.UTF8.GetBytes(_username));

            var passwordMatch = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(password),
                Encoding.UTF8.GetBytes(_password));

            if (usernameMatch && passwordMatch)
            {
                return true;
            }

            SetChallengeResponse(httpContext);
            return false;
        }
        catch
        {
            SetChallengeResponse(httpContext);
            return false;
        }
    }

    private static void SetChallengeResponse(Microsoft.AspNetCore.Http.HttpContext httpContext)
    {
        httpContext.Response.StatusCode = 401;
        httpContext.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Hangfire Dashboard\"";
    }
}
