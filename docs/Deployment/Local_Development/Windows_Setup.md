# Windows 11 Setup for Local Development

## Overview

This guide provides step-by-step instructions for configuring Windows 11 to run the complete HarmoniHSE360 Docker Standalone simulation, including Hyper-V, Docker Desktop, and all required development tools.

## 🎯 Prerequisites

### System Requirements Verification
```powershell
# Check Windows version (must be Windows 11 Pro/Enterprise)
Get-ComputerInfo | Select-Object WindowsProductName, WindowsVersion

# Check if Hyper-V is supported
Get-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V-All

# Check available memory (32GB minimum recommended)
Get-CimInstance -ClassName Win32_PhysicalMemory | Measure-Object -Property Capacity -Sum | ForEach-Object {[math]::Round($_.Sum / 1GB, 2)}

# Check CPU cores (8 minimum recommended)
Get-CimInstance -ClassName Win32_Processor | Select-Object Name, NumberOfCores, NumberOfLogicalProcessors

# Check available disk space (500GB minimum recommended)
Get-WmiObject -Class Win32_LogicalDisk | Select-Object DeviceID, @{Name="Size(GB)";Expression={[math]::Round($_.Size / 1GB, 2)}}, @{Name="FreeSpace(GB)";Expression={[math]::Round($_.FreeSpace / 1GB, 2)}}
```

### Required Windows Editions
- ✅ Windows 11 Pro
- ✅ Windows 11 Enterprise
- ✅ Windows 11 Education
- ❌ Windows 11 Home (Hyper-V not supported)

## 🚀 Phase 1: Enable Windows Features (15-20 minutes)

### 1.1 Enable Hyper-V and Containers
```powershell
# Run PowerShell as Administrator
# Enable Hyper-V (required for Docker Desktop)
Enable-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V -All -NoRestart

# Enable Windows Subsystem for Linux v2
Enable-WindowsOptionalFeature -Online -FeatureName Microsoft-Windows-Subsystem-Linux -NoRestart

# Enable Virtual Machine Platform
Enable-WindowsOptionalFeature -Online -FeatureName VirtualMachinePlatform -NoRestart

# Enable Containers feature
Enable-WindowsOptionalFeature -Online -FeatureName Containers -All -NoRestart

# Restart computer to apply changes
Restart-Computer
```

### 1.2 Verify Feature Installation
```powershell
# After restart, verify Hyper-V is enabled
Get-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V-All

# Check if WSL2 is available
wsl --status

# Verify virtualization is enabled in BIOS
Get-ComputerInfo | Select-Object HyperVRequirementVirtualizationFirmwareEnabled, HyperVRequirementSecondLevelAddressTranslation
```

## 🐳 Phase 2: Docker Desktop Installation (20-30 minutes)

### 2.1 Install Docker Desktop

#### Option A: Direct Download
```powershell
# Download Docker Desktop installer
$url = "https://desktop.docker.com/win/main/amd64/Docker%20Desktop%20Installer.exe"
$output = "$env:TEMP\DockerDesktopInstaller.exe"
Invoke-WebRequest -Uri $url -OutFile $output

# Run installer
Start-Process -FilePath $output -Wait

# Clean up installer
Remove-Item $output
```

#### Option B: Using Chocolatey (Recommended)
```powershell
# Install Chocolatey package manager
Set-ExecutionPolicy Bypass -Scope Process -Force
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))

# Install Docker Desktop
choco install docker-desktop -y

# Refresh environment variables
refreshenv
```

### 2.2 Configure Docker Desktop

#### Initial Configuration
1. **Start Docker Desktop** from Start Menu
2. **Accept License Agreement**
3. **Choose Configuration**:
   - ✅ Use WSL 2 instead of Hyper-V (recommended)
   - ✅ Add shortcut to desktop
   - ✅ Use Docker Compose V2

#### Resource Configuration
```json
// Docker Desktop Settings → Resources → Advanced
{
  "cpus": 6,
  "memory": 20480,
  "swap": 4096,
  "disk": 200,
  "experimental": false,
  "debug": false
}
```

#### WSL Integration
```json
// Docker Desktop Settings → Resources → WSL Integration
{
  "enableIntegration": true,
  "enableDistroIntegration": {
    "Ubuntu": true,
    "Ubuntu-20.04": true,
    "Ubuntu-22.04": true
  }
}
```

### 2.3 Verify Docker Installation
```powershell
# Check Docker version
docker --version

# Check Docker Compose version
docker compose version

# Test Docker functionality
docker run hello-world

# Check Docker system information
docker system info

# Verify Docker Desktop is running
Get-Process "Docker Desktop" -ErrorAction SilentlyContinue
```

## 🛠️ Phase 3: Development Tools Installation (15-25 minutes)

### 3.1 Install Essential Tools
```powershell
# Install Git for version control
choco install git -y

# Install Node.js (required for ngrok and development tools)
choco install nodejs -y

# Install .NET 8 SDK (for local development and debugging)
choco install dotnet-8.0-sdk -y

# Install PowerShell 7 (enhanced PowerShell)
choco install powershell-core -y

# Install Windows Terminal (better terminal experience)
choco install microsoft-windows-terminal -y

# Install Visual Studio Code (optional but recommended)
choco install vscode -y

# Refresh environment variables
refreshenv
```

### 3.2 Configure Git
```powershell
# Configure Git with your information
git config --global user.name "Your Name"
git config --global user.email "your.email@company.com"

# Configure Git line endings for Windows
git config --global core.autocrlf true

# Configure Git to use Windows Credential Manager
git config --global credential.helper manager-core
```

### 3.3 Install Additional Development Tools
```powershell
# Install curl for API testing
choco install curl -y

# Install jq for JSON processing
choco install jq -y

# Install 7zip for archive handling
choco install 7zip -y

# Install Notepad++ for text editing
choco install notepadplusplus -y

# Install Postman for API testing (optional)
choco install postman -y
```

## ⚙️ Phase 4: System Optimization (10-15 minutes)

### 4.1 Configure Windows Performance
```powershell
# Set Windows power plan to High Performance
powercfg /setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c

# Disable Windows Search indexing for Docker volumes (optional)
# This can improve Docker performance
Set-Service -Name "WSearch" -StartupType Disabled
Stop-Service -Name "WSearch" -Force

# Configure Windows Defender exclusions for Docker
Add-MpPreference -ExclusionPath "C:\ProgramData\Docker"
Add-MpPreference -ExclusionPath "C:\Users\$env:USERNAME\.docker"
Add-MpPreference -ExclusionPath "C:\ProgramData\DockerDesktop"
```

### 4.2 Configure Virtual Memory
```powershell
# Check current virtual memory settings
Get-CimInstance -ClassName Win32_PageFileUsage

# Increase virtual memory if needed (for systems with 32GB RAM)
# Note: This requires manual configuration through System Properties
Write-Host "Consider increasing virtual memory to 48GB (1.5x physical RAM) through System Properties → Advanced → Performance Settings → Advanced → Virtual Memory"
```

### 4.3 Configure Network Settings
```powershell
# Configure DNS for better performance (optional)
# Set primary DNS to Cloudflare
netsh interface ip set dns "Wi-Fi" static 1.1.1.1
netsh interface ip add dns "Wi-Fi" 1.0.0.1 index=2

# Or set to Google DNS
# netsh interface ip set dns "Wi-Fi" static 8.8.8.8
# netsh interface ip add dns "Wi-Fi" 8.8.4.4 index=2
```

## 🔧 Phase 5: Docker Desktop Optimization (10-15 minutes)

### 5.1 Configure Docker Settings

#### File Sharing Configuration
```powershell
# Ensure C: drive is shared with Docker
# Docker Desktop → Settings → Resources → File Sharing
# Add C:\ if not already present
```

#### Docker Daemon Configuration
Create or edit `%USERPROFILE%\.docker\daemon.json`:
```json
{
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "100m",
    "max-file": "5"
  },
  "storage-driver": "windowsfilter",
  "experimental": false,
  "debug": false,
  "registry-mirrors": [],
  "insecure-registries": [],
  "dns": ["1.1.1.1", "8.8.8.8"],
  "data-root": "C:\\ProgramData\\Docker"
}
```

### 5.2 Test Docker Performance
```powershell
# Test Docker performance with a simple container
docker run --rm -it alpine:latest sh -c "echo 'Docker is working correctly'"

# Test Docker Compose functionality
$testCompose = @"
version: '3.8'
services:
  test:
    image: alpine:latest
    command: echo 'Docker Compose is working correctly'
"@

$testCompose | Out-File -FilePath "$env:TEMP\test-compose.yml" -Encoding UTF8
docker compose -f "$env:TEMP\test-compose.yml" up
Remove-Item "$env:TEMP\test-compose.yml"
```

## ✅ Phase 6: Verification and Testing (10-15 minutes)

### 6.1 System Verification Checklist
```powershell
# Create verification script
$verificationScript = @"
Write-Host "=== HarmoniHSE360 Local Development Environment Verification ===" -ForegroundColor Green

# Check Windows version
Write-Host "`nWindows Version:" -ForegroundColor Yellow
Get-ComputerInfo | Select-Object WindowsProductName, WindowsVersion

# Check Hyper-V status
Write-Host "`nHyper-V Status:" -ForegroundColor Yellow
Get-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V-All | Select-Object State

# Check available resources
Write-Host "`nSystem Resources:" -ForegroundColor Yellow
Write-Host "RAM: $([math]::Round((Get-CimInstance -ClassName Win32_PhysicalMemory | Measure-Object -Property Capacity -Sum).Sum / 1GB, 2)) GB"
Write-Host "CPU Cores: $((Get-CimInstance -ClassName Win32_Processor).NumberOfCores)"
Write-Host "Logical Processors: $((Get-CimInstance -ClassName Win32_Processor).NumberOfLogicalProcessors)"

# Check Docker
Write-Host "`nDocker Status:" -ForegroundColor Yellow
try {
    docker --version
    docker compose version
    Write-Host "Docker is running: $(if (Get-Process 'Docker Desktop' -ErrorAction SilentlyContinue) { 'Yes' } else { 'No' })"
} catch {
    Write-Host "Docker not available" -ForegroundColor Red
}

# Check development tools
Write-Host "`nDevelopment Tools:" -ForegroundColor Yellow
try { git --version } catch { Write-Host "Git not installed" -ForegroundColor Red }
try { node --version } catch { Write-Host "Node.js not installed" -ForegroundColor Red }
try { dotnet --version } catch { Write-Host ".NET SDK not installed" -ForegroundColor Red }

Write-Host "`n=== Verification Complete ===" -ForegroundColor Green
"@

# Save and run verification script
$verificationScript | Out-File -FilePath "$env:TEMP\verify-setup.ps1" -Encoding UTF8
& "$env:TEMP\verify-setup.ps1"
Remove-Item "$env:TEMP\verify-setup.ps1"
```

### 6.2 Performance Baseline
```powershell
# Create performance baseline script
$performanceScript = @"
Write-Host "=== Performance Baseline ===" -ForegroundColor Green

# CPU performance test
Write-Host "`nCPU Performance Test:" -ForegroundColor Yellow
Measure-Command { 1..1000000 | ForEach-Object { [math]::Sqrt($_) } } | Select-Object TotalSeconds

# Memory test
Write-Host "`nMemory Information:" -ForegroundColor Yellow
Get-CimInstance -ClassName Win32_OperatingSystem | Select-Object TotalVisibleMemorySize, FreePhysicalMemory

# Disk performance test
Write-Host "`nDisk Performance Test:" -ForegroundColor Yellow
Measure-Command { 
    $testFile = "$env:TEMP\disktest.tmp"
    1..1000 | ForEach-Object { "Test data line $_" } | Out-File $testFile
    Get-Content $testFile | Out-Null
    Remove-Item $testFile
} | Select-Object TotalSeconds

Write-Host "`n=== Baseline Complete ===" -ForegroundColor Green
"@

$performanceScript | Out-File -FilePath "$env:TEMP\performance-baseline.ps1" -Encoding UTF8
& "$env:TEMP\performance-baseline.ps1"
Remove-Item "$env:TEMP\performance-baseline.ps1"
```

## 🔧 Troubleshooting Common Issues

### Issue: Hyper-V Not Available
```powershell
# Check if virtualization is enabled in BIOS
Get-ComputerInfo | Select-Object HyperVRequirementVirtualizationFirmwareEnabled

# If false, enable virtualization in BIOS settings
# Restart computer and enter BIOS/UEFI settings
# Enable Intel VT-x or AMD-V
```

### Issue: Docker Desktop Won't Start
```powershell
# Reset Docker Desktop
& "C:\Program Files\Docker\Docker\Docker Desktop.exe" --reset-to-factory

# Check Windows services
Get-Service | Where-Object { $_.Name -like "*docker*" }

# Restart Docker services
Restart-Service -Name "com.docker.service"
```

### Issue: Insufficient Resources
```powershell
# Check current Docker resource allocation
docker system df

# Clean up Docker resources
docker system prune -a -f
docker volume prune -f
```

## 🎯 Next Steps

After completing Windows setup:

1. **Configure Ngrok**: Follow [Ngrok Configuration Guide](./Ngrok_Configuration.md)
2. **Set Up Environment**: Follow [Environment Configuration Guide](./Environment_Configuration.md)
3. **Deploy Local Stack**: Use the docker-compose.local.yml configuration
4. **Test Setup**: Follow [Testing Procedures](./Testing_Procedures.md)

## 📞 Support Resources

### Windows 11 Resources
- [Windows 11 Hyper-V Documentation](https://docs.microsoft.com/en-us/virtualization/hyper-v-on-windows/)
- [Windows 11 System Requirements](https://docs.microsoft.com/en-us/windows/whats-new/windows-11-requirements)

### Docker Resources
- [Docker Desktop for Windows](https://docs.docker.com/desktop/windows/)
- [Docker Desktop Troubleshooting](https://docs.docker.com/desktop/troubleshoot/)

### Community Support
- [Docker Community Forums](https://forums.docker.com/)
- [Windows 11 Community](https://answers.microsoft.com/en-us/windows)

---

*Previous: [Local Development README](./README.md) | Next: [Ngrok Configuration](./Ngrok_Configuration.md)*
