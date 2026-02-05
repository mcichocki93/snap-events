# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/WeddingPhotos.Api/WeddingPhotos.Api.csproj", "WeddingPhotos.Api/"]
COPY ["src/WeddingPhotos.Domain/WeddingPhotos.Domain.csproj", "WeddingPhotos.Domain/"]
COPY ["src/WeddingPhotos.Infrastructure/WeddingPhotos.Infrastructure.csproj", "WeddingPhotos.Infrastructure/"]
RUN dotnet restore "WeddingPhotos.Api/WeddingPhotos.Api.csproj"

# Copy everything else and build
COPY src/ .
RUN dotnet build "WeddingPhotos.Api/WeddingPhotos.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "WeddingPhotos.Api/WeddingPhotos.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage - use Jammy (Ubuntu 22.04) for better SSL compatibility
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS final
WORKDIR /app

# Install ca-certificates for SSL
RUN apt-get update && apt-get install -y --no-install-recommends ca-certificates curl && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Create directories
RUN mkdir -p /app/logs /app/secrets && \
    chown -R appuser:appuser /app

# Copy published app
COPY --from=publish /app/publish .

# Set ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 5000

# Health check
HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

# Set environment variables defaults
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "WeddingPhotos.Api.dll"]
