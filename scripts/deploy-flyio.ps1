# Harmoni360 Fly.io Deployment Script (PowerShell)
# This script automates the deployment process to Fly.io

param(
    [string]$AppName = "harmoni360-app",
    [string]$DbName = "harmoni360-db",
    [string]$RedisName = "harmoni360-redis",
    [string]$Region = "sjc"
)

$ErrorActionPreference = "Stop"

Write-Host "🚀 Starting Harmoni360 Fly.io Deployment" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green

# Check if fly CLI is installed
try {
    $flyVersion = fly version
    Write-Host "✅ Fly CLI found: $flyVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Fly CLI is not installed. Please install it first:" -ForegroundColor Red
    Write-Host "   iwr https://fly.io/install.ps1 -useb | iex" -ForegroundColor Yellow
    exit 1
}

# Check if user is authenticated
try {
    $whoami = fly auth whoami
    Write-Host "✅ Authenticated as: $whoami" -ForegroundColor Green
} catch {
    Write-Host "❌ Not authenticated with Fly.io. Please run:" -ForegroundColor Red
    Write-Host "   fly auth login" -ForegroundColor Yellow
    exit 1
}

Write-Host "📋 Configuration:" -ForegroundColor Cyan
Write-Host "   App Name: $AppName" -ForegroundColor White
Write-Host "   Database: $DbName" -ForegroundColor White
Write-Host "   Redis: $RedisName" -ForegroundColor White
Write-Host "   Region: $Region" -ForegroundColor White
Write-Host ""

# Function to check if app exists
function Test-AppExists {
    param([string]$Name)
    $apps = fly apps list --json | ConvertFrom-Json
    return $apps | Where-Object { $_.Name -eq $Name }
}

# Function to check if postgres app exists
function Test-PostgresExists {
    param([string]$Name)
    try {
        $postgres = fly postgres list --json | ConvertFrom-Json
        return $postgres | Where-Object { $_.Name -eq $Name }
    } catch {
        return $false
    }
}

# Step 1: Create PostgreSQL database if it doesn't exist
Write-Host "🗄️  Setting up PostgreSQL database..." -ForegroundColor Yellow
if (Test-PostgresExists $DbName) {
    Write-Host "   ✅ Database $DbName already exists" -ForegroundColor Green
} else {
    Write-Host "   📦 Creating PostgreSQL database: $DbName" -ForegroundColor Blue
    fly postgres create --name $DbName --region $Region --initial-cluster-size 1
    Write-Host "   ✅ Database created successfully" -ForegroundColor Green
}

# Step 2: Create Redis instance if it doesn't exist
Write-Host "🔴 Setting up Redis..." -ForegroundColor Yellow
try {
    $redisList = fly ext redis list --json | ConvertFrom-Json
    $redisExists = $redisList | Where-Object { $_.name -eq $RedisName }
    
    if ($redisExists) {
        Write-Host "   ✅ Redis instance $RedisName already exists" -ForegroundColor Green
    } else {
        Write-Host "   📦 Creating Redis instance: $RedisName" -ForegroundColor Blue
        fly ext redis create --name $RedisName
        Write-Host "   ✅ Redis created successfully" -ForegroundColor Green
    }
} catch {
    Write-Host "   📦 Creating Redis instance: $RedisName" -ForegroundColor Blue
    fly ext redis create --name $RedisName
    Write-Host "   ✅ Redis created successfully" -ForegroundColor Green
}

# Step 3: Initialize Fly app if it doesn't exist
Write-Host "🛠️  Setting up Fly application..." -ForegroundColor Yellow
if (Test-AppExists $AppName) {
    Write-Host "   ✅ App $AppName already exists" -ForegroundColor Green
} else {
    Write-Host "   📦 Initializing Fly app: $AppName" -ForegroundColor Blue
    
    # Check if fly.toml exists, if not copy from example
    if (-not (Test-Path "fly.toml")) {
        if (Test-Path "fly.toml.example") {
            Copy-Item "fly.toml.example" "fly.toml"
            Write-Host "   📄 Copied fly.toml from example" -ForegroundColor Blue
        } else {
            Write-Host "   ❌ fly.toml.example not found. Please create fly.toml manually." -ForegroundColor Red
            exit 1
        }
    }
    
    fly launch --no-deploy --name $AppName --region $Region
    Write-Host "   ✅ App initialized successfully" -ForegroundColor Green
}

# Step 4: Create volume if it doesn't exist
Write-Host "💾 Setting up persistent volume..." -ForegroundColor Yellow
$volumeName = "harmoni360_uploads"
try {
    $volumes = fly volumes list -a $AppName --json | ConvertFrom-Json
    $volumeExists = $volumes | Where-Object { $_.name -eq $volumeName }
    
    if ($volumeExists) {
        Write-Host "   ✅ Volume $volumeName already exists" -ForegroundColor Green
    } else {
        Write-Host "   📦 Creating volume: $volumeName" -ForegroundColor Blue
        fly volumes create $volumeName --region $Region --size 1 -a $AppName
        Write-Host "   ✅ Volume created successfully" -ForegroundColor Green
    }
} catch {
    Write-Host "   📦 Creating volume: $volumeName" -ForegroundColor Blue
    fly volumes create $volumeName --region $Region --size 1 -a $AppName
    Write-Host "   ✅ Volume created successfully" -ForegroundColor Green
}

# Step 5: Set up secrets
Write-Host "🔐 Setting up secrets..." -ForegroundColor Yellow

# Generate JWT key
$jwtKey = "Harmoni360-Production-JWT-Key-$(Get-Date -Format 'yyyyMMddHHmmss')-$([System.Guid]::NewGuid().ToString('N').Substring(0,16))"
fly secrets set "Jwt__Key=$jwtKey" -a $AppName
Write-Host "   ✅ JWT key set" -ForegroundColor Green

Write-Host "   ⚠️  Please set database and Redis connection strings manually:" -ForegroundColor Yellow
Write-Host "   fly secrets set ConnectionStrings__DefaultConnection=`"postgres://username:password@hostname:5432/database`" -a $AppName" -ForegroundColor Cyan
Write-Host "   fly secrets set ConnectionStrings__Redis=`"redis://username:password@hostname:6379`" -a $AppName" -ForegroundColor Cyan

# Step 6: Deploy application
Write-Host "🚀 Deploying application..." -ForegroundColor Yellow
fly deploy -a $AppName

# Step 7: Run database migrations
Write-Host "🔄 Running database migrations..." -ForegroundColor Yellow
Write-Host "   Please run manually: fly ssh console -a $AppName -C `"cd /app && dotnet ef database update`"" -ForegroundColor Cyan

Write-Host ""
Write-Host "🎉 Deployment completed successfully!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host "📱 Application URL: https://$AppName.fly.dev" -ForegroundColor Cyan
Write-Host "🔍 Status: fly status -a $AppName" -ForegroundColor White
Write-Host "📊 Logs: fly logs -a $AppName" -ForegroundColor White
Write-Host "🖥️  Dashboard: https://fly.io/apps/$AppName" -ForegroundColor White
Write-Host ""
Write-Host "🔧 Next steps:" -ForegroundColor Yellow
Write-Host "   1. Set database and Redis connection strings (see above)" -ForegroundColor White
Write-Host "   2. Test the application at the URL above" -ForegroundColor White
Write-Host "   3. Set up custom domain if needed: fly certs create yourdomain.com -a $AppName" -ForegroundColor White
Write-Host "   4. Load demo data: fly ssh console -a $AppName" -ForegroundColor White
Write-Host ""
