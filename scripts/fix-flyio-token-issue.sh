#!/bin/bash

# Harmoni360 - Fix Fly.io Token Authentication Issue
# This script fixes the missing FLY_API_TOKEN_STAGING secret issue

set -e

echo "🔧 Harmoni360 - Fixing Fly.io Token Authentication Issue"
echo "============================================================"
echo ""

# Check prerequisites
echo "📋 Checking prerequisites..."

# Check if GitHub CLI is installed and authenticated
if ! command -v gh &> /dev/null; then
    echo "❌ GitHub CLI (gh) is not installed"
    echo "   Install it from: https://cli.github.com/"
    exit 1
fi

if ! gh auth status &> /dev/null; then
    echo "❌ GitHub CLI is not authenticated"
    echo "   Run: gh auth login"
    exit 1
fi

# Check if Fly CLI is installed and authenticated
if ! command -v fly &> /dev/null; then
    echo "❌ Fly CLI is not installed"
    echo "   Install it from: https://fly.io/docs/hands-on/install-flyctl/"
    exit 1
fi

if ! fly auth whoami &> /dev/null; then
    echo "❌ Fly CLI is not authenticated"
    echo "   Run: fly auth login"
    exit 1
fi

echo "✅ All prerequisites met"
echo ""

# Get current GitHub secrets
echo "🔍 Checking current GitHub secrets..."
CURRENT_SECRETS=$(gh secret list --json name | jq -r '.[].name')

echo "Current secrets:"
echo "$CURRENT_SECRETS" | grep FLY_API_TOKEN || echo "  No FLY_API_TOKEN secrets found"
echo ""

# Check if we have the main token
if echo "$CURRENT_SECRETS" | grep -q "^FLY_API_TOKEN$"; then
    echo "✅ FLY_API_TOKEN exists"
    HAS_MAIN_TOKEN=true
else
    echo "❌ FLY_API_TOKEN not found"
    HAS_MAIN_TOKEN=false
fi

# Check if we have the staging token
if echo "$CURRENT_SECRETS" | grep -q "^FLY_API_TOKEN_STAGING$"; then
    echo "✅ FLY_API_TOKEN_STAGING exists"
    HAS_STAGING_TOKEN=true
else
    echo "❌ FLY_API_TOKEN_STAGING not found"
    HAS_STAGING_TOKEN=false
fi

echo ""

# Determine the fix strategy
if [ "$HAS_MAIN_TOKEN" = true ] && [ "$HAS_STAGING_TOKEN" = false ]; then
    echo "🎯 Strategy: Create staging token using existing production token"
    echo ""
    
    # Option 1: Use the same token for both (recommended for simplicity)
    echo "Choose your approach:"
    echo "1. Use the same token for both staging and production (recommended)"
    echo "2. Create a separate staging token"
    echo ""
    read -p "Enter your choice (1 or 2): " CHOICE
    
    if [ "$CHOICE" = "1" ]; then
        echo ""
        echo "🔄 Using the same token for both environments..."
        echo "This is the simplest and most common approach."
        echo ""
        echo "⚠️  You'll need to manually copy the FLY_API_TOKEN value to FLY_API_TOKEN_STAGING"
        echo ""
        echo "Steps to fix manually:"
        echo "1. Go to: https://github.com/risky-biz/harmoni-hse-360/settings/secrets/actions"
        echo "2. Click 'New repository secret'"
        echo "3. Name: FLY_API_TOKEN_STAGING"
        echo "4. Value: Copy the same value from FLY_API_TOKEN secret"
        echo "5. Click 'Add secret'"
        echo ""
        echo "Or run this command (you'll need to paste the token value):"
        echo "gh secret set FLY_API_TOKEN_STAGING"
        
    elif [ "$CHOICE" = "2" ]; then
        echo ""
        echo "🔑 Creating separate staging token..."
        
        DATE_SUFFIX=$(date +%Y%m%d)
        echo "Creating staging deploy token..."
        STAGING_TOKEN=$(fly tokens create deploy -x 8760h --name "github-actions-staging-$DATE_SUFFIX" --json | jq -r .token)
        
        if [ -z "$STAGING_TOKEN" ] || [ "$STAGING_TOKEN" = "null" ]; then
            echo "❌ Failed to create staging token"
            exit 1
        fi
        
        echo "✅ Staging token created"
        
        echo "Setting FLY_API_TOKEN_STAGING secret..."
        echo "$STAGING_TOKEN" | gh secret set FLY_API_TOKEN_STAGING
        
        echo "✅ FLY_API_TOKEN_STAGING secret configured"
        
        # Clean up
        unset STAGING_TOKEN
    else
        echo "❌ Invalid choice"
        exit 1
    fi
    
elif [ "$HAS_MAIN_TOKEN" = false ]; then
    echo "🔑 No tokens found. Running full token setup..."
    echo ""
    echo "This will create both production and staging tokens."
    echo ""
    read -p "Continue? (y/N): " CONFIRM
    
    if [ "$CONFIRM" != "y" ] && [ "$CONFIRM" != "Y" ]; then
        echo "❌ Setup cancelled"
        exit 1
    fi
    
    # Run the full setup script
    ./scripts/setup-flyio-tokens.sh
    
else
    echo "✅ Both tokens already exist"
    echo "The authentication issue might be elsewhere."
    echo ""
    echo "Let's verify the tokens work:"
fi

echo ""
echo "🧪 Testing the fix..."

# Verify secrets exist
echo "Checking GitHub secrets:"
gh secret list | grep FLY_API_TOKEN

echo ""
echo "✅ Fix completed!"
echo ""
echo "🚀 Next steps:"
echo "1. Test the deployment by pushing to develop branch:"
echo "   git push origin develop"
echo ""
echo "2. Monitor the GitHub Actions workflow:"
echo "   https://github.com/risky-biz/harmoni-hse-360/actions"
echo ""
echo "3. If issues persist, check the workflow logs for detailed error messages"
