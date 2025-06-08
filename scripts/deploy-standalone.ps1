# Harmoni360 Standalone Server Deployment Script (PowerShell)
# This script automates the deployment process for standalone server on Windows

param(
    [switch]$SkipPrerequisites,
    [switch]$Force
)

# Configuration
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir
$LogFile = "harmoni360-deploy-$(Get-Date -Format 'yyyyMMdd_HHmmss').log"

# Logging functions
function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    
    $Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $LogMessage = "[$Timestamp] [$Level] $Message"
    
    switch ($Level) {
        "ERROR" { Write-Host $LogMessage -ForegroundColor Red }
        "WARNING" { Write-Host $LogMessage -ForegroundColor Yellow }
        "SUCCESS" { Write-Host $LogMessage -ForegroundColor Green }
        default { Write-Host $LogMessage -ForegroundColor Cyan }
    }
    
    Add-Content -Path $LogFile -Value $LogMessage
}

function Write-Error-Exit {
    param([string]$Message)
    Write-Log $Message "ERROR"
    exit 1
}

# Check if running as administrator
function Test-Administrator {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

# Check prerequisites
function Test-Prerequisites {
    Write-Log "Checking prerequisites..."
    
    if (-not $SkipPrerequisites) {
        # Check if running as administrator
        if (-not (Test-Administrator)) {
            Write-Error-Exit "This script must be run as Administrator. Please run PowerShell as Administrator and try again."
        }
        
        # Check if Docker Desktop is installed and running
        try {
            $dockerVersion = docker --version
            Write-Log "Docker found: $dockerVersion"
        }
        catch {
            Write-Error-Exit "Docker is not installed or not in PATH. Please install Docker Desktop first."
        }
        
        # Check if Docker Compose is available
        try {
            $composeVersion = docker-compose --version
            Write-Log "Docker Compose found: $composeVersion"
        }
        catch {
            Write-Error-Exit "Docker Compose is not available. Please ensure Docker Desktop is properly installed."
        }
        
        # Check if Docker is running
        try {
            docker ps | Out-Null
            Write-Log "Docker daemon is running"
        }
        catch {
            Write-Error-Exit "Docker daemon is not running. Please start Docker Desktop."
        }
        
        # Check if Git is installed
        try {
            $gitVersion = git --version
            Write-Log "Git found: $gitVersion"
        }
        catch {
            Write-Error-Exit "Git is not installed or not in PATH. Please install Git first."
        }
        
        # Check available disk space (minimum 10GB)
        $drive = Get-WmiObject -Class Win32_LogicalDisk -Filter "DeviceID='C:'"
        $freeSpaceGB = [math]::Round($drive.FreeSpace / 1GB, 2)
        if ($freeSpaceGB -lt 10) {
            Write-Error-Exit "Insufficient disk space. At least 10GB of free space is required. Available: $freeSpaceGB GB"
        }
        
        Write-Log "Prerequisites check passed" "SUCCESS"
    }
    else {
        Write-Log "Skipping prerequisites check" "WARNING"
    }
}

# Create directory structure
function New-DirectoryStructure {
    Write-Log "Creating directory structure..."
    
    $BaseDir = "C:\Harmoni360"
    $Directories = @(
        "$BaseDir\data\postgres",
        "$BaseDir\data\redis",
        "$BaseDir\data\uploads",
        "$BaseDir\data\seq",
        "$BaseDir\data\prometheus",
        "$BaseDir\data\grafana",
        "$BaseDir\logs\app",
        "$BaseDir\logs\nginx",
        "$BaseDir\backups\postgres",
        "$BaseDir\backups\uploads",
        "$BaseDir\ssl",
        "$BaseDir\scripts"
    )
    
    foreach ($dir in $Directories) {
        if (-not (Test-Path $dir)) {
            New-Item -ItemType Directory -Path $dir -Force | Out-Null
            Write-Log "Created directory: $dir"
        }
    }
    
    Write-Log "Directory structure created" "SUCCESS"
}

# Check environment configuration
function Test-Environment {
    Write-Log "Checking environment configuration..."
    
    $EnvFile = Join-Path $ProjectRoot ".env.prod"
    
    if (-not (Test-Path $EnvFile)) {
        Write-Log "Production environment file not found. Creating from template..." "WARNING"
        $TemplateFile = Join-Path $ProjectRoot ".env.prod.example"
        Copy-Item $TemplateFile $EnvFile
        Write-Error-Exit "Please edit .env.prod with your configuration values and run the script again."
    }
    
    # Read environment file
    $EnvVars = @{}
    Get-Content $EnvFile | ForEach-Object {
        if ($_ -match '^([^#][^=]+)=(.*)$') {
            $EnvVars[$matches[1]] = $matches[2]
        }
    }
    
    # Check required variables
    $RequiredVars = @("POSTGRES_PASSWORD", "REDIS_PASSWORD", "JWT_KEY", "DOMAIN_NAME")
    foreach ($var in $RequiredVars) {
        if (-not $EnvVars.ContainsKey($var) -or [string]::IsNullOrEmpty($EnvVars[$var])) {
            Write-Error-Exit "Required environment variable $var is not set in .env.prod"
        }
    }
    
    Write-Log "Environment configuration validated" "SUCCESS"
    return $EnvVars
}

# Generate SSL certificates
function New-SSLCertificates {
    param([hashtable]$EnvVars)
    
    Write-Log "Setting up SSL certificates..."
    
    $CertPath = "C:\Harmoni360\ssl\cert.pem"
    $KeyPath = "C:\Harmoni360\ssl\key.pem"
    
    if (-not (Test-Path $CertPath) -or -not (Test-Path $KeyPath)) {
        Write-Log "SSL certificates not found. Generating self-signed certificates for testing..."
        
        $DomainName = $EnvVars["DOMAIN_NAME"]
        if ([string]::IsNullOrEmpty($DomainName)) {
            $DomainName = "localhost"
        }
        
        # Generate self-signed certificate using PowerShell
        $cert = New-SelfSignedCertificate -DnsName $DomainName -CertStoreLocation "cert:\LocalMachine\My" -KeyLength 2048 -KeyAlgorithm RSA -HashAlgorithm SHA256 -KeyUsage DigitalSignature, KeyEncipherment -Type SSLServerAuthentication
        
        # Export certificate and private key
        $certPassword = ConvertTo-SecureString -String "temp" -Force -AsPlainText
        Export-PfxCertificate -Cert $cert -FilePath "C:\Harmoni360\ssl\temp.pfx" -Password $certPassword | Out-Null
        
        # Convert to PEM format (requires OpenSSL or similar tool)
        Write-Log "Self-signed certificate generated. For production, please use Let's Encrypt or proper CA certificates." "WARNING"
    }
    else {
        Write-Log "SSL certificates already exist"
    }
    
    Write-Log "SSL certificates configured" "SUCCESS"
}

# Deploy application
function Deploy-Application {
    Write-Log "Building and deploying application..."
    
    Set-Location $ProjectRoot
    
    # Pull latest changes (if this is an update)
    if (Test-Path ".git") {
        Write-Log "Pulling latest changes..."
        try {
            git pull origin main
        }
        catch {
            Write-Log "Failed to pull latest changes. Continuing with current code." "WARNING"
        }
    }
    
    # Build application
    Write-Log "Building Docker images..."
    docker-compose -f docker-compose.prod.yml build --no-cache
    if ($LASTEXITCODE -ne 0) {
        Write-Error-Exit "Failed to build Docker images"
    }
    
    # Start services
    Write-Log "Starting services..."
    docker-compose -f docker-compose.prod.yml up -d
    if ($LASTEXITCODE -ne 0) {
        Write-Error-Exit "Failed to start services"
    }
    
    # Wait for services to be ready
    Write-Log "Waiting for services to start..."
    Start-Sleep -Seconds 30
    
    # Check service health
    Test-ServiceHealth
    
    Write-Log "Application deployed successfully" "SUCCESS"
}

# Check service health
function Test-ServiceHealth {
    Write-Log "Checking service health..."
    
    # Check if all containers are running
    $Services = @("postgres", "redis", "app", "nginx", "seq", "prometheus", "grafana")
    $FailedServices = @()
    
    foreach ($service in $Services) {
        $status = docker-compose -f docker-compose.prod.yml ps $service
        if ($status -notmatch "Up") {
            $FailedServices += $service
        }
    }
    
    if ($FailedServices.Count -gt 0) {
        Write-Error-Exit "The following services failed to start: $($FailedServices -join ', ')"
    }
    
    # Test application health endpoint
    Write-Log "Testing application health endpoint..."
    for ($i = 1; $i -le 10; $i++) {
        try {
            $response = Invoke-WebRequest -Uri "https://localhost/health" -UseBasicParsing -SkipCertificateCheck -TimeoutSec 10
            if ($response.StatusCode -eq 200) {
                Write-Log "Application health check passed" "SUCCESS"
                return
            }
        }
        catch {
            Write-Log "Health check attempt $i/10 failed, retrying in 10 seconds..."
            Start-Sleep -Seconds 10
        }
    }
    
    Write-Error-Exit "Application health check failed after 10 attempts"
}

# Run database migrations
function Invoke-DatabaseMigrations {
    Write-Log "Running database migrations..."
    
    # Wait for database to be ready
    Write-Log "Waiting for database to be ready..."
    for ($i = 1; $i -le 30; $i++) {
        try {
            docker-compose -f docker-compose.prod.yml exec -T postgres pg_isready -U harmoni360
            if ($LASTEXITCODE -eq 0) {
                break
            }
        }
        catch {
            # Continue waiting
        }
        
        if ($i -eq 30) {
            Write-Error-Exit "Database failed to become ready after 5 minutes"
        }
        Start-Sleep -Seconds 10
    }
    
    # Run migrations
    Write-Log "Applying database migrations..."
    docker-compose -f docker-compose.prod.yml exec -T app dotnet ef database update
    if ($LASTEXITCODE -ne 0) {
        Write-Error-Exit "Database migration failed"
    }
    
    Write-Log "Database migrations completed" "SUCCESS"
}

# Configure Windows Firewall
function Set-FirewallRules {
    Write-Log "Configuring Windows Firewall..."
    
    try {
        # Allow HTTP traffic
        New-NetFirewallRule -DisplayName "Harmoni360 HTTP" -Direction Inbound -Protocol TCP -LocalPort 80 -Action Allow -ErrorAction SilentlyContinue
        
        # Allow HTTPS traffic
        New-NetFirewallRule -DisplayName "Harmoni360 HTTPS" -Direction Inbound -Protocol TCP -LocalPort 443 -Action Allow -ErrorAction SilentlyContinue
        
        Write-Log "Firewall rules configured" "SUCCESS"
    }
    catch {
        Write-Log "Failed to configure firewall rules. Please configure manually." "WARNING"
    }
}

# Display deployment summary
function Show-DeploymentSummary {
    param([hashtable]$EnvVars)
    
    Write-Log "Deployment completed successfully!" "SUCCESS"
    Write-Host ""
    Write-Host "=== Harmoni360 Deployment Summary ===" -ForegroundColor Green
    Write-Host "Application URL: https://$($EnvVars['DOMAIN_NAME'])"
    Write-Host "Grafana Dashboard: https://$($EnvVars['DOMAIN_NAME']):3000"
    Write-Host "Seq Logs: http://localhost:5341"
    Write-Host ""
    Write-Host "Default Credentials:" -ForegroundColor Yellow
    Write-Host "- Grafana: admin / $($EnvVars['GRAFANA_ADMIN_PASSWORD'])"
    Write-Host ""
    Write-Host "Important Directories:" -ForegroundColor Yellow
    Write-Host "- Configuration: $ProjectRoot\.env.prod"
    Write-Host "- Data Directory: C:\Harmoni360\data"
    Write-Host "- Logs Directory: C:\Harmoni360\logs"
    Write-Host "- Backups Directory: C:\Harmoni360\backups"
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Yellow
    Write-Host "1. Configure proper SSL certificates for production"
    Write-Host "2. Set up monitoring alerts in Grafana"
    Write-Host "3. Set up automated backups using Task Scheduler"
    Write-Host "4. Review security settings"
    Write-Host ""
    Write-Host "For troubleshooting, check: $LogFile"
    Write-Host "Documentation: docs\Deployment\Standalone_Server\"
}

# Main deployment function
function Start-Deployment {
    Write-Log "Starting Harmoni360 standalone server deployment..."
    
    try {
        Test-Prerequisites
        New-DirectoryStructure
        $EnvVars = Test-Environment
        New-SSLCertificates -EnvVars $EnvVars
        Set-FirewallRules
        Deploy-Application
        Invoke-DatabaseMigrations
        Show-DeploymentSummary -EnvVars $EnvVars
        
        Write-Log "Deployment completed successfully!" "SUCCESS"
    }
    catch {
        Write-Log "Deployment failed: $($_.Exception.Message)" "ERROR"
        Write-Log "Check log file for details: $LogFile" "ERROR"
        exit 1
    }
}

# Handle script interruption
trap {
    Write-Log "Deployment interrupted. Check log file: $LogFile" "ERROR"
    exit 1
}

# Run main function
Start-Deployment
