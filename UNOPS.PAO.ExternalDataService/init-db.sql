-- Initialize External Data Integration Service Monitoring Database
-- This script creates the basic schema structure for the monitoring database
-- Application data and reference tables are managed by the main UNOPS PAO database
-- 
-- NOTE: Monitoring tables (SyncExecutionLogs, SyncBatchLogs, etc.) are created 
-- automatically by the service on startup using simple table creation logic.

-- Create schemas for external data service monitoring
CREATE SCHEMA IF NOT EXISTS external;

-- Grant permissions for development user
GRANT ALL PRIVILEGES ON SCHEMA external TO dev;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA external TO dev;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA external TO dev;

-- Display setup completion message
DO $$ 
BEGIN 
    RAISE NOTICE 'External Data Integration Service monitoring database initialized successfully!';
    RAISE NOTICE 'Monitoring schemas created: external';
    RAISE NOTICE 'Monitoring tables will be created automatically by the service on first startup';
    RAISE NOTICE 'Service will use main UNOPS PAO database for application data and reference lookups';
END $$;
