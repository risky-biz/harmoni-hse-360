# HarmoniHSE360 Local Development Environment Setup Script
# This script automates the setup of the complete local development environment

param(
    [switch]$SkipPrerequisites,
    [switch]$GenerateSecrets,
    [switch]$StartServices,
    [switch]$SetupNgrok,
    [string]$NgrokAuthToken = "",
    [switch]$Verbose
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Enable verbose output if requested
if ($Verbose) {
    $VerbosePreference = "Continue"
}

Write-Host "=== HarmoniHSE360 Local Development Environment Setup ===" -ForegroundColor Green
Write-Host "Starting setup process..." -ForegroundColor Yellow

# Function to check if running as administrator
function Test-Administrator {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

# Function to generate secure password
function New-SecurePassword {
    param([int]$Length = 32)
    $chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*"
    $password = ""
    for ($i = 0; $i -lt $Length; $i++) {
        $password += $chars[(Get-Random -Maximum $chars.Length)]
    }
    return $password
}

# Function to generate JWT key
function New-JwtKey {
    return [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-SecurePassword -Length 64)))
}

# Function to generate Seq password hash
function New-SeqPasswordHash {
    param([string]$Password)
    try {
        $hash = echo $Password | docker run --rm -i datalust/seq config hash
        return $hash.Trim()
    } catch {
        Write-Warning "Could not generate Seq password hash. Using placeholder."
        return '$SHA256$V1$10000$placeholder_hash'
    }
}

# Check prerequisites
if (-not $SkipPrerequisites) {
    Write-Host "`n=== Checking Prerequisites ===" -ForegroundColor Cyan
    
    # Check Windows version
    $osVersion = Get-CimInstance -ClassName Win32_OperatingSystem
    Write-Verbose "OS: $($osVersion.Caption) $($osVersion.Version)"
    
    # Check if Hyper-V is enabled
    $hyperV = Get-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V-All
    if ($hyperV.State -ne "Enabled") {
        Write-Error "Hyper-V is not enabled. Please enable Hyper-V and restart your computer."
    }
    Write-Host "✅ Hyper-V is enabled" -ForegroundColor Green
    
    # Check Docker Desktop
    try {
        $dockerVersion = docker --version
        Write-Host "✅ Docker Desktop: $dockerVersion" -ForegroundColor Green
    } catch {
        Write-Error "Docker Desktop is not installed or not running. Please install Docker Desktop."
    }
    
    # Check available memory
    $memory = Get-CimInstance -ClassName Win32_PhysicalMemory | Measure-Object -Property Capacity -Sum
    $memoryGB = [math]::Round($memory.Sum / 1GB, 2)
    if ($memoryGB -lt 32) {
        Write-Warning "System has $memoryGB GB RAM. 32GB+ recommended for full stack."
    } else {
        Write-Host "✅ System Memory: $memoryGB GB" -ForegroundColor Green
    }
    
    # Check available disk space
    $disk = Get-WmiObject -Class Win32_LogicalDisk | Where-Object { $_.DeviceID -eq "C:" }
    $freeSpaceGB = [math]::Round($disk.FreeSpace / 1GB, 2)
    if ($freeSpaceGB -lt 100) {
        Write-Warning "Low disk space: $freeSpaceGB GB free. 100GB+ recommended."
    } else {
        Write-Host "✅ Available Disk Space: $freeSpaceGB GB" -ForegroundColor Green
    }
}

# Create directory structure
Write-Host "`n=== Creating Directory Structure ===" -ForegroundColor Cyan
$directories = @(
    "local-dev\config\nginx\conf.d",
    "local-dev\config\prometheus\rules",
    "local-dev\config\grafana\provisioning\datasources",
    "local-dev\config\grafana\provisioning\dashboards",
    "local-dev\config\grafana\dashboards",
    "local-dev\config\redis",
    "local-dev\config\postgres\init-scripts",
    "local-dev\ssl",
    "local-dev\logs\nginx",
    "local-dev\logs\app",
    "local-dev\backups\postgres",
    "local-dev\backups\uploads",
    "local-dev\scripts"
)

foreach ($dir in $directories) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        Write-Verbose "Created directory: $dir"
    }
}
Write-Host "✅ Directory structure created" -ForegroundColor Green

