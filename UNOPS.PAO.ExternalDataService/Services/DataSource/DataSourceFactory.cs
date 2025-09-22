using UNOPS.PAO.ExternalDataService.Models.Configuration;

namespace UNOPS.PAO.ExternalDataService.Services.DataSource;

public class DataSourceFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataSourceFactory> _logger;

    public DataSourceFactory(IServiceProvider serviceProvider, ILogger<DataSourceFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public IDataSourceService CreateDataSourceService(SourceConfiguration sourceConfig)
    {
        return sourceConfig.Type.ToLower() switch
        {
            "bigquery" => _serviceProvider.GetRequiredService<BigQuerySourceService>(),
            _ => throw new NotSupportedException($"Data source type '{sourceConfig.Type}' is not supported")
        };
    }

    public IEnumerable<string> GetSupportedDataSourceTypes()
    {
        return new[] { "bigquery" };
    }
}
