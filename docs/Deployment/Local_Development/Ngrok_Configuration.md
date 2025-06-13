# Ngrok Configuration for Local Development

## Overview

This guide covers the complete setup and configuration of ngrok to provide public HTTPS access to your local HarmoniHSE360 development environment, simulating real-world domain access and SSL certificates.

## 🎯 Ngrok Overview

### What is Ngrok?
Ngrok creates secure tunnels from public URLs to your local development environment, enabling:
- **Public HTTPS access** to local applications
- **Webhook testing** with external services
- **Mobile device testing** over the internet
- **Client demonstrations** without deployment
- **SSL/TLS simulation** for production-like testing

### Ngrok Architecture
```
Internet Users
      │
      ▼
┌─────────────────┐
│  Ngrok Cloud    │
│  (ngrok.io)     │
└─────────────────┘
      │
      ▼ (Secure Tunnel)
┌─────────────────┐
│  Local Machine  │
│  (Windows 11)   │
└─────────────────┘
      │
      ▼
┌─────────────────┐
│  Docker Stack   │
│  (HarmoniHSE360)│
└─────────────────┘
```

## 💰 Ngrok Pricing Analysis

### Plan Comparison
| Feature | Free Plan | Basic Plan ($8/month) | Pro Plan ($20/month) | Business Plan ($39/month) |
|---------|-----------|----------------------|---------------------|---------------------------|
| **Online Tunnels** | 1 | 10 | 25 | 100 |
| **Custom Subdomains** | ❌ | ✅ | ✅ | ✅ |
| **Reserved Domains** | ❌ | ❌ | ✅ | ✅ |
| **Password Protection** | ❌ | ✅ | ✅ | ✅ |
| **IP Whitelisting** | ❌ | ❌ | ✅ | ✅ |
| **Custom Certificates** | ❌ | ❌ | ❌ | ✅ |
| **Team Management** | ❌ | ❌ | ❌ | ✅ |

### Recommendation for HarmoniHSE360
- **Development/Testing**: Free plan (single tunnel)
- **Team Development**: Basic plan ($8/month) for custom subdomains
- **Client Demos**: Pro plan ($20/month) for reserved domains
- **Enterprise**: Business plan ($39/month) for team features

## 🚀 Phase 1: Ngrok Installation (10-15 minutes)

### 1.1 Install Ngrok

#### Option A: Using Chocolatey (Recommended)
```powershell
# Install ngrok via Chocolatey
choco install ngrok -y

# Verify installation
ngrok version
```

#### Option B: Manual Installation
```powershell
# Download ngrok
$url = "https://bin.equinox.io/c/bNyj1mQVY4c/ngrok-v3-stable-windows-amd64.zip"
$output = "$env:TEMP\ngrok.zip"
Invoke-WebRequest -Uri $url -OutFile $output

# Extract to tools directory
$extractPath = "C:\tools\ngrok"
New-Item -ItemType Directory -Path $extractPath -Force
Expand-Archive -Path $output -DestinationPath $extractPath -Force

# Add to PATH
$currentPath = [Environment]::GetEnvironmentVariable("PATH", "User")
if ($currentPath -notlike "*$extractPath*") {
    [Environment]::SetEnvironmentVariable("PATH", "$currentPath;$extractPath", "User")
}

# Clean up
Remove-Item $output

# Refresh environment
refreshenv
```

