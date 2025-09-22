using Serilog;
using UNOPS.PAO.ExternalDataService.Infrastructure.Database;
using UNOPS.PAO.ExternalDataService.Infrastructure.Utilities;
using UNOPS.PAO.ExternalDataService.Services;
using UNOPS.PAO.ExternalDataService.Services.Configuration;
using UNOPS.PAO.ExternalDataService.Services.Database;
using UNOPS.PAO.ExternalDataService.Services.DataSource;
using UNOPS.PAO.ExternalDataService.Services.Sync;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/external-data-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddControllersWithViews(); // For admin interface

// Configure database contexts
var monitoringConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

var appConnectionString = builder.Configuration.GetConnectionString("ApplicationConnection") 
    ?? throw new InvalidOperationException("Connection string 'ApplicationConnection' not found.");

// External Data Sync context removed - now using raw SQL for all operations

// Simple monitoring database service for table creation and basic operations
builder.Services.AddScoped<IMonitoringDatabaseService>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<MonitoringDatabaseService>>();
    return new MonitoringDatabaseService(monitoringConnectionString, logger);
});

// Simple database service for application data operations
// Uses raw database connections - no Entity Framework overhead
builder.Services.AddScoped<IApplicationDatabaseService>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<ApplicationDatabaseService>>();
    return new ApplicationDatabaseService(appConnectionString, logger);
});

// Configuration services
builder.Services.AddScoped<ConfigurationValidator>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();

// Data source services
builder.Services.AddScoped<BigQuerySourceService>();
builder.Services.AddScoped<IDataSourceService>(provider => 
    provider.GetRequiredService<BigQuerySourceService>());
builder.Services.AddScoped<DataSourceFactory>();

// Database services
builder.Services.AddScoped<IDynamicTableService, DynamicTableService>();
builder.Services.AddScoped<IForeignKeyResolver, ForeignKeyResolver>();

// Sync services
builder.Services.AddScoped<ISyncLoggingService, SyncLoggingService>();
builder.Services.AddScoped<ISyncProcessor, SyncProcessor>();
builder.Services.AddScoped<ISyncOrchestrator, SyncOrchestrator>();
builder.Services.AddScoped<IExternalDataSyncService, ExternalDataSyncService>();

// Utility services
builder.Services.AddScoped<DataTypeMapper>();
builder.Services.AddScoped<TransformationEngine>();
builder.Services.AddScoped<SqlHelper>();

// Configure BigQuery client if credentials are provided
var bigQueryProjectId = builder.Configuration["BigQuery:ProjectId"];
if (!string.IsNullOrEmpty(bigQueryProjectId))
{
    var useDefaultCredentials = builder.Configuration.GetValue<bool>("BigQuery:UseDefaultCredentials", true);
    var credentialsPath = builder.Configuration["BigQuery:CredentialsPath"];
    
    if (useDefaultCredentials)
    {
        // Use default credentials (environment variables, metadata server, etc.)
        Log.Information("Using default BigQuery credentials for project {ProjectId}", bigQueryProjectId);
    }
    else if (!string.IsNullOrEmpty(credentialsPath) && File.Exists(credentialsPath))
    {
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);
        Log.Information("Using BigQuery credentials from file: {CredentialsPath}", credentialsPath);
    }
    else
    {
        Log.Warning("BigQuery credentials not properly configured. Service account key or default credentials required.");
    }
}

// Configure CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("monitoring-database", () =>
    {
        // Simple database connection test for monitoring database
        try
        {
            using var connection = new Npgsql.NpgsqlConnection(monitoringConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            command.ExecuteScalar();
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy(ex.Message, ex);
        }
    })
    .AddCheck("application-database", () =>
    {
        // Simple database connection test for application database
        try
        {
            using var connection = new Npgsql.NpgsqlConnection(appConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            command.ExecuteScalar();
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy(ex.Message, ex);
        }
    });

// Configure API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "UNOPS External Data Integration Service", 
        Version = "v1",
        Description = "Service for synchronizing external data sources into UNOPS PAO database"
    });
    
    // Include XML comments for better API documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configure application options
builder.Services.Configure<ExternalDataServiceOptions>(
    builder.Configuration.GetSection("ExternalDataService"));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "External Data Integration Service V1");
        c.RoutePrefix = "swagger";
    });
    
    app.UseCors("DevelopmentCors");
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    
    // Add security headers for non-development environments
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
    }
    
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles(); // For admin interface assets

app.UseRouting();

// Basic authentication for admin interface in production
if (!app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/admin"))
        {
            // In production, you might want to add proper authentication here
            // For now, we'll just add a simple check for API key or basic auth
            var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
            var configuredApiKey = app.Configuration["AdminApiKey"];
            
            if (string.IsNullOrEmpty(apiKey) || apiKey != configuredApiKey)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }
        }
        
        await next();
    });
}

app.MapControllers();
app.MapHealthChecks("/health");

// Add a simple root endpoint
app.MapGet("/", () => new
{
    service = "UNOPS External Data Integration Service",
    version = "1.0.0",
    status = "Running",
    environment = app.Environment.EnvironmentName,
    timestamp = DateTime.UtcNow,
    endpoints = new
    {
        api = "/api/sync",
        admin = "/admin",
        health = "/health",
        swagger = "/swagger"
    }
});

// Error handling endpoint
app.MapGet("/error", () => Results.Problem("An error occurred processing your request."));

// Ensure databases are setup and tables exist
try
{
    using var scope = app.Services.CreateScope();
    
    // Ensure monitoring database tables exist (simple table creation)
    var monitoringDbService = scope.ServiceProvider.GetRequiredService<IMonitoringDatabaseService>();
    await monitoringDbService.EnsureTablesExistAsync();
    Log.Information("Monitoring database tables verified/created successfully");
    
    // Test application database connection
    var appDbService = scope.ServiceProvider.GetRequiredService<IApplicationDatabaseService>();
    await appDbService.ExecuteScalarAsync<int>("SELECT 1");
    Log.Information("Application database connection verified successfully");
    
    // Load and validate configurations on startup
    var configService = scope.ServiceProvider.GetRequiredService<IConfigurationService>();
    var configurations = await configService.LoadAllConfigurationsAsync();
    
    Log.Information("Loaded {ConfigCount} sync configurations on startup", configurations.Count());
    
    foreach (var config in configurations)
    {
        if (config.Metadata.Enabled)
        {
            Log.Information("Enabled configuration: {ConfigName} - {Description}", 
                config.Metadata.Name, config.Metadata.Description);
        }
        else
        {
            Log.Information("Disabled configuration: {ConfigName}", config.Metadata.Name);
        }
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "An error occurred during startup");
    throw;
}

Log.Information("UNOPS External Data Integration Service starting up");
Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
Log.Information("Configuration path: {ConfigPath}", 
    app.Configuration["ExternalDataService:ConfigurationPath"] ?? "config");

app.Run();

// Configuration options class
public class ExternalDataServiceOptions
{
    public string ConfigurationPath { get; set; } = "config";
    public bool Enabled { get; set; } = true;
    public int CheckIntervalMinutes { get; set; } = 60;
    public bool AutoCreateTables { get; set; } = false;
    public bool TestMode { get; set; } = false;
    public string LogLevel { get; set; } = "Information";
}
