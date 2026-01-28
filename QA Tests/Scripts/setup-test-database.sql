-- ============================================
-- UNOPS Opportunity+ Test Database Setup
-- ============================================
-- Purpose: Create and configure PostgreSQL test database for integration testing
-- Run as: psql -U postgres -f setup-test-database.sql
-- Date: January 23, 2026
-- ============================================

-- Create test database
CREATE DATABASE unops_pao_test;

-- Create test user
CREATE USER pao_test_user WITH ENCRYPTED PASSWORD 'Test_Pass_123!';

-- Grant privileges on database
GRANT ALL PRIVILEGES ON DATABASE unops_pao_test TO pao_test_user;

-- Connect to the new database
\c unops_pao_test;

-- Set up schema
CREATE SCHEMA IF NOT EXISTS public;
GRANT ALL ON SCHEMA public TO pao_test_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO pao_test_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO pao_test_user;

-- Grant default privileges for future objects
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO pao_test_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO pao_test_user;

-- Success message
\echo '✅ Test database setup complete!'
\echo '   Database: unops_pao_test'
\echo '   User: pao_test_user'
\echo '   Password: Test_Pass_123!'
\echo ''
\echo 'Next step: Run EF Core migrations'
\echo '   dotnet ef database update --project UNOPS.PAO.UNOPSDataAccess --startup-project UNOPS.PAO.Server'
