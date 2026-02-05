using AspNetCoreRateLimit;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.MemoryStorage;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using Sentry;
using Sentry.AspNetCore;
using Serilog;
using Serilog.Events;
using WeddingPhotos.Api.Middleware;
using WeddingPhotos.Domain.Interfaces;
using WeddingPhotos.Infrastructure;
using WeddingPhotos.Infrastructure.Configuration;
using WeddingPhotos.Infrastructure.Repositories;
using WeddingPhotos.Infrastructure.Services;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Get Sentry DSN FIRST (before configuring Serilog) - Environment variable takes precedence
    var sentryDsn = Environment.GetEnvironmentVariable("SENTRY_DSN")
        ?? builder.Configuration["Sentry:Dsn"];

    // Configure Serilog with conditional Sentry sink
    var loggerConfig = new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        )
        .WriteTo.File(
            path: "logs/wedding-photos-.log",
            rollingInterval: RollingInterval.Day,
            rollOnFileSizeLimit: true,
            fileSizeLimitBytes: 10_000_000,
            retainedFileCountLimit: 30,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"
        );

    // Add Sentry sink ONLY if DSN is configured
    if (!string.IsNullOrEmpty(sentryDsn))
    {
        loggerConfig.WriteTo.Sentry(o =>
        {
            o.Dsn = sentryDsn;
            o.MinimumBreadcrumbLevel = LogEventLevel.Information;
            o.MinimumEventLevel = LogEventLevel.Warning;
        });
    }

    Log.Logger = loggerConfig.CreateLogger();

    Log.Information("Starting WeddingPhotos API");

    // Use Serilog for logging
    builder.Host.UseSerilog();

    // Configure Sentry (after builder is created so Configuration is available)
    if (!string.IsNullOrEmpty(sentryDsn))
    {
        builder.WebHost.UseSentry(o =>
        {
            o.Dsn = sentryDsn;
            o.Environment = builder.Configuration["Sentry:Environment"] ?? builder.Environment.EnvironmentName;

            // TracesSampleRate: 100% for dev, 10% for production (cost optimization)
            var defaultTracesRate = builder.Environment.IsProduction() ? 0.1 : 1.0;
            o.TracesSampleRate = double.Parse(
                builder.Configuration["Sentry:TracesSampleRate"] ?? defaultTracesRate.ToString(System.Globalization.CultureInfo.InvariantCulture),
                System.Globalization.CultureInfo.InvariantCulture
            );

            // ProfilesSampleRate: Performance profiling (CPU, memory)
            o.ProfilesSampleRate = builder.Environment.IsProduction() ? 0.1 : 1.0;

            o.SendDefaultPii = bool.Parse(builder.Configuration["Sentry:SendDefaultPii"] ?? "false");
            o.AttachStacktrace = true;
            o.MaxBreadcrumbs = 50;

            // Dynamic release from assembly version
            var version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0";
            o.Release = $"wedding-photos@{version}";

            // Filter out noise and expected exceptions
            o.SetBeforeSend((sentryEvent, hint) =>
            {
                // Ignore health check endpoints (generates too much noise)
                if (sentryEvent.Request?.Url?.Contains("/health") == true)
                    return null;

                // Ignore client-cancelled requests (OperationCanceledException)
                if (sentryEvent.Exception is OperationCanceledException)
                    return null;

                // Ignore TaskCanceledException (common when client disconnects)
                if (sentryEvent.Exception is TaskCanceledException)
                    return null;

                return sentryEvent;
            });
        });

        Log.Information("Sentry error tracking enabled for environment: {Environment}",
            builder.Configuration["Sentry:Environment"] ?? builder.Environment.EnvironmentName);
    }
    else
    {
        Log.Information("Sentry disabled - no DSN configured");
    }

    // ============================================
    // SECURITY CONFIGURATION
    // ============================================

    // Register HttpContextAccessor for getting client IP and User-Agent in services
    builder.Services.AddHttpContextAccessor();

    // Load CORS configuration from environment variable or appsettings
    var allowedOriginsEnv = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS");
    var allowedOrigins = !string.IsNullOrEmpty(allowedOriginsEnv)
        ? allowedOriginsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        : builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:5173", "http://localhost:3000" };

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("SecureCorsPolicy", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials()
                  .SetIsOriginAllowedToAllowWildcardSubdomains();
        });
    });

    // Rate Limiting
    builder.Services.AddMemoryCache();
    builder.Services.Configure<IpRateLimitOptions>(options =>
    {
        options.EnableEndpointRateLimiting = true;
        options.StackBlockedRequests = false;
        options.HttpStatusCode = 429;
        options.RealIpHeader = "X-Real-IP";
        options.ClientIdHeader = "X-ClientId";

        options.GeneralRules = new List<RateLimitRule>
        {
            new RateLimitRule { Endpoint = "*", Period = "1m", Limit = 60 },
            new RateLimitRule { Endpoint = "*/photo/upload/*", Period = "1h", Limit = 100 },
            new RateLimitRule { Endpoint = "*/photo/gallery/*", Period = "1m", Limit = 30 },
            new RateLimitRule { Endpoint = "*/photo/proxy/*", Period = "1m", Limit = 100 },
            new RateLimitRule { Endpoint = "*/contact", Period = "1h", Limit = 5 } // Contact form: 5 per hour (anti-spam)
        };
    });

    builder.Services.AddInMemoryRateLimiting();
    builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

    // Request Size Limits
    builder.Services.Configure<FormOptions>(options =>
    {
        options.ValueLengthLimit = int.MaxValue;
        options.MultipartBodyLengthLimit = 104857600;
        options.MultipartHeadersLengthLimit = 16384;
    });

    builder.Services.Configure<KestrelServerOptions>(options =>
    {
        options.Limits.MaxRequestBodySize = 104857600;
        options.Limits.MaxConcurrentConnections = 100;
        options.Limits.MaxConcurrentUpgradedConnections = 100;
        options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
        options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
    });

    // ============================================
    // APPLICATION SERVICES
    // ============================================

    // Load configuration with environment variable overrides
    var mongoConnectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING")
        ?? builder.Configuration.GetSection("MongoDbSettings:ConnectionString").Value;
    var mongoDatabaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME")
        ?? builder.Configuration.GetSection("MongoDbSettings:DatabaseName").Value;
    var mongoClientsCollection = Environment.GetEnvironmentVariable("MONGODB_CLIENTS_COLLECTION")
        ?? builder.Configuration.GetSection("MongoDbSettings:ClientsCollectionName").Value;

    var googleServiceAccountKeyPath = Environment.GetEnvironmentVariable("GOOGLE_SERVICE_ACCOUNT_KEY_PATH")
        ?? builder.Configuration.GetSection("GoogleCloud:ServiceAccountKeyPath").Value;
    var googleProjectId = Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID")
        ?? builder.Configuration.GetSection("GoogleCloud:ProjectId").Value;

    // Configure MongoDbSettings with environment variables
    builder.Services.Configure<MongoDbSettings>(options =>
    {
        options.ConnectionString = mongoConnectionString ?? string.Empty;
        options.DatabaseName = mongoDatabaseName ?? string.Empty;
        options.ClientsCollectionName = mongoClientsCollection ?? "Clients";
    });

    // Configure GoogleCloudSettings with environment variables
    builder.Services.Configure<GoogleCloudSettings>(options =>
    {
        options.ServiceAccountKeyPath = googleServiceAccountKeyPath ?? string.Empty;
        options.ProjectId = googleProjectId ?? string.Empty;
    });

    Log.Information("Configuration loaded successfully");

    if (string.IsNullOrEmpty(mongoConnectionString))
        throw new InvalidOperationException("MongoDB connection string is not configured. Set MONGODB_CONNECTION_STRING environment variable or MongoDbSettings:ConnectionString in appsettings.json");
    if (string.IsNullOrEmpty(mongoDatabaseName))
        throw new InvalidOperationException("MongoDB database name is not configured. Set MONGODB_DATABASE_NAME environment variable or MongoDbSettings:DatabaseName in appsettings.json");
    if (string.IsNullOrEmpty(googleServiceAccountKeyPath))
        throw new InvalidOperationException("Google service account key path is not configured. Set GOOGLE_SERVICE_ACCOUNT_KEY_PATH environment variable or GoogleCloud:ServiceAccountKeyPath in appsettings.json");
    if (string.IsNullOrEmpty(googleProjectId))
        throw new InvalidOperationException("Google project ID is not configured. Set GOOGLE_PROJECT_ID environment variable or GoogleCloud:ProjectId in appsettings.json");

    builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoConnectionString));
    builder.Services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDatabaseName));

    // Add Infrastructure services (includes cache, repositories, services)
    builder.Services.AddInfrastructure(builder.Configuration);

    // Hangfire Configuration with environment variable override
    var hangfireEnabled = bool.TryParse(
        Environment.GetEnvironmentVariable("HANGFIRE_ENABLED")
        ?? builder.Configuration["Hangfire:Enabled"],
        out var hfEnabled) && hfEnabled;

    if (hangfireEnabled)
    {
        builder.Services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseMemoryStorage());

        builder.Services.AddHangfireServer();
        Log.Information("Hangfire background jobs enabled");
    }

    builder.Services.AddControllers(options =>
    {
        options.MaxModelBindingCollectionSize = 1024;
    });

    // FluentValidation
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Health Checks Configuration
    var healthChecksBuilder = builder.Services.AddHealthChecks()
        .AddCheck("api", () => HealthCheckResult.Healthy("API is running"))
        .AddMongoDb(
            _ => new MongoClient(mongoConnectionString!),
            name: "mongodb",
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "db", "mongodb" });

    // Add Redis health check only if enabled
    var redisCacheSettings = builder.Configuration.GetSection("RedisCache").Get<RedisCacheSettings>();
    if (redisCacheSettings?.Enabled == true)
    {
        healthChecksBuilder.AddRedis(
            redisCacheSettings.ConnectionString,
            name: "redis",
            failureStatus: HealthStatus.Degraded, // Degraded instead of Unhealthy (app works without Redis)
            tags: new[] { "cache", "redis" });
    }

    var app = builder.Build();

    // ============================================
    // MIDDLEWARE PIPELINE
    // ============================================

    app.UseMiddleware<SecurityHeadersMiddleware>();

    //if (app.Environment.IsProduction())
    //{
    //    app.UseHttpsRedirection();
    //}

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms from {RemoteIpAddress}";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());

            // Sentry tags
            SentrySdk.ConfigureScope(scope =>
            {
                scope.SetTag("endpoint", httpContext.Request.Path);
                scope.SetTag("method", httpContext.Request.Method);
                scope.SetTag("ip", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
            });
        };
    });

    app.UseCors("SecureCorsPolicy");
    app.UseIpRateLimiting();

    // Hangfire Dashboard (optional, disabled by default for security)
    var hangfireDashboardEnabled = bool.TryParse(
        Environment.GetEnvironmentVariable("HANGFIRE_DASHBOARD_ENABLED")
        ?? builder.Configuration["Hangfire:DashboardEnabled"],
        out var dashEnabled) && dashEnabled;

    if (hangfireEnabled && hangfireDashboardEnabled)
    {
        var dashboardPath = Environment.GetEnvironmentVariable("HANGFIRE_DASHBOARD_PATH")
            ?? builder.Configuration["Hangfire:DashboardPath"]
            ?? "/hangfire";
        app.UseHangfireDashboard(dashboardPath, new DashboardOptions
        {
            Authorization = new[] { new HangfireAuthorizationFilter() }
        });
        Log.Information("Hangfire Dashboard enabled at {Path}", dashboardPath);
    }

    app.UseAuthorization();

    // Health Check Endpoints
    // Simple endpoint for load balancers/uptime monitors
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                timestamp = DateTime.UtcNow
            });
            await context.Response.WriteAsync(result);
        }
    });

    // Detailed endpoint with full diagnostics (for monitoring/debugging)
    app.MapHealthChecks("/health/detailed", new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    // Ready endpoint - checks if app is ready to serve traffic
    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("db") || check.Name == "api"
    });

    // Live endpoint - checks if app is alive (doesn't check dependencies)
    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = check => check.Name == "api"
    });

    app.MapControllers();

    Log.Information("WeddingPhotos API started successfully");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}