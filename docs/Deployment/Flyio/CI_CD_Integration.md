# CI/CD Integration with GitHub Actions

## Overview

This guide covers setting up automated deployment pipelines for HarmoniHSE360 using GitHub Actions and Fly.io. The CI/CD pipeline provides automated testing, building, and deployment with zero-downtime deployments and proper environment management.

## 🏗️ Pipeline Architecture

### Workflow Overview

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Developer     │    │  GitHub Actions │    │    Fly.io       │
│   Push Code     │───►│   CI/CD Pipeline│───►│   Deployment    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                │
                                ▼
                       ┌─────────────────┐
                       │   Test Results  │
                       │   Build Status  │
                       │   Notifications │
                       └─────────────────┘
```

### Pipeline Stages

1. **Code Quality**: Linting, formatting, security scanning
2. **Testing**: Unit tests, integration tests, coverage reports
3. **Building**: Docker image creation and registry push
4. **Deployment**: Automated deployment to staging/production
5. **Verification**: Health checks and smoke tests
6. **Notification**: Status updates and alerts

## 🔧 Prerequisites

### Required Setup
- GitHub repository with HarmoniHSE360 code
- Fly.io account with applications created
- Fly.io API token with deployment permissions
- GitHub repository secrets configured

### Permissions Required
- GitHub repository admin access
- Fly.io organization member with deploy permissions
- Docker registry push permissions (GitHub Container Registry)

## 📝 Step 1: Generate Fly.io API Token

### 1.1 Create API Token

```bash
# Login to Fly.io CLI
flyctl auth login

# Create a new API token
flyctl auth token

# Copy the generated token - you'll need it for GitHub secrets
```

### 1.2 Verify Token Permissions

```bash
# Test token (replace YOUR_TOKEN with actual token)
export FLY_API_TOKEN="YOUR_TOKEN"
flyctl apps list

# Should list your applications
```

## 🔐 Step 2: Configure GitHub Secrets

### 2.1 Add Repository Secrets

Navigate to your GitHub repository → Settings → Secrets and variables → Actions

Add the following secrets:

| Secret Name | Value | Description |
|-------------|-------|-------------|
| `FLY_API_TOKEN` | `[your-fly-api-token]` | Fly.io API token for deployments |
| `CODECOV_TOKEN` | `[codecov-token]` | Code coverage reporting (optional) |
| `SLACK_WEBHOOK_URL` | `[slack-webhook]` | Slack notifications (optional) |

### 2.2 Configure Environment Variables

Add these as repository variables (not secrets):

| Variable Name | Value | Description |
|---------------|-------|-------------|
| `FLY_APP_NAME_STAGING` | `harmoni360-staging` | Staging app name |
| `FLY_APP_NAME_PRODUCTION` | `harmoni360-app` | Production app name |
| `DOCKER_REGISTRY` | `ghcr.io` | Container registry |

## 📋 Step 3: Review Existing Workflows

### 3.1 Current Workflow Files

The repository includes several pre-configured workflows:

```bash
.github/workflows/
├── deploy.yml              # Main CI/CD pipeline
├── ci-cd.yml               # Legacy pipeline (backup)
├── pr-checks.yml           # Pull request validation
├── security-scan.yml       # Security scanning
└── test-flyio-token.yml    # Token validation
```

### 3.2 Main Deployment Workflow

The primary workflow is in `.github/workflows/deploy.yml`:

**Triggers:**
- Push to `main` branch → Production deployment
- Push to `develop` branch → Staging deployment
- Pull requests → Testing only
- Manual workflow dispatch → Custom environment

**Jobs:**
1. `build-and-test` - Code quality and testing
2. `security-scan` - Vulnerability scanning
3. `docker-build` - Container image creation
4. `deploy-staging` - Staging environment deployment
5. `deploy-production` - Production environment deployment

## 🚀 Step 4: Environment Configuration

### 4.1 Staging Environment Setup

```bash
# Create staging application (if not exists)
flyctl apps create harmoni360-staging --org your-org

# Create staging database
flyctl postgres create --name harmoni360-staging-db --region sjc

# Create staging Redis
flyctl redis create --name harmoni360-staging-cache --region sjc

