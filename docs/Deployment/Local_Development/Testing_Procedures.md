# Local Development Testing Procedures

## Overview

This guide describes how to verify that the HarmoniHSE360 stack is running correctly in the local Docker environment.

## 🔍 Health Checks

### Automated Health Checks
```powershell
# Verify all containers are running
docker-compose -f docker-compose.local.yml ps

# Run bundled health check script
.\local-dev\scripts\health-check.ps1

# Inspect recent logs
docker-compose -f docker-compose.local.yml logs --tail=50
```

### Service Validation
```bash
# Application health endpoint
curl -f https://localhost/healthz

# Database readiness
docker-compose -f docker-compose.local.yml exec postgres pg_isready -U harmoni360

# Redis ping
docker-compose -f docker-compose.local.yml exec redis redis-cli ping
```

## 📝 Manual Verification Steps

1. Open the application via `https://localhost` or the generated ngrok URL.
2. Sign in using your development credentials.
3. Navigate through the main dashboard to confirm UI elements load correctly.
4. Check Seq at `https://localhost:5341` for request logs.
5. Review Grafana dashboards at `https://localhost:3000` for metrics.
6. If email features are enabled, trigger a test email and confirm receipt.

## ✔️ Additional Checks

- Restart individual services with `docker-compose -f docker-compose.local.yml restart <service>`.
- Monitor runtime usage with `docker stats`.
- Recreate the environment with `docker-compose -f docker-compose.local.yml down && docker-compose -f docker-compose.local.yml up -d` if issues persist.

## 📚 Related Documentation
- [Troubleshooting Guide](./Troubleshooting.md)
- [Environment Configuration](./Environment_Configuration.md)
- [Windows Setup](./Windows_Setup.md)

---

*Previous: [Environment Configuration](./Environment_Configuration.md) | Next: [Troubleshooting Guide](./Troubleshooting.md)*
