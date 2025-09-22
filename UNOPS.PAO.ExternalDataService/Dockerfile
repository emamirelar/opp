# External Data Integration Service - Production Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Install required system dependencies
RUN apt-get update && apt-get install -y \
    curl \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Create directories with proper permissions
RUN mkdir -p /app/logs /app/config && \
    chown -R appuser:appuser /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project file and restore dependencies
COPY ["UNOPS.PAO.ExternalDataService.csproj", "."]
RUN dotnet restore "UNOPS.PAO.ExternalDataService.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src"
RUN dotnet build "UNOPS.PAO.ExternalDataService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "UNOPS.PAO.ExternalDataService.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false \
    --no-restore

FROM base AS final
WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Copy configuration templates
COPY --chown=appuser:appuser config-templates ./config-templates/

# Create sample config directory structure
RUN mkdir -p config/development config/staging config/production && \
    chown -R appuser:appuser config

# Set environment variables for Cloud Run
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true

# Switch to non-root user
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "UNOPS.PAO.ExternalDataService.dll"]
