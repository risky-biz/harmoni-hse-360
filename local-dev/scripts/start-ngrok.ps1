# HarmoniHSE360 Ngrok Tunnel Management Script
# Manages ngrok tunnels for local development environment

param(
    [string]$Action = "start",  # start, stop, status, restart
    [string[]]$Tunnels = @("harmoni360-app"),  # Specific tunnels to manage
    [switch]$All,  # Manage all tunnels
    [switch]$Background,  # Run in background
    [switch]$ShowUrls,  # Display tunnel URLs
    [int]$WaitSeconds = 10  # Wait time for tunnel startup
)

$ErrorActionPreference = "Stop"

# Function to check if ngrok is installed
function Test-NgrokInstalled {
    try {
        $null = Get-Command ngrok -ErrorAction Stop
        return $true
    } catch {
        return $false
    }
}

# Function to get tunnel status
function Get-NgrokStatus {
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:4040/api/tunnels" -ErrorAction Stop
        return $response.tunnels
    } catch {
        return $null
    }
}

# Function to display tunnel URLs
function Show-TunnelUrls {
    $tunnels = Get-NgrokStatus
    if ($tunnels) {
        Write-Host "`n=== Active Ngrok Tunnels ===" -ForegroundColor Green
        foreach ($tunnel in $tunnels) {
            $name = $tunnel.name
            $publicUrl = $tunnel.public_url
            $localAddr = $tunnel.config.addr
            Write-Host "  $name" -ForegroundColor Yellow
            Write-Host "    Public URL: $publicUrl" -ForegroundColor White
            Write-Host "    Local Address: $localAddr" -ForegroundColor Gray
        }
        
        # Save URLs to file for easy access
        $urlInfo = @{}
        foreach ($tunnel in $tunnels) {
            $urlInfo[$tunnel.name] = $tunnel.public_url
        }
        $urlInfo | ConvertTo-Json | Out-File -FilePath "tunnel-urls.json" -Encoding UTF8
        Write-Host "`n  URLs saved to tunnel-urls.json" -ForegroundColor Cyan
    } else {
        Write-Host "No active tunnels found" -ForegroundColor Yellow
    }
}

# Function to wait for services to be ready
function Wait-ForServices {
    param([int]$TimeoutSeconds = 60)
    
    Write-Host "Waiting for local services to be ready..." -ForegroundColor Yellow
    
    $services = @(
        @{ Name = "Application"; Url = "https://localhost/health"; Port = 443 },
        @{ Name = "Grafana"; Url = "http://localhost:3000/api/health"; Port = 3000 },
        @{ Name = "Prometheus"; Url = "http://localhost:9090/-/ready"; Port = 9090 },
        @{ Name = "Seq"; Url = "http://localhost:5341/api"; Port = 5341 }
    )
    
    $startTime = Get-Date
    $allReady = $false
    
    while (-not $allReady -and ((Get-Date) - $startTime).TotalSeconds -lt $TimeoutSeconds) {
        $allReady = $true
        foreach ($service in $services) {
            try {
                $response = Invoke-WebRequest -Uri $service.Url -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
                Write-Host "  ✅ $($service.Name) is ready" -ForegroundColor Green
            } catch {
                Write-Host "  ⏳ $($service.Name) not ready yet..." -ForegroundColor Yellow
                $allReady = $false
            }
        }
        
        if (-not $allReady) {
            Start-Sleep -Seconds 5
        }
    }
    
    if ($allReady) {
        Write-Host "All services are ready!" -ForegroundColor Green
    } else {
        Write-Warning "Some services may not be ready yet. Check docker-compose logs."
    }
}

# Main script logic
Write-Host "=== HarmoniHSE360 Ngrok Tunnel Manager ===" -ForegroundColor Green

# Check if ngrok is installed
if (-not (Test-NgrokInstalled)) {
    Write-Error "Ngrok is not installed or not in PATH. Please install ngrok first."
}

# Check if .env.local exists and load ngrok token
if (Test-Path ".env.local") {
    $envContent = Get-Content ".env.local" -Raw
    if ($envContent -match "NGROK_AUTHTOKEN=(.+)") {
        $authToken = $matches[1]
        if ($authToken -and $authToken -ne "your_ngrok_authtoken_here") {
            Write-Host "Using ngrok authtoken from .env.local" -ForegroundColor Cyan
        } else {
            Write-Warning "Ngrok authtoken not configured in .env.local"
        }
    }
}

