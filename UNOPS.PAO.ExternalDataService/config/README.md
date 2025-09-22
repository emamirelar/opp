# External Data Integration - Configuration Files

This directory contains YAML configuration files that define how external data sources are synchronized into the UNOPS PAO database.

## Directory Structure

```
config/
├── development/          # Development and testing configurations
│   ├── partners-test.yaml
│   └── projects-test.yaml
├── staging/             # Staging environment configurations
├── production/          # Production configurations (deploy separately)
│   └── partners-prod.yaml
└── README.md           # This file
```

## Configuration File Format

Each configuration file defines:

- **Metadata**: Name, description, schedule, and execution parameters
- **Source**: BigQuery connection details and data extraction query
- **Destination**: Target table schema and field mappings
- **Transformations**: Data cleanup and formatting rules
- **Foreign Key Mappings**: How to resolve external references to internal IDs
- **Validation**: Data quality checks and business rules
- **Error Handling**: How to handle various error scenarios

## Getting Started

1. **Copy a template**: Start with `../config-templates/basic-sync-template.yaml`
2. **Customize for your data source**: Update connection details and query
3. **Test locally**: Place in `development/` folder and run sync manually
4. **Deploy to production**: Move to `production/` folder when ready

## Environment-Specific Settings

### Development
- Small test datasets with LIMIT clauses
- Detailed logging enabled
- Auto-table creation enabled
- Relaxed validation rules

### Staging
- Production-like data volumes
- Standard logging levels
- Production validation rules
- Manual table creation

### Production
- Full datasets with incremental sync
- Minimal logging for performance
- Strict validation and error handling
- Comprehensive monitoring and alerting

## Configuration Examples

### Basic Partner Sync
```yaml
metadata:
  name: "Partner Data Sync"
  enabled: true
  schedule_cron: "0 6 * * *"  # Daily at 6 AM

source:
  type: "bigquery"
  connection:
    project_id: "your-project-id"
    use_environment_auth: true
  query: |
    SELECT partner_id, partner_name, email
    FROM partners
    WHERE updated_at >= @last_sync_date
```

### Field Transformations
```yaml
field_mappings:
  - source_field: "email"
    destination_field: "Email"
    data_type: "varchar(255)"
    transformations:
      - type: "lowercase"
      - type: "validate_email"
```

### Foreign Key Resolution
```yaml
foreign_key_mappings:
  - source_field: "country_code"
    lookup_table: "Countries"
    lookup_field: "CountryCode"
    destination_field: "CountryId"
    on_lookup_fail: LogWarning
```

## Available Transformations

- **Text**: trim, uppercase, lowercase, title_case, normalize_whitespace
- **Validation**: validate_email, format_phone
- **Numeric**: round, clamp
- **Date**: format_date
- **Mapping**: status_mapping (with custom mapping dictionary)
- **Conditional**: default_if_empty, replace, regex_replace

## Scheduling

Use cron expressions for automatic scheduling:
- `0 6 * * *` - Daily at 6 AM
- `0 */4 * * *` - Every 4 hours
- `0 0 * * 1` - Weekly on Monday at midnight
- `0 0 1 * *` - Monthly on the 1st at midnight

## Security Best Practices

1. **Never commit production credentials** to version control
2. **Use environment variables** for sensitive configuration
3. **Limit data access** with appropriate SQL WHERE clauses
4. **Enable logging** but avoid logging sensitive data
5. **Use least privilege** BigQuery service accounts
6. **Validate input data** with quality checks
7. **Monitor sync operations** and set up alerts

## Troubleshooting

### Common Issues

1. **Configuration not loading**
   - Check YAML syntax with an online validator
   - Verify file is in the correct directory
   - Check application logs for specific errors

2. **BigQuery connection failed**
   - Verify project ID and credentials
   - Check service account permissions
   - Test with a simple query first

3. **Foreign key lookup failed**
   - Verify lookup table exists
   - Check lookup field values match exactly
   - Use case-insensitive matching if needed

4. **Data transformation errors**
   - Test transformations with sample data
   - Check transformation parameter syntax
   - Use default values for problematic fields

### Debug Mode

For troubleshooting, enable detailed logging:

```yaml
logging:
  log_level: "Debug"
  log_sql_queries: true
  log_record_details: true
```

### Manual Testing

Execute a configuration manually via the admin interface:
1. Open `https://your-service.com/admin`
2. Find your configuration
3. Click "Execute" to run manually
4. Check logs and results

## Support

For questions about external data integration:
- Check the service logs at `/logs`
- Use the admin interface at `/admin`
- Review API documentation at `/swagger`
- Contact the data integration team

## Configuration Schema

For the complete configuration schema and validation rules, see the JSON Schema definition in the codebase at:
`Services/Configuration/ConfigurationValidator.cs`
