# Local Development Troubleshooting Guide

## Overview

This guide lists common issues encountered when running the HarmoniHSE360 stack locally and provides steps to resolve them.

## 🚨 Common Issues and Solutions

### 1. Docker Containers Fail to Start
**Symptoms:** Containers immediately exit or `Cannot connect to the Docker daemon` messages appear.

**Solutions:**
- Ensure Docker Desktop is running.
- Run `docker-compose -f docker-compose.local.yml ps` to check container status.
- Inspect logs with `docker-compose -f docker-compose.local.yml logs` to find errors.

### 2. Database Connection Errors
**Symptoms:** Application cannot connect to PostgreSQL or migrations fail.

**Solutions:**
- Verify credentials in `.env.local` match those in `docker-compose.local.yml`.
- Check database logs: `docker-compose -f docker-compose.local.yml logs postgres`.
- Test connectivity: `docker-compose -f docker-compose.local.yml exec postgres pg_isready -U harmoni360`.

### 3. Ngrok Tunnel Not Starting
**Symptoms:** `ngrok` command fails or no public URL is generated.

**Solutions:**
- Confirm ngrok is installed and available in your PATH.
- Run `ngrok config check` to verify your authtoken.
- Ensure no corporate firewall rules are blocking outbound tunnels.

### 4. Port Conflicts
**Symptoms:** Errors such as `address already in use` when starting services.

**Solutions:**
- Identify the conflicting process with `netstat -ano | findstr :8080` (Windows) or `lsof -i :8080` (Linux/Mac).
- Stop the process or change the port numbers in `.env.local` and `docker-compose.local.yml`.

### 5. Slow Performance
**Symptoms:** Pages load slowly or containers consume high CPU/RAM.

**Solutions:**
- Increase resource allocation in Docker Desktop settings.
- Monitor usage with `docker stats` to identify bottlenecks.
- Restart containers to clear temporary load: `docker-compose -f docker-compose.local.yml restart`.

## 📚 Additional Resources
- [Testing Procedures](./Testing_Procedures.md)
- [Environment Configuration](./Environment_Configuration.md)
- [Windows Setup](./Windows_Setup.md)

---

*Previous: [Testing Procedures](./Testing_Procedures.md) | Back to [README](./README.md)*