switch ($Action.ToLower()) {
    "start" {
        Write-Host "`nStarting ngrok tunnels..." -ForegroundColor Cyan
        
        # Wait for local services to be ready
        Wait-ForServices -TimeoutSeconds 60
        
        try {
            if ($All) {
                if ($Background) {
                    Start-Process -FilePath "ngrok" -ArgumentList "start --all" -WindowStyle Hidden
                    Write-Host "Started all tunnels in background" -ForegroundColor Green
                } else {
                    & ngrok start --all
                }
            } else {
                $tunnelArgs = $Tunnels -join " "
                if ($Background) {
                    Start-Process -FilePath "ngrok" -ArgumentList "start $tunnelArgs" -WindowStyle Hidden
                    Write-Host "Started tunnels ($tunnelArgs) in background" -ForegroundColor Green
                } else {
                    & ngrok start $tunnelArgs
                }
            }
            
            # Wait for tunnels to establish
            if ($Background) {
                Write-Host "Waiting for tunnels to establish..." -ForegroundColor Yellow
                Start-Sleep -Seconds $WaitSeconds
                
                # Check tunnel status
                $attempts = 0
                $maxAttempts = 12  # 60 seconds total
                while ($attempts -lt $maxAttempts) {
                    $tunnels = Get-NgrokStatus
                    if ($tunnels -and $tunnels.Count -gt 0) {
                        Write-Host "Tunnels established successfully!" -ForegroundColor Green
                        break
                    }
                    Start-Sleep -Seconds 5
                    $attempts++
                }
                
                if ($ShowUrls) {
                    Show-TunnelUrls
                }
            }
        } catch {
            Write-Error "Failed to start ngrok tunnels: $($_.Exception.Message)"
        }
    }
    
    "stop" {
        Write-Host "`nStopping ngrok tunnels..." -ForegroundColor Cyan
        try {
            Get-Process -Name "ngrok" -ErrorAction SilentlyContinue | Stop-Process -Force
            Write-Host "Ngrok tunnels stopped" -ForegroundColor Green
        } catch {
            Write-Warning "No ngrok processes found or failed to stop"
        }
    }
    
    "restart" {
        Write-Host "`nRestarting ngrok tunnels..." -ForegroundColor Cyan
        
        # Stop existing tunnels
        Get-Process -Name "ngrok" -ErrorAction SilentlyContinue | Stop-Process -Force
        Start-Sleep -Seconds 3
        
        # Start tunnels again
        if ($All) {
            Start-Process -FilePath "ngrok" -ArgumentList "start --all" -WindowStyle Hidden
        } else {
            $tunnelArgs = $Tunnels -join " "
            Start-Process -FilePath "ngrok" -ArgumentList "start $tunnelArgs" -WindowStyle Hidden
        }
        
        Start-Sleep -Seconds $WaitSeconds
        Write-Host "Ngrok tunnels restarted" -ForegroundColor Green
        
        if ($ShowUrls) {
            Show-TunnelUrls
        }
    }
    
    "status" {
        Write-Host "`nChecking ngrok tunnel status..." -ForegroundColor Cyan
        
        # Check if ngrok process is running
        $ngrokProcess = Get-Process -Name "ngrok" -ErrorAction SilentlyContinue
        if ($ngrokProcess) {
            Write-Host "Ngrok process is running (PID: $($ngrokProcess.Id))" -ForegroundColor Green
            
            # Get tunnel details
            Show-TunnelUrls
            
            # Show ngrok web interface URL
            Write-Host "`nNgrok Web Interface: http://localhost:4040" -ForegroundColor Cyan
        } else {
            Write-Host "Ngrok is not running" -ForegroundColor Yellow
        }
    }
    
    "urls" {
        Show-TunnelUrls
    }
    
    default {
        Write-Host "Usage: .\start-ngrok.ps1 -Action <start|stop|restart|status|urls>" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Examples:" -ForegroundColor Cyan
        Write-Host "  .\start-ngrok.ps1 -Action start -All -Background -ShowUrls"
        Write-Host "  .\start-ngrok.ps1 -Action start -Tunnels @('harmoni360-app', 'grafana')"
        Write-Host "  .\start-ngrok.ps1 -Action status"
        Write-Host "  .\start-ngrok.ps1 -Action urls"
        Write-Host "  .\start-ngrok.ps1 -Action stop"
    }
}

# Additional helpful information
if ($Action -eq "start" -and $Background) {
    Write-Host "`n=== Quick Access Commands ===" -ForegroundColor Cyan
    Write-Host "Check tunnel status: .\start-ngrok.ps1 -Action status" -ForegroundColor White
    Write-Host "Show tunnel URLs: .\start-ngrok.ps1 -Action urls" -ForegroundColor White
    Write-Host "Ngrok web interface: http://localhost:4040" -ForegroundColor White
    Write-Host "Stop tunnels: .\start-ngrok.ps1 -Action stop" -ForegroundColor White
}
