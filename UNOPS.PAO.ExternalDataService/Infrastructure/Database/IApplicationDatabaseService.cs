using System.Data;
using System.Data.Common;

namespace UNOPS.PAO.ExternalDataService.Infrastructure.Database;

/// <summary>
/// Simple database service for application database operations
/// Provides raw database access without Entity Framework overhead
/// </summary>
public interface IApplicationDatabaseService
{
    /// <summary>
    /// Gets a database connection to the application database
    /// </summary>
    Task<DbConnection> GetConnectionAsync();
    
    /// <summary>
    /// Executes a raw SQL query and returns results
    /// </summary>
    Task<IEnumerable<T>> ExecuteQueryAsync<T>(string sql, object? parameters = null);
    
    /// <summary>
    /// Executes a raw SQL command (INSERT, UPDATE, DELETE, CREATE TABLE, etc.)
    /// </summary>
    Task<int> ExecuteCommandAsync(string sql, object? parameters = null);
    
    /// <summary>
    /// Begins a database transaction
    /// </summary>
    Task<DbTransaction> BeginTransactionAsync();
    
    /// <summary>
    /// Executes a query and returns the first result or default
    /// </summary>
    Task<T?> ExecuteScalarAsync<T>(string sql, object? parameters = null);
}

/// <summary>
/// Simple implementation using Npgsql directly
/// </summary>
public class ApplicationDatabaseService : IApplicationDatabaseService
{
    private readonly string _connectionString;
    private readonly ILogger<ApplicationDatabaseService> _logger;

    public ApplicationDatabaseService(string connectionString, ILogger<ApplicationDatabaseService> logger)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _logger = logger;
    }

    public async Task<DbConnection> GetConnectionAsync()
    {
        var connection = new Npgsql.NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }

    public async Task<IEnumerable<T>> ExecuteQueryAsync<T>(string sql, object? parameters = null)
    {
        using var connection = await GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        
        if (parameters != null)
        {
            AddParameters(command, parameters);
        }

        var results = new List<T>();
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            if (typeof(T) == typeof(int) && reader.FieldCount == 1)
            {
                results.Add((T)(object)reader.GetInt32(0));
            }
            else if (typeof(T) == typeof(string) && reader.FieldCount == 1)
            {
                results.Add((T)(object)reader.GetString(0));
            }
            else
            {
                // For complex types, you'd need more sophisticated mapping
                throw new NotSupportedException($"Type {typeof(T)} not supported for simple queries. Use ExecuteQueryAsync with custom mapping.");
            }
        }

        return results;
    }

    public async Task<int> ExecuteCommandAsync(string sql, object? parameters = null)
    {
        using var connection = await GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandTimeout = 300; // 5 minutes like the DbContext
        
        if (parameters != null)
        {
            AddParameters(command, parameters);
        }

        return await command.ExecuteNonQueryAsync();
    }

    public async Task<DbTransaction> BeginTransactionAsync()
    {
        var connection = await GetConnectionAsync();
        return await connection.BeginTransactionAsync();
    }

    public async Task<T?> ExecuteScalarAsync<T>(string sql, object? parameters = null)
    {
        using var connection = await GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        
        if (parameters != null)
        {
            AddParameters(command, parameters);
        }

        var result = await command.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value) 
            return default;
        
        try
        {
            // Handle type conversion, especially for PostgreSQL's long/int differences
            if (typeof(T) == typeof(int) && result is long longValue)
            {
                return (T)(object)(int)longValue;
            }
            return (T)Convert.ChangeType(result, typeof(T));
        }
        catch (InvalidCastException)
        {
            // Fallback for direct cast if conversion fails
            return (T)result;
        }
    }

    private void AddParameters(DbCommand command, object parameters)
    {
        var properties = parameters.GetType().GetProperties();
        foreach (var property in properties)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = $"@{property.Name}";
            parameter.Value = property.GetValue(parameters) ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }
    }
}