# Configure staging secrets
flyctl secrets set -a harmoni360-staging \
  ConnectionStrings__DefaultConnection="Host=harmoni360-staging-db.internal;Port=5432;Database=harmoni360;Username=postgres;Password=[staging-db-password]" \
  ConnectionStrings__Redis="[staging-redis-url]" \
  Jwt__Key="$(openssl rand -base64 32)" \
  ASPNETCORE_ENVIRONMENT="Staging"
```

### 4.2 Production Environment Setup

```bash
# Create production application (if not exists)
flyctl apps create harmoni360-app --org your-org

# Create production database
flyctl postgres create --name harmoni360-db --region sjc

# Create production Redis
flyctl redis create --name harmoni360-cache --region sjc

# Configure production secrets
flyctl secrets set -a harmoni360-app \
  ConnectionStrings__DefaultConnection="Host=harmoni360-db.internal;Port=5432;Database=harmoni360;Username=postgres;Password=[production-db-password]" \
  ConnectionStrings__Redis="[production-redis-url]" \
  Jwt__Key="$(openssl rand -base64 32)" \
  ASPNETCORE_ENVIRONMENT="Production"
```

### 4.3 Create Fly.io Configuration Files

#### Staging Configuration (`fly.staging.toml`)

```toml
app = "harmoni360-staging"
primary_region = "sjc"

[build]
  dockerfile = "Dockerfile.flyio"

[env]
  ASPNETCORE_ENVIRONMENT = "Staging"
  ASPNETCORE_URLS = "http://+:8080"

[[services]]
  internal_port = 8080
  protocol = "tcp"

  [[services.ports]]
    handlers = ["http"]
    port = 80
    force_https = true

  [[services.ports]]
    handlers = ["tls", "http"]
    port = 443

  [services.concurrency]
    type = "connections"
    hard_limit = 25
    soft_limit = 20

[[services.http_checks]]
  interval = "10s"
  grace_period = "5s"
  method = "GET"
  path = "/health"
  protocol = "http"
  timeout = "2s"

[mounts]
  source = "harmoni360_staging_uploads"
  destination = "/app/uploads"
```

#### Production Configuration (`fly.toml`)

```toml
app = "harmoni360-app"
primary_region = "sjc"

[build]
  dockerfile = "Dockerfile.flyio"

[env]
  ASPNETCORE_ENVIRONMENT = "Production"
  ASPNETCORE_URLS = "http://+:8080"

[[services]]
  internal_port = 8080
  protocol = "tcp"

  [[services.ports]]
    handlers = ["http"]
    port = 80
    force_https = true

  [[services.ports]]
    handlers = ["tls", "http"]
    port = 443

  [services.concurrency]
    type = "connections"
    hard_limit = 50
    soft_limit = 40

[[services.http_checks]]
  interval = "10s"
  grace_period = "5s"
  method = "GET"
  path = "/health"
  protocol = "http"
  timeout = "2s"

[mounts]
  source = "harmoni360_uploads"
  destination = "/app/uploads"
```

## 🔄 Step 5: Deployment Workflow

### 5.1 Automatic Deployments

#### Staging Deployment
```bash
# Push to develop branch triggers staging deployment
git checkout develop
git add .
git commit -m "feat: new feature for testing"
git push origin develop

# Monitor deployment
gh workflow run deploy.yml --ref develop
gh run watch
```

#### Production Deployment
```bash
# Push to main branch triggers production deployment
git checkout main
git merge develop
git push origin main

# Monitor deployment (requires approval)
gh workflow run deploy.yml --ref main
gh run watch
```

### 5.2 Manual Deployments

#### Using GitHub CLI
```bash
# Deploy to staging
gh workflow run deploy.yml --ref develop -f environment=staging

