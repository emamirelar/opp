# =============================================================================
# Pre-built PostgreSQL Test Database for CI/CD
# =============================================================================
# This image contains all EF Core migrations pre-applied, reducing CI/CD
# database setup time from ~30 minutes to ~30 seconds.
#
# Build: docker build -t opportunityplus-testdb:latest .
# Push:  docker push ghcr.io/unops-itg/opportunityplus-testdb:latest
# =============================================================================

FROM pgvector/pgvector:pg16

# Set environment variables
ENV POSTGRES_DB=TestDb
ENV POSTGRES_USER=test
ENV POSTGRES_PASSWORD=test

# Copy initialization scripts
COPY init-scripts/ /docker-entrypoint-initdb.d/

# Expose PostgreSQL port
EXPOSE 5432

# Health check
HEALTHCHECK --interval=10s --timeout=5s --start-period=30s --retries=5 \
    CMD pg_isready -U test -d TestDb || exit 1