# Generate secrets and environment file
if ($GenerateSecrets -or -not (Test-Path ".env.local")) {
    Write-Host "`n=== Generating Secrets and Environment Configuration ===" -ForegroundColor Cyan
    
    # Generate secure passwords
    $postgresPassword = New-SecurePassword -Length 32
    $redisPassword = New-SecurePassword -Length 32
    $jwtKey = New-JwtKey
    $grafanaPassword = New-SecurePassword -Length 16
    $seqPassword = New-SecurePassword -Length 16
    $seqPasswordHash = New-SeqPasswordHash -Password $seqPassword
    
    # Create .env.local file
    $envContent = Get-Content ".env.local.example" -Raw
    $envContent = $envContent -replace "POSTGRES_PASSWORD=.*", "POSTGRES_PASSWORD=$postgresPassword"
    $envContent = $envContent -replace "REDIS_PASSWORD=.*", "REDIS_PASSWORD=$redisPassword"
    $envContent = $envContent -replace "JWT_KEY=.*", "JWT_KEY=$jwtKey"
    $envContent = $envContent -replace "GRAFANA_ADMIN_PASSWORD=.*", "GRAFANA_ADMIN_PASSWORD=$grafanaPassword"
    $envContent = $envContent -replace "SEQ_ADMIN_PASSWORD_HASH=.*", "SEQ_ADMIN_PASSWORD_HASH=$seqPasswordHash"
    
    if ($NgrokAuthToken) {
        $envContent = $envContent -replace "NGROK_AUTHTOKEN=.*", "NGROK_AUTHTOKEN=$NgrokAuthToken"
    }
    
    $envContent | Out-File -FilePath ".env.local" -Encoding UTF8
    
    Write-Host "✅ Environment configuration created" -ForegroundColor Green
    Write-Host "Generated passwords (save these securely):" -ForegroundColor Yellow
    Write-Host "  PostgreSQL: $postgresPassword" -ForegroundColor White
    Write-Host "  Redis: $redisPassword" -ForegroundColor White
    Write-Host "  Grafana Admin: $grafanaPassword" -ForegroundColor White
    Write-Host "  Seq Admin: $seqPassword" -ForegroundColor White
}

# Generate self-signed SSL certificate
Write-Host "`n=== Generating SSL Certificate ===" -ForegroundColor Cyan
if (-not (Test-Path "local-dev\ssl\cert.pem")) {
    try {
        # Create OpenSSL configuration
        $opensslConfig = @"
[req]
distinguished_name = req_distinguished_name
x509_extensions = v3_req
prompt = no

[req_distinguished_name]
C = US
ST = Development
L = Local
O = HarmoniHSE360
OU = Development
CN = localhost

[v3_req]
keyUsage = keyEncipherment, dataEncipherment
extendedKeyUsage = serverAuth
subjectAltName = @alt_names

[alt_names]
DNS.1 = localhost
DNS.2 = *.ngrok.io
DNS.3 = *.ngrok-free.app
IP.1 = 127.0.0.1
IP.2 = ::1
"@
        
        $opensslConfig | Out-File -FilePath "local-dev\ssl\openssl.conf" -Encoding ASCII
        
        # Generate certificate using OpenSSL (if available) or PowerShell
        if (Get-Command openssl -ErrorAction SilentlyContinue) {
            & openssl req -x509 -nodes -days 365 -newkey rsa:2048 `
                -keyout "local-dev\ssl\key.pem" `
                -out "local-dev\ssl\cert.pem" `
                -config "local-dev\ssl\openssl.conf"
        } else {
            # Fallback to PowerShell certificate generation
            $cert = New-SelfSignedCertificate -DnsName "localhost", "*.ngrok.io", "*.ngrok-free.app" `
                -CertStoreLocation "cert:\LocalMachine\My" `
                -KeyAlgorithm RSA -KeyLength 2048 `
                -NotAfter (Get-Date).AddYears(1)
            
            # Export certificate
            $certPath = "local-dev\ssl\cert.pem"
            $keyPath = "local-dev\ssl\key.pem"
            
            Export-Certificate -Cert $cert -FilePath $certPath -Type CERT
            # Note: Private key export requires additional steps in PowerShell
            Write-Warning "SSL certificate generated but private key export may require manual steps."
        }
        
        Write-Host "✅ SSL certificate generated" -ForegroundColor Green
    } catch {
        Write-Warning "Could not generate SSL certificate: $($_.Exception.Message)"
        Write-Host "You may need to generate certificates manually or use ngrok's built-in SSL." -ForegroundColor Yellow
    }
}