### 1.2 Create Ngrok Account
1. Visit [ngrok.com/signup](https://ngrok.com/signup)
2. Sign up with email or GitHub account
3. Verify email address
4. Access dashboard at [dashboard.ngrok.com](https://dashboard.ngrok.com)

### 1.3 Configure Authentication
```powershell
# Get your authtoken from https://dashboard.ngrok.com/get-started/your-authtoken
# Replace YOUR_AUTHTOKEN with your actual token
ngrok config add-authtoken YOUR_AUTHTOKEN_HERE

# Verify configuration
ngrok config check
```

## ⚙️ Phase 2: Ngrok Configuration (15-20 minutes)

### 2.1 Create Ngrok Configuration File

Create `%USERPROFILE%\.ngrok2\ngrok.yml`:

```yaml
# HarmoniHSE360 Local Development Ngrok Configuration
version: "2"
authtoken: YOUR_AUTHTOKEN_HERE

# Global settings
console_ui: true
console_ui_color: transparent
log_level: info
log_format: logfmt
log: C:\Users\%USERNAME%\AppData\Local\ngrok\ngrok.log

# Tunnel definitions
tunnels:
  # Main HarmoniHSE360 application (HTTPS)
  harmoni360-app:
    proto: http
    addr: 443
    bind_tls: true
    inspect: true
    # For paid plans, add custom subdomain:
    # subdomain: harmoni360-dev
    # For reserved domains (Pro plan):
    # hostname: harmoni360-dev.yourdomain.com
    
  # Grafana monitoring dashboard
  grafana:
    proto: http
    addr: 3000
    bind_tls: true
    inspect: true
    # For paid plans:
    # subdomain: harmoni360-grafana
    
  # Prometheus metrics
  prometheus:
    proto: http
    addr: 9090
    bind_tls: true
    inspect: true
    # For paid plans:
    # subdomain: harmoni360-prometheus
    
  # Seq structured logging
  seq:
    proto: http
    addr: 5341
    bind_tls: true
    inspect: true
    # For paid plans:
    # subdomain: harmoni360-seq

# Region selection (choose closest to your location)
region: us  # Options: us, eu, ap, au, sa, jp, in

# Update check
update_channel: stable
update_check: true
```

### 2.2 Free Plan Configuration

For users on the free plan, use this simplified configuration:

```yaml
# HarmoniHSE360 Free Plan Configuration
version: "2"
authtoken: YOUR_AUTHTOKEN_HERE

tunnels:
  # Main application only (free plan limitation)
  harmoni360-app:
    proto: http
    addr: 443
    bind_tls: true
    inspect: true

region: us
```

### 2.3 Paid Plan Configuration with Custom Domains

For Basic/Pro plan users:

```yaml
# HarmoniHSE360 Paid Plan Configuration
version: "2"
authtoken: YOUR_AUTHTOKEN_HERE

tunnels:
  harmoni360-app:
    proto: http
    addr: 443
    bind_tls: true
    subdomain: harmoni360-dev  # Custom subdomain
    inspect: true
    auth: "username:password"  # Optional password protection
    
  grafana:
    proto: http
    addr: 3000
    bind_tls: true
    subdomain: harmoni360-grafana
    inspect: true
    auth: "admin:monitoring"
    
  prometheus:
    proto: http
    addr: 9090
    bind_tls: true
    subdomain: harmoni360-prometheus
    inspect: true
    auth: "admin:metrics"
    
  seq:
    proto: http
    addr: 5341
    bind_tls: true
    subdomain: harmoni360-seq
    inspect: true
    auth: "admin:logging"

region: us
```

## 🔧 Phase 3: Tunnel Management (10-15 minutes)

### 3.1 Starting Tunnels

#### Single Tunnel (Free Plan)
```powershell
# Start main application tunnel
ngrok start harmoni360-app

# Alternative: Direct command
ngrok http 443 --bind-tls=true
```

#### Multiple Tunnels (Paid Plans)
```powershell
# Start all tunnels
ngrok start --all

# Start specific tunnels
ngrok start harmoni360-app grafana

# Start tunnels in background
Start-Process -FilePath "ngrok" -ArgumentList "start --all" -WindowStyle Hidden
```

### 3.2 Tunnel Status and Management

```powershell
# Check tunnel status via API
Invoke-RestMethod -Uri "http://localhost:4040/api/tunnels" | ConvertTo-Json -Depth 3

# Get tunnel URLs programmatically
$tunnels = Invoke-RestMethod -Uri "http://localhost:4040/api/tunnels"
$appUrl = ($tunnels.tunnels | Where-Object { $_.name -eq "harmoni360-app" }).public_url
Write-Host "Application URL: $appUrl"

# Save tunnel URLs to file
$tunnelInfo = @{
    "Application" = ($tunnels.tunnels | Where-Object { $_.name -eq "harmoni360-app" }).public_url
    "Grafana" = ($tunnels.tunnels | Where-Object { $_.name -eq "grafana" }).public_url
    "Prometheus" = ($tunnels.tunnels | Where-Object { $_.name -eq "prometheus" }).public_url
    "Seq" = ($tunnels.tunnels | Where-Object { $_.name -eq "seq" }).public_url
}
$tunnelInfo | ConvertTo-Json | Out-File -FilePath "tunnel-urls.json"
```

### 3.3 Ngrok Web Interface

Access the ngrok web interface at: `http://localhost:4040`

Features available:
- **Tunnel status** and traffic monitoring
- **Request inspection** with headers and body
- **Replay requests** for testing
- **Traffic filtering** and search
- **Performance metrics**

## 🔐 Phase 4: Security Configuration (15-20 minutes)

### 4.1 Password Protection

```yaml
# Add to ngrok.yml for password-protected tunnels
tunnels:
  harmoni360-app:
    proto: http
    addr: 443
    bind_tls: true
    auth: "harmoni360:SecurePassword123!"
    
  grafana:
    proto: http
    addr: 3000
    bind_tls: true
    auth: "monitoring:GrafanaPass456!"
```

### 4.2 IP Whitelisting (Pro Plan)

```yaml
# Restrict access to specific IP addresses
tunnels:
  harmoni360-app:
    proto: http
    addr: 443
    bind_tls: true
    ip_restriction:
      allow_cidrs:
        - "203.0.113.0/24"  # Your office network
        - "198.51.100.0/24" # Client network
      deny_cidrs:
        - "0.0.0.0/0"       # Deny all others
```

### 4.3 Request Headers and Security

```yaml
# Add security headers
tunnels:
  harmoni360-app:
    proto: http
    addr: 443
    bind_tls: true
    request_header:
      add:
        - "X-Forwarded-Proto: https"
        - "X-Real-IP: {{.NgrokClientIP}}"
      remove:
        - "X-Forwarded-For"
```

## 📊 Phase 5: Monitoring and Logging (10-15 minutes)

### 5.1 Enable Logging

```yaml
# Configure detailed logging
log_level: debug
log_format: json
log: C:\Users\%USERNAME%\AppData\Local\ngrok\ngrok.log

# Rotate logs
log_rotate:
  max_size: 100MB
  max_age: 30
  max_backups: 5
```

### 5.2 Webhook Testing

```powershell
# Test webhook endpoints
$webhookUrl = "https://your-tunnel-url.ngrok.io/api/webhooks/test"
$testPayload = @{
    "event" = "test"
    "timestamp" = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssZ")
    "data" = @{
        "message" = "Test webhook from ngrok"
    }
} | ConvertTo-Json

Invoke-RestMethod -Uri $webhookUrl -Method POST -Body $testPayload -ContentType "application/json"
```

### 5.3 Performance Monitoring

```powershell
# Monitor tunnel performance
$stats = Invoke-RestMethod -Uri "http://localhost:4040/api/tunnels"
foreach ($tunnel in $stats.tunnels) {
    Write-Host "Tunnel: $($tunnel.name)"
    Write-Host "URL: $($tunnel.public_url)"
    Write-Host "Connections: $($tunnel.metrics.conns.count)"
    Write-Host "Requests: $($tunnel.metrics.http.count)"
    Write-Host "---"
}
```

## 🧪 Phase 6: Testing and Validation (15-20 minutes)

### 6.1 Basic Connectivity Tests

```powershell
# Test tunnel connectivity
$tunnels = Invoke-RestMethod -Uri "http://localhost:4040/api/tunnels"
$appUrl = ($tunnels.tunnels | Where-Object { $_.name -eq "harmoni360-app" }).public_url

# Test HTTPS access
try {
    $response = Invoke-WebRequest -Uri "$appUrl/health" -UseBasicParsing
    Write-Host "Health check successful: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "Health check failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test SSL certificate
$cert = [System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
try {
    $request = [System.Net.WebRequest]::Create($appUrl)
    $response = $request.GetResponse()
    Write-Host "SSL connection successful" -ForegroundColor Green
    $response.Close()
} catch {
    Write-Host "SSL connection failed: $($_.Exception.Message)" -ForegroundColor Red
}
```

### 6.2 Load Testing

```powershell
# Simple load test using PowerShell
$tunnels = Invoke-RestMethod -Uri "http://localhost:4040/api/tunnels"
$appUrl = ($tunnels.tunnels | Where-Object { $_.name -eq "harmoni360-app" }).public_url

$jobs = @()
1..10 | ForEach-Object {
    $jobs += Start-Job -ScriptBlock {
        param($url)
        1..100 | ForEach-Object {
            try {
                Invoke-WebRequest -Uri "$url/health" -UseBasicParsing | Out-Null
            } catch {
                Write-Error "Request failed: $_"
            }
        }
    } -ArgumentList $appUrl
}

# Wait for completion and get results
$jobs | Wait-Job | Receive-Job
$jobs | Remove-Job
```

## 🔧 Troubleshooting Common Issues

### Issue: Tunnel Connection Failed
```powershell
# Check ngrok service status
Get-Process ngrok -ErrorAction SilentlyContinue

# Restart ngrok
Stop-Process -Name ngrok -Force -ErrorAction SilentlyContinue
Start-Sleep 2
ngrok start harmoni360-app
```

### Issue: SSL Certificate Errors
```powershell
# Check tunnel SSL configuration
$tunnels = Invoke-RestMethod -Uri "http://localhost:4040/api/tunnels"
$tunnels.tunnels | Select-Object name, public_url, proto

# Verify local service is running
Test-NetConnection -ComputerName localhost -Port 443
```

### Issue: Authentication Problems
```powershell
# Verify authtoken
ngrok config check

# Re-add authtoken
ngrok config add-authtoken YOUR_NEW_AUTHTOKEN
```

## 📋 Best Practices

### Security Best Practices
- ✅ Use password protection for sensitive services
- ✅ Implement IP whitelisting when possible
- ✅ Monitor tunnel access logs regularly
- ✅ Rotate authtokens periodically
- ✅ Use HTTPS tunnels exclusively

### Performance Best Practices
- ✅ Choose the closest ngrok region
- ✅ Monitor tunnel metrics regularly
- ✅ Use reserved domains for consistent URLs
- ✅ Implement proper error handling
- ✅ Cache static content locally

### Development Best Practices
- ✅ Use environment-specific configurations
- ✅ Document tunnel URLs for team access
- ✅ Implement proper logging and monitoring
- ✅ Test with real mobile devices
- ✅ Validate webhook integrations

## 🎯 Next Steps

After configuring ngrok:

1. **Environment Setup**: Configure [Environment Variables](./Environment_Configuration.md)
2. **Deploy Local Stack**: Start the Docker Compose environment
3. **Test Integration**: Verify all services through ngrok tunnels
4. **Team Access**: Share tunnel URLs with team members
5. **Client Demos**: Use ngrok URLs for client demonstrations

## 📞 Support Resources

### Ngrok Documentation
- [Ngrok Documentation](https://ngrok.com/docs)
- [Ngrok API Reference](https://ngrok.com/docs/api)
- [Ngrok Troubleshooting](https://ngrok.com/docs/troubleshooting)

### Community Support
- [Ngrok Community Forum](https://community.ngrok.com/)
- [Ngrok GitHub Issues](https://github.com/inconshreveable/ngrok)

---

*Previous: [Windows Setup](./Windows_Setup.md) | Next: [Environment Configuration](./Environment_Configuration.md)*
