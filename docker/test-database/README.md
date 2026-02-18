# Pre-built Test Database for CI/CD

This directory contains the configuration for a pre-built PostgreSQL Docker image with all EF Core migrations already applied.

## Why?

The application has **376 migrations** that take ~25-30 minutes to run from scratch. By pre-baking these migrations into a Docker image, CI/CD database setup is reduced to **~30 seconds**.

| Approach | Database Setup Time | Total CI/CD Time |
|----------|--------------------:|------------------:|
| Run migrations each time | ~25-30 min | ~40 min |
| **Pre-built image** | **~30 sec** | **~10 min** |

## How It Works

1. **Build workflow** (`build-test-database.yml`) runs when migrations change
2. Workflow applies all migrations to a fresh PostgreSQL container
3. Schema is exported and baked into a new Docker image
4. Image is pushed to GitHub Container Registry
5. Playwright tests use this pre-built image instead of running migrations

## Files

```
docker/test-database/
├── Dockerfile              # Docker image definition
├── README.md               # This file
├── generate-schema.ps1     # Local script to generate schema
└── init-scripts/
    ├── 01-extensions.sql   # Creates pgvector/pg_trgm extensions
    └── 02-schema.sql       # Generated schema (not committed)
```

## Usage

### In GitHub Actions (Playwright Tests)

```yaml
services:
  postgres:
    image: ghcr.io/unops-itg/opportunityplus-testdb:latest
    env:
      POSTGRES_DB: TestDb
      POSTGRES_USER: test
      POSTGRES_PASSWORD: test
    ports:
      - 5432:5432
```

### Local Development

```bash
# Pull the pre-built image
docker pull ghcr.io/unops-itg/opportunityplus-testdb:latest

# Run it
docker run -d \
  --name testdb \
  -p 5432:5432 \
  ghcr.io/unops-itg/opportunityplus-testdb:latest

# Connect
psql -h localhost -p 5432 -U test -d TestDb
```

## Rebuilding the Image

### Automatic (Recommended)

The image is automatically rebuilt when:
- Migrations are added/modified in `UNOPS.PAO.DataAccess/Migrations/`
- Migrations are added/modified in `UNOPS.PAO.UNOPSDataAccess/Migrations/`
- Docker configuration changes in `docker/test-database/`

### Manual Trigger

1. Go to **Actions** → **Build Test Database Image**
2. Click **Run workflow**
3. Wait for completion (~30 min)

### Local Build

```powershell
# Generate the schema locally
cd docker/test-database
./generate-schema.ps1

# Build the image
docker build -t opportunityplus-testdb:local .

# Test it
docker run -d --name testdb -p 5432:5432 opportunityplus-testdb:local
```

## Maintenance

### When to Rebuild

Rebuild the image whenever:
- ✅ New migrations are added
- ✅ Existing migrations are modified
- ✅ SQL scripts in `init-scripts/` are updated
- ✅ PostgreSQL version is upgraded

### Troubleshooting

**Image outdated?**
```bash
# Check the image creation date
docker inspect ghcr.io/unops-itg/opportunityplus-testdb:latest --format='{{.Created}}'
```

**Missing tables/columns?**
- Trigger a manual rebuild of the image
- Or run pending migrations after container starts

**Extension errors?**
- The image uses `pgvector/pgvector:pg16` as base
- Extensions are created in `01-extensions.sql`

## Security Notes

- Test credentials (`test/test`) are only for CI/CD
- Never use this image in production
- The image contains no real data, only schema