# Deploy to production
gh workflow run deploy.yml --ref main -f environment=production
```

#### Using GitHub Web Interface
1. Go to repository → Actions tab
2. Select "Harmoni360 CI/CD Pipeline"
3. Click "Run workflow"
4. Select branch and environment
5. Click "Run workflow"

### 5.3 Deployment Process

The deployment process includes:

1. **Pre-deployment Checks**
   - Code quality validation
   - Security scanning
   - Unit and integration tests
   - Docker image build and push

2. **Deployment Execution**
   - Zero-downtime deployment to Fly.io
   - Database migration execution
   - Health check verification
   - Rollback on failure

3. **Post-deployment Verification**
   - Application health checks
   - API endpoint testing
   - Performance monitoring
   - Notification sending

## 📊 Step 6: Monitoring and Notifications

### 6.1 Deployment Status Monitoring

```bash
# Check workflow status
gh run list --workflow=deploy.yml

# View specific run details
gh run view [run-id]

# View logs
gh run view [run-id] --log
```

### 6.2 Application Health Monitoring

```bash
# Check application status after deployment
flyctl status -a harmoni360-staging
flyctl status -a harmoni360-app

# View application logs
flyctl logs -a harmoni360-staging -f
flyctl logs -a harmoni360-app -f

# Test health endpoints
curl -f https://harmoni360-staging.fly.dev/health
curl -f https://harmoni360-app.fly.dev/health
```

### 6.3 Slack Notifications (Optional)

Configure Slack webhook for deployment notifications:

1. Create Slack webhook URL
2. Add `SLACK_WEBHOOK_URL` to GitHub secrets
3. Notifications will be sent for:
   - Successful deployments
   - Failed deployments
   - Security scan results

## 🛠️ Step 7: Troubleshooting CI/CD

### 7.1 Common Issues

#### Authentication Failures
```bash
# Verify Fly.io token
flyctl auth whoami

# Regenerate token if needed
flyctl auth token
```

#### Build Failures
```bash
# Check Docker build locally
docker build -f Dockerfile.flyio -t harmoni360:test .

# Test container locally
docker run -p 8080:8080 harmoni360:test
```

#### Deployment Failures
```bash
# Check Fly.io application status
flyctl status -a harmoni360-staging

# View deployment logs
flyctl logs -a harmoni360-staging

# Check database connectivity
flyctl postgres connect -a harmoni360-staging-db
```

### 7.2 Debugging Workflows

```bash
# Enable debug logging in workflow
# Add this to workflow environment:
ACTIONS_STEP_DEBUG: true
ACTIONS_RUNNER_DEBUG: true

# View detailed logs
gh run view [run-id] --log --verbose
```

### 7.3 Rollback Procedures

#### Automatic Rollback
The pipeline includes automatic rollback on health check failures.

#### Manual Rollback
```bash
# List recent releases
flyctl releases -a harmoni360-app

# Rollback to previous version
flyctl releases rollback [version] -a harmoni360-app
```

## 🔐 Step 8: Security Best Practices

### 8.1 Secret Management
- ✅ Use GitHub secrets for sensitive data
- ✅ Rotate API tokens regularly
- ✅ Use least-privilege access principles
- ✅ Monitor secret usage and access

### 8.2 Workflow Security
- ✅ Pin action versions to specific commits
- ✅ Use official actions when possible
- ✅ Review third-party actions before use
- ✅ Enable branch protection rules

### 8.3 Deployment Security
- ✅ Require approval for production deployments
- ✅ Use environment-specific configurations
- ✅ Implement proper health checks
- ✅ Monitor deployment logs

## 📈 Step 9: Performance Optimization

### 9.1 Build Optimization
- Use Docker layer caching
- Optimize Dockerfile for faster builds
- Use multi-stage builds
- Cache dependencies

### 9.2 Deployment Optimization
- Use blue-green deployments
- Implement proper health checks
- Configure appropriate timeouts
- Monitor resource usage

## 🎯 Next Steps

After setting up CI/CD:

1. **Environment Management**: Configure [Environment Management](./Environment_Management.md)
2. **Monitoring Setup**: Implement [Monitoring and Logging](./Monitoring_and_Logging.md)
3. **Performance Tuning**: Review [Scaling and Performance](./Scaling_and_Performance.md)
4. **Backup Strategy**: Set up [Backup and Recovery](./Backup_and_Recovery.md)

---

*Previous: [Getting Started](./Getting_Started.md) | Next: [Environment Management](./Environment_Management.md)*