# Setup ngrok configuration
if ($SetupNgrok) {
    Write-Host "`n=== Setting up Ngrok Configuration ===" -ForegroundColor Cyan
    
    if (-not $NgrokAuthToken) {
        $NgrokAuthToken = Read-Host "Enter your ngrok authtoken (get it from https://dashboard.ngrok.com/get-started/your-authtoken)"
    }
    
    if ($NgrokAuthToken) {
        try {
            # Configure ngrok authtoken
            & ngrok config add-authtoken $NgrokAuthToken
            
            # Create ngrok configuration
            $ngrokConfigPath = "$env:USERPROFILE\.ngrok2\ngrok.yml"
            $ngrokConfig = @"
version: "2"
authtoken: $NgrokAuthToken

tunnels:
  harmoni360-app:
    proto: http
    addr: 443
    bind_tls: true
    inspect: true
    
  grafana:
    proto: http
    addr: 3000
    bind_tls: true
    inspect: true
    
  prometheus:
    proto: http
    addr: 9090
    bind_tls: true
    inspect: true
    
  seq:
    proto: http
    addr: 5341
    bind_tls: true
    inspect: true

region: us
"@
            
            $ngrokConfig | Out-File -FilePath $ngrokConfigPath -Encoding UTF8
            Write-Host "✅ Ngrok configuration created" -ForegroundColor Green
        } catch {
            Write-Warning "Could not configure ngrok: $($_.Exception.Message)"
        }
    }
}

# Start services
if ($StartServices) {
    Write-Host "`n=== Starting Docker Services ===" -ForegroundColor Cyan
    
    try {
        # Pull latest images
        Write-Host "Pulling Docker images..." -ForegroundColor Yellow
        & docker-compose -f docker-compose.local.yml pull
        
        # Build application image
        Write-Host "Building application image..." -ForegroundColor Yellow
        & docker-compose -f docker-compose.local.yml build
        
        # Start services
        Write-Host "Starting services..." -ForegroundColor Yellow
        & docker-compose -f docker-compose.local.yml up -d
        
        # Wait for services to be healthy
        Write-Host "Waiting for services to be ready..." -ForegroundColor Yellow
        Start-Sleep -Seconds 30
        
        # Check service status
        $services = & docker-compose -f docker-compose.local.yml ps --format json | ConvertFrom-Json
        foreach ($service in $services) {
            $status = if ($service.State -eq "running") { "✅" } else { "❌" }
            Write-Host "$status $($service.Service): $($service.State)" -ForegroundColor $(if ($service.State -eq "running") { "Green" } else { "Red" })
        }
        
        Write-Host "✅ Services started successfully" -ForegroundColor Green
        
        # Display access URLs
        Write-Host "`n=== Service Access URLs ===" -ForegroundColor Cyan
        Write-Host "Application: https://localhost" -ForegroundColor White
        Write-Host "Grafana: http://localhost:3000" -ForegroundColor White
        Write-Host "Prometheus: http://localhost:9090" -ForegroundColor White
        Write-Host "Seq: http://localhost:5341" -ForegroundColor White
        
    } catch {
        Write-Error "Failed to start services: $($_.Exception.Message)"
    }
}

# Final instructions
Write-Host "`n=== Setup Complete ===" -ForegroundColor Green
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Review and customize .env.local file" -ForegroundColor White
Write-Host "2. Start services: docker-compose -f docker-compose.local.yml up -d" -ForegroundColor White
Write-Host "3. Start ngrok tunnels: ngrok start --all" -ForegroundColor White
Write-Host "4. Access application via ngrok URL or https://localhost" -ForegroundColor White
Write-Host "5. Monitor services via Grafana dashboard" -ForegroundColor White

Write-Host "`nFor troubleshooting, check:" -ForegroundColor Yellow
Write-Host "- Service logs: docker-compose -f docker-compose.local.yml logs -f" -ForegroundColor White
Write-Host "- Service status: docker-compose -f docker-compose.local.yml ps" -ForegroundColor White
Write-Host "- Ngrok status: http://localhost:4040" -ForegroundColor White

Write-Host "`n=== Setup Script Completed ===" -ForegroundColor Green
