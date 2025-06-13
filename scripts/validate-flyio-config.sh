#!/bin/bash

# Harmoni360 Fly.io Configuration Validation Script
# This script validates that all configuration fixes are in place

set -e

echo "🔍 Validating Harmoni360 Single-App Fly.io Configuration..."
echo "============================================================"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to check if a file exists and contains expected content
check_file_content() {
    local file="$1"
    local pattern="$2"
    local description="$3"
    
    if [ -f "$file" ]; then
        if grep -q "$pattern" "$file"; then
            echo -e "✅ ${GREEN}$description${NC}"
            return 0
        else
            echo -e "❌ ${RED}$description - Pattern not found: $pattern${NC}"
            return 1
        fi
    else
        echo -e "❌ ${RED}$description - File not found: $file${NC}"
        return 1
    fi
}

# Function to check if a file exists
check_file_exists() {
    local file="$1"
    local description="$2"
    
    if [ -f "$file" ]; then
        echo -e "✅ ${GREEN}$description${NC}"
        return 0
    else
        echo -e "❌ ${RED}$description - File not found: $file${NC}"
        return 1
    fi
}

echo "📋 Checking Configuration Files..."
echo "-----------------------------------"

# Check if fly.toml exists
check_file_exists "fly.toml" "Single app fly.toml exists"

# Check app name
check_file_content "fly.toml" "app = \"harmoni-hse-360\"" "fly.toml has correct app name"

# Verify staging configuration is removed (should not exist)
if [ -f "fly.staging.toml" ]; then
    echo -e "⚠️  ${YELLOW}fly.staging.toml still exists - should be removed for single-app strategy${NC}"
else
    echo -e "✅ ${GREEN}fly.staging.toml correctly removed (single-app strategy)${NC}"
fi

# Check Dockerfile.flyio uses correct .NET version
check_file_content "Dockerfile.flyio" "mcr.microsoft.com/dotnet/sdk:8.0-alpine" "Dockerfile.flyio uses .NET 8.0 SDK"
check_file_content "Dockerfile.flyio" "mcr.microsoft.com/dotnet/aspnet:8.0-alpine" "Dockerfile.flyio uses .NET 8.0 runtime"

# Check project targets .NET 8.0
check_file_content "src/Harmoni360.Web/Harmoni360.Web.csproj" "<TargetFramework>net8.0</TargetFramework>" "Project targets .NET 8.0"

# Check GitHub Actions workflow uses correct health check URL (single hostname)
check_file_content ".github/workflows/deploy.yml" "harmoni-hse-360.fly.dev/health" "GitHub Actions uses correct health check URL"

# Verify no references to old staging hostname
if grep -q "harmoni360-staging.fly.dev" ".github/workflows/deploy.yml"; then
    echo -e "❌ ${RED}GitHub Actions still references old staging hostname${NC}"
else
    echo -e "✅ ${GREEN}No references to old staging hostname in GitHub Actions${NC}"
fi

# Check Program.cs has conditional HTTPS redirection
check_file_content "src/Harmoni360.Web/Program.cs" "if (app.Environment.IsDevelopment())" "Program.cs has conditional HTTPS redirection"

# Check Program.cs has startup logging
check_file_content "src/Harmoni360.Web/Program.cs" "Starting Harmoni360 application" "Program.cs has startup logging"

echo ""
echo "🔧 Checking Application Configuration..."
echo "---------------------------------------"

# Check if health checks are configured
check_file_content "src/Harmoni360.Web/Program.cs" "app.MapHealthChecks(\"/health\")" "Health checks endpoint is mapped"

# Check if ASPNETCORE_URLS is set correctly
check_file_content "fly.toml" "ASPNETCORE_URLS = \"http://+:8080\"" "ASPNETCORE_URLS configured correctly"

# Check internal port configuration
check_file_content "fly.toml" "internal_port = 8080" "Internal port configured correctly"

# Check forwarded headers configuration
check_file_content "fly.toml" "ASPNETCORE_FORWARDEDHEADERS_ENABLED = \"true\"" "Forwarded headers enabled"

# Check environment-based deployment configuration
check_file_content ".github/workflows/deploy.yml" "ASPNETCORE_ENVIRONMENT=Staging" "Staging environment deployment configured"
check_file_content ".github/workflows/deploy.yml" "ASPNETCORE_ENVIRONMENT=Production" "Production environment deployment configured"

echo ""
echo "🚀 Deployment Readiness Check..."
echo "--------------------------------"

# Check if fly CLI is available
if command -v flyctl &> /dev/null; then
    echo -e "✅ ${GREEN}Fly CLI is installed${NC}"
    
    # Check if authenticated
    if flyctl auth whoami &> /dev/null; then
        echo -e "✅ ${GREEN}Authenticated with Fly.io${NC}"
    else
        echo -e "⚠️  ${YELLOW}Not authenticated with Fly.io - run 'flyctl auth login'${NC}"
    fi
else
    echo -e "❌ ${RED}Fly CLI not installed - install from https://fly.io/docs/hands-on/install-flyctl/${NC}"
fi

echo ""
echo "📊 Validation Summary"
echo "===================="

# Count total checks and passed checks
total_checks=16
passed_checks=0

# Re-run checks silently to count passes
check_file_exists "fly.toml" "" && ((passed_checks++)) || true
[ ! -f "fly.staging.toml" ] && ((passed_checks++)) || true  # Check staging config is removed
check_file_content "fly.toml" "app = \"harmoni-hse-360\"" "" && ((passed_checks++)) || true
check_file_content "Dockerfile.flyio" "mcr.microsoft.com/dotnet/sdk:8.0-alpine" "" && ((passed_checks++)) || true
check_file_content "Dockerfile.flyio" "mcr.microsoft.com/dotnet/aspnet:8.0-alpine" "" && ((passed_checks++)) || true
check_file_content "src/Harmoni360.Web/Harmoni360.Web.csproj" "<TargetFramework>net8.0</TargetFramework>" "" && ((passed_checks++)) || true
check_file_content ".github/workflows/deploy.yml" "harmoni-hse-360.fly.dev/health" "" && ((passed_checks++)) || true
! grep -q "harmoni360-staging.fly.dev" ".github/workflows/deploy.yml" && ((passed_checks++)) || true  # No old staging hostname
check_file_content "src/Harmoni360.Web/Program.cs" "if (app.Environment.IsDevelopment())" "" && ((passed_checks++)) || true
check_file_content "src/Harmoni360.Web/Program.cs" "Starting Harmoni360 application" "" && ((passed_checks++)) || true
check_file_content "src/Harmoni360.Web/Program.cs" "app.MapHealthChecks(\"/health\")" "" && ((passed_checks++)) || true
check_file_content "fly.toml" "ASPNETCORE_URLS = \"http://+:8080\"" "" && ((passed_checks++)) || true
check_file_content "fly.toml" "internal_port = 8080" "" && ((passed_checks++)) || true
check_file_content "fly.toml" "ASPNETCORE_FORWARDEDHEADERS_ENABLED = \"true\"" "" && ((passed_checks++)) || true
check_file_content ".github/workflows/deploy.yml" "ASPNETCORE_ENVIRONMENT=Staging" "" && ((passed_checks++)) || true
check_file_content ".github/workflows/deploy.yml" "ASPNETCORE_ENVIRONMENT=Production" "" && ((passed_checks++)) || true

echo "Passed: $passed_checks/$total_checks checks"

if [ $passed_checks -eq $total_checks ]; then
    echo -e "🎉 ${GREEN}All configuration checks passed! Ready for deployment.${NC}"
    echo ""
    echo "Next steps (Single-App Strategy):"
    echo "1. Commit and push changes to repository"
    echo "2. Deploy staging: git push origin develop (auto-deploy)"
    echo "3. Deploy production: git push origin main (auto-deploy)"
    echo "4. Monitor deployment: flyctl logs -f -a harmoni-hse-360"
    echo "5. Test health endpoint: curl https://harmoni-hse-360.fly.dev/health"
    echo "6. Verify environment: flyctl ssh console -a harmoni-hse-360 -C 'echo \$ASPNETCORE_ENVIRONMENT'"
    exit 0
else
    echo -e "⚠️  ${YELLOW}Some configuration issues found. Please fix them before deployment.${NC}"
    exit 1
fi
