#!/bin/bash

# HarmoniHSE360 Fly.io Token Setup Script
# This script helps set up Fly.io API tokens for GitHub Actions

set -e

echo "🚀 HarmoniHSE360 Fly.io Token Setup"
echo "===================================="

# Check if required tools are installed
echo "🔍 Checking prerequisites..."

if ! command -v fly &> /dev/null; then
    echo "❌ Fly.io CLI not found. Please install it first:"
    echo "   curl -L https://fly.io/install.sh | sh"
    exit 1
fi

if ! command -v gh &> /dev/null; then
    echo "❌ GitHub CLI not found. Please install it first:"
    echo "   https://cli.github.com/"
    exit 1
fi

echo "✅ Fly.io CLI found"
echo "✅ GitHub CLI found"

# Check authentication
echo ""
echo "🔐 Checking authentication..."

if ! fly auth whoami &>/dev/null; then
    echo "❌ Not authenticated with Fly.io. Please run:"
    echo "   fly auth login"
    exit 1
fi

if ! gh auth status &>/dev/null; then
    echo "❌ Not authenticated with GitHub. Please run:"
    echo "   gh auth login"
    exit 1
fi

FLY_USER=$(fly auth whoami)
GH_USER=$(gh api user --jq .login)

echo "✅ Authenticated with Fly.io as: $FLY_USER"
echo "✅ Authenticated with GitHub as: $GH_USER"

# Generate tokens
echo ""
echo "🔑 Generating Fly.io API token..."

DATE_SUFFIX=$(date +%Y%m%d)

echo "Creating deploy token for both staging and production..."
DEPLOY_TOKEN=$(fly tokens create deploy -x 8760h --name "github-actions-deploy-$DATE_SUFFIX" --json | jq -r .token)

if [ -z "$DEPLOY_TOKEN" ] || [ "$DEPLOY_TOKEN" = "null" ]; then
    echo "❌ Failed to generate token"
    exit 1
fi

echo "✅ Deploy token generated"

# Set GitHub secrets
echo ""
echo "🔒 Setting GitHub repository secret..."

echo "Setting FLY_API_TOKEN (used for both staging and production)..."
echo "$DEPLOY_TOKEN" | gh secret set FLY_API_TOKEN

echo "✅ GitHub secret configured"

# Verify setup
echo ""
echo "✅ Verifying setup..."

echo "GitHub secrets:"
gh secret list | grep FLY_API_TOKEN

echo ""
echo "Fly.io tokens:"
fly tokens list | grep github-actions

# Test token
echo ""
echo "🧪 Testing token..."

echo "Testing deploy token..."
if FLY_API_TOKEN="$DEPLOY_TOKEN" fly auth whoami &>/dev/null; then
    echo "✅ Deploy token works"
else
    echo "❌ Deploy token failed"
fi

# Security recommendations
echo ""
echo "🔒 Security Recommendations"
echo "=========================="
echo "1. ✅ Single token for both environments (simplified management)"
echo "2. ✅ Limited to deploy permissions only"
echo "3. ✅ Token expires in 1 year (8760 hours)"
echo "4. ⚠️  Set up token rotation reminder for $(date -d '+1 year' '+%B %Y')"
echo "5. ⚠️  Monitor token usage in Fly.io dashboard"
echo "6. ⚠️  Enable GitHub secret scanning alerts"

# Next steps
echo ""
echo "🎯 Next Steps"
echo "============"
echo "1. Test the CI/CD pipeline:"
echo "   gh workflow run test-tokens.yml"
echo ""
echo "2. Create a test deployment:"
echo "   git checkout -b test-deployment"
echo "   git push origin test-deployment"
echo ""
echo "3. Test staging deployment:"
echo "   git checkout develop"
echo "   git push origin develop"
echo ""
echo "4. Test production deployment:"
echo "   git checkout main"
echo "   git push origin main"
echo ""
echo "5. Set up GitHub environments (optional):"
echo "   - Go to repository Settings > Environments"
echo "   - Create 'staging' and 'production' environments"
echo "   - Move secrets to environment-specific secrets"
echo ""
echo "6. Configure Slack notifications (optional):"
echo "   gh secret set SLACK_WEBHOOK_URL"

echo ""
echo "🎉 Setup completed successfully!"
echo ""
echo "Your Fly.io tokens are now configured for GitHub Actions."
echo "The CI/CD pipeline will automatically deploy:"
echo "  - develop branch → staging environment"
echo "  - main branch → production environment"

# Clean up sensitive variables
unset DEPLOY_TOKEN
