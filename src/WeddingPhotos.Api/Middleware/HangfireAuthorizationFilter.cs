using Hangfire.Dashboard;
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
            return false;
        }

        // Get Authorization header
        string authHeader = httpContext.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic "))
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
            var credentials = decodedCredentials.Split(':', 2);

            if (credentials.Length != 2)
            {
                SetChallengeResponse(httpContext);
                return false;
            }

            var username = credentials[0];
            var password = credentials[1];

            // Verify credentials
            if (username == _username && password == _password)
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
