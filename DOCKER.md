# Docker Configuration Guide

This document explains the Docker setup for the CQRS Library Management System, including development and production configurations.

## 📋 Table of Contents

- [Overview](#overview)
- [File Structure](#file-structure)
- [Development Environment](#development-environment)
- [Production Environment](#production-environment)
- [Environment Variables](#environment-variables)
- [Debugging](#debugging)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

## 🎯 Overview

The project includes separate Docker configurations for **development** and **production** environments:

| Environment | Dockerfile | Docker Compose | Features |
|-------------|------------|----------------|----------|
| **Development** | `Dockerfile.dev` | `docker-compose.override.yml` | Hot reload, debugging, verbose logging |
| **Production** | `Dockerfile.prod` | `docker-compose.prod.yml` | Optimized, secure, minimal image |

## 📁 File Structure

```
cqrs-library/
├── Dockerfile.dev              # Development container
├── Dockerfile.prod             # Production container
├── docker-compose.yml          # Base configuration
├── docker-compose.override.yml # Development overrides (auto-loaded)
├── docker-compose.prod.yml     # Production configuration
├── .env.example                # Environment variables template
├── .dockerignore               # Docker ignore rules
└── traefik/
    ├── traefik.yml             # Traefik static configuration
    └── dynamic.yml             # Traefik dynamic configuration (middlewares)
```

## 🔧 Development Environment

### Features

- ✅ **Hot Reload** - Changes reflected instantly without rebuild
- ✅ **Debugger Support** - VS Code remote debugging via vsdbg
- ✅ **Volume Mounting** - Source code mounted for live editing
- ✅ **Verbose Logging** - Detailed logs for debugging
- ✅ **Fast Startup** - No optimization, quick iteration

### Usage

**Start Development Environment:**

```bash
# Default - automatically uses docker-compose.override.yml
docker-compose up

# Or explicitly
docker-compose -f docker-compose.yml -f docker-compose.override.yml up

# Build and start
docker-compose up --build

# Detached mode
docker-compose up -d
```

**Stop Development Environment:**

```bash
docker-compose down

# Remove volumes (database data)
docker-compose down -v
```

### Development Configuration

**Dockerfile.dev:**
- Based on `mcr.microsoft.com/dotnet/sdk:10.0`
- Includes VS debugger (`vsdbg`)
- Uses `dotnet watch run` for hot reload
- Exposes ports: 8080, 5000, 5001
- Development environment variables

**docker-compose.override.yml:**
- Source code volume mounting
- Hot reload enabled
- PostgreSQL query logging
- Interactive terminal (stdin_open, tty)
- Development health checks

### Hot Reload

Changes to `.cs` files are automatically detected and the application restarts:

```bash
# Make changes to any C# file
# Application automatically restarts
# Check logs
docker-compose logs -f library-api
```

### Debugging with VS Code

1. **Start containers:**
   ```bash
   docker-compose up
   ```

2. **Attach debugger:**
   - Open VS Code
   - Install "Remote - Containers" extension
   - Attach to running container
   - Set breakpoints
   - Debug!

**VS Code launch.json for debugging:**
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Docker: Attach to .NET",
      "type": "coreclr",
      "request": "attach",
      "processId": "${command:pickRemoteProcess}",
      "pipeTransport": {
        "pipeCwd": "${workspaceRoot}",
        "pipeProgram": "docker",
        "pipeArgs": ["exec", "-i", "cqrs-library-api-dev"],
        "debuggerPath": "/vsdbg/vsdbg",
        "quoteArgs": false
      },
      "sourceFileMap": {
        "/app": "${workspaceFolder}"
      }
    }
  ]
}
```

## 🚀 Production Environment

### Features

- ✅ **Multi-stage Build** - Minimal final image size
- ✅ **Security Hardened** - Non-root user, read-only filesystem
- ✅ **Optimized** - Release build, no dev tools
- ✅ **Health Checks** - Automatic container health monitoring
- ✅ **Resource Limits** - CPU and memory constraints
- ✅ **Logging** - Structured logging with rotation
- ✅ **Traefik Reverse Proxy** - Automatic service discovery, SSL, rate limiting

### Usage

**Start Production Environment:**

```bash
# Using production configuration
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# Build from scratch
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up --build -d

# View logs
docker-compose -f docker-compose.yml -f docker-compose.prod.yml logs -f
```

**Stop Production Environment:**

```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml down

# Keep volumes (preserve database)
docker-compose -f docker-compose.yml -f docker-compose.prod.yml down
```

### Production Configuration

**Dockerfile.prod:**
- Multi-stage build (build → publish → runtime)
- Based on `mcr.microsoft.com/dotnet/aspnet:10.0-alpine`
- Non-root user (appuser:1000)
- Read-only filesystem
- Health check endpoint
- Minimal image size (~105MB)

**docker-compose.prod.yml:**
- Production environment variables
- Resource limits (CPU: 1 core, Memory: 512MB)
- Restart policy (on-failure)
- Security options (no-new-privileges)
- Logging configuration (10MB max, 3 files)
- Traefik reverse proxy with Let's Encrypt

### Image Size Comparison

| Image | Size | Purpose |
|-------|------|---------|
| Development (`sdk:10.0`) | ~1.2GB | Debugging, hot reload |
| Production (`aspnet:10.0-alpine`) | ~105MB | Optimized runtime |

### Security Features

**Non-root User:**
```dockerfile
RUN adduser -D -u 1000 appuser
USER appuser
```

**Read-only Filesystem:**
```yaml
read_only: true
tmpfs:
  - /tmp
```

**Security Options:**
```yaml
security_opt:
  - no-new-privileges:true
```

### Traefik Reverse Proxy

The production setup includes Traefik v3.0 as a modern reverse proxy with:

- ✅ **Automatic Service Discovery** - Docker label-based configuration
- ✅ **Let's Encrypt SSL/TLS** - Automatic certificate management
- ✅ **HTTP to HTTPS Redirect** - Automatic secure redirect
- ✅ **Rate Limiting** - Configurable request limits
- ✅ **Compression** - Automatic gzip compression
- ✅ **Security Headers** - HSTS, XSS protection, frame deny
- ✅ **Dashboard** - Web UI for monitoring (with authentication)
- ✅ **Metrics** - Prometheus metrics endpoint

**Configuration Files:**

Traefik uses two configuration files:

1. **`traefik/traefik.yml`** - Static configuration (entry points, providers, certificates)
2. **`traefik/dynamic.yml`** - Dynamic configuration (middlewares, TLS options)

**Quick Start:**

1. Update `.env` file with your domain:
```bash
DOMAIN=yourdomain.com
LETSENCRYPT_EMAIL=admin@yourdomain.com
TRAEFIK_LOG_LEVEL=INFO
```

2. Start production environment:
```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

3. Traefik automatically:
   - Discovers the library-api service via Docker labels
   - Requests Let's Encrypt certificates
   - Configures HTTPS with automatic redirect
   - Applies security middlewares

**Access Points:**
- HTTP: `http://yourdomain.com` (port 80, redirects to HTTPS)
- HTTPS: `https://yourdomain.com/api` (port 443)
- Dashboard: `http://localhost:8081` or `https://traefik.yourdomain.com`
- Direct API: `http://localhost:8080` (bypasses Traefik)

**Dashboard Authentication:**

Default credentials (change in production!):
- Username: `admin`
- Password: `admin`

Generate new credentials:
```bash
# Install htpasswd (Apache utils)
apt-get install apache2-utils  # Ubuntu/Debian
yum install httpd-tools         # CentOS/RHEL

# Generate password hash
htpasswd -nb admin your-password

# Update docker-compose.prod.yml with the output
```

**Let's Encrypt SSL:**

For production with real domain:
1. Point your domain DNS to server IP
2. Set `DOMAIN` in `.env`
3. Set `LETSENCRYPT_EMAIL` in `.env`
4. Start containers - certificates are automatic!

For testing (staging):
- Uncomment staging CA server in `traefik/traefik.yml`
- This avoids Let's Encrypt rate limits during testing

**Middlewares:**

Traefik applies these middlewares automatically:
- `security-headers`: HSTS, XSS protection, frame deny
- `rate-limit`: 100 requests/second average, 50 burst
- `compress`: Gzip compression
- `api-stripprefix`: Removes `/api` prefix before forwarding

**Monitoring:**

View Traefik dashboard:
```bash
# Local development
open http://localhost:8081

# Production (with domain)
open https://traefik.yourdomain.com
```

Check Prometheus metrics:
```bash
curl http://localhost:8081/metrics
```

## 🔑 Environment Variables

### Using .env File

Create a `.env` file from the template:

```bash
cp .env.example .env
```

Edit `.env` with your values:

```env
POSTGRES_DB=librarydb
POSTGRES_USER=libraryuser
POSTGRES_PASSWORD=your-secure-password
POSTGRES_PORT=5432
API_PORT=8080
ASPNETCORE_ENVIRONMENT=Production
NEED_SEED=false
```

Docker Compose automatically loads `.env` file.

### Environment Variables Reference

| Variable | Description | Development | Production |
|----------|-------------|-------------|------------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Development` | `Production` |
| `ASPNETCORE_URLS` | Binding URLs | `http://+:8080` | `http://+:8080` |
| `ConnectionStrings__DefaultConnection` | Database connection | Auto-configured | From .env |
| `POSTGRES_DB` | Database name | `librarydb` | From .env |
| `POSTGRES_USER` | Database user | `libraryuser` | From .env |
| `POSTGRES_PASSWORD` | Database password | `librarypass` | From .env |
| `NEED_SEED` | Enable seeding | `false` | `false` |
| `DOMAIN` | Domain for Traefik | N/A | From .env |
| `LETSENCRYPT_EMAIL` | Let's Encrypt email | N/A | From .env |
| `TRAEFIK_LOG_LEVEL` | Traefik log level | N/A | `INFO` |
| `DOTNET_USE_POLLING_FILE_WATCHER` | Hot reload (dev) | `true` | N/A |
| `DOTNET_EnableDiagnostics` | Diagnostics (prod) | N/A | `0` |

### Override Variables

**Development:**
```bash
# Set before running docker-compose
export NEED_SEED=true
docker-compose up
```

**Production:**
```bash
# Using .env file
echo "NEED_SEED=true" >> .env
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

## 🐛 Debugging

### Attach to Running Container

**Using Docker CLI:**
```bash
# Start shell in container
docker exec -it cqrs-library-api-dev /bin/bash

# View logs
docker logs -f cqrs-library-api-dev

# Monitor resources
docker stats cqrs-library-api-dev
```

**Using Docker Compose:**
```bash
# View logs
docker-compose logs -f library-api

# Execute command
docker-compose exec library-api /bin/bash

# View environment
docker-compose exec library-api env
```

### Remote Debugging with VS Code

The development container includes `vsdbg` for remote debugging:

1. Start container: `docker-compose up`
2. Attach VS Code debugger (see configuration above)
3. Set breakpoints
4. Debug!

### Database Debugging

**Connect to PostgreSQL:**
```bash
# Using Docker Compose
docker-compose exec postgres psql -U libraryuser -d librarydb

# Using Docker
docker exec -it cqrs-library-db psql -U libraryuser -d librarydb
```

**Query Examples:**
```sql
-- Check authors
SELECT COUNT(*) FROM "Authors";

-- Check books
SELECT * FROM "Books" LIMIT 10;

-- Check borrowed books
SELECT b."Title", r."Email", b."BorrowedAt", b."DueDate"
FROM "Books" b
JOIN "Readers" r ON b."BorrowedByReaderId" = r."Id"
WHERE b."BorrowedByReaderId" IS NOT NULL;
```

## ✅ Best Practices

### Development

1. **Always use docker-compose.override.yml** for local development
2. **Don't commit .env** - use .env.example as template
3. **Use volume mounts** for hot reload
4. **Enable query logging** for database debugging
5. **Run tests inside containers** for consistency

### Production

1. **Use docker-compose.prod.yml** for production
2. **Set resource limits** to prevent resource exhaustion
3. **Enable health checks** for automatic recovery
4. **Use secrets management** (Docker secrets, Kubernetes secrets)
5. **Implement proper logging** with log rotation
6. **Regular backups** of database volumes
7. **Monitor container health** and performance
8. **Use environment-specific .env files**
9. **Enable SSL/TLS** with Let's Encrypt via Traefik
10. **Monitor Traefik dashboard** for traffic and health

### Security

1. **Use non-root users** in containers
2. **Enable read-only filesystems** where possible
3. **Set security options** (no-new-privileges)
4. **Scan images** for vulnerabilities
5. **Keep base images updated**
6. **Don't expose unnecessary ports**
7. **Use secrets** instead of environment variables for sensitive data

## 🔧 Troubleshooting

### Container Won't Start

**Check logs:**
```bash
docker-compose logs library-api
```

**Common causes:**
- Port already in use
- Database not ready
- Missing environment variables
- Configuration errors

### Hot Reload Not Working

**Solutions:**
1. Check volume mounts in docker-compose.override.yml
2. Verify `DOTNET_USE_POLLING_FILE_WATCHER=true`
3. Restart container: `docker-compose restart library-api`
4. Check file permissions

### Database Connection Issues

**Check health:**
```bash
docker-compose ps postgres
```

**Verify connection:**
```bash
docker-compose exec postgres pg_isready -U libraryuser
```

**Reset database:**
```bash
docker-compose down -v
docker-compose up -d
```

### Build Failures

**Clear cache:**
```bash
docker-compose down
docker system prune -a
docker-compose build --no-cache
```

**Check Dockerfile syntax:**
```bash
docker build -f Dockerfile.dev -t test .
```

### Performance Issues

**Check resource usage:**
```bash
docker stats
```

**Increase limits in docker-compose.prod.yml:**
```yaml
deploy:
  resources:
    limits:
      cpus: '2'
      memory: 1G
```

## 📊 Monitoring

### Container Health

**Check status:**
```bash
docker-compose ps
```

**Health check logs:**
```bash
docker inspect --format='{{json .State.Health}}' cqrs-library-api-prod
```

### Logs

**View all logs:**
```bash
docker-compose logs
```

**Tail logs:**
```bash
docker-compose logs -f --tail=100 library-api
```

**Export logs:**
```bash
docker-compose logs > logs.txt
```

## 🎯 Common Commands

### Development

```bash
# Start
docker-compose up

# Start with build
docker-compose up --build

# Stop
docker-compose down

# Rebuild single service
docker-compose build library-api

# View logs
docker-compose logs -f library-api

# Shell access
docker-compose exec library-api /bin/bash
```

### Production

```bash
# Start
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# Stop
docker-compose -f docker-compose.yml -f docker-compose.prod.yml down

# View logs
docker-compose -f docker-compose.yml -f docker-compose.prod.yml logs -f

# Update and restart
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build

# Scale API (multiple instances)
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d --scale library-api=3
```

## 📚 Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [.NET in Docker](https://learn.microsoft.com/en-us/dotnet/core/docker/introduction)
- [Docker Security Best Practices](https://docs.docker.com/engine/security/)

---

For more information, see [README.md](README.md) and [CLAUDE.md](CLAUDE.md).
