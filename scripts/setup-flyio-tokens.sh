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
echo "🔑 Generating Fly.io API tokens..."

DATE_SUFFIX=$(date +%Y%m%d)

echo "Creating production deploy token..."
PROD_TOKEN=$(fly tokens create deploy -x 8760h --name "github-actions-production-$DATE_SUFFIX" --json | jq -r .token)

echo "Creating staging deploy token..."
STAGING_TOKEN=$(fly tokens create deploy -x 8760h --name "github-actions-staging-$DATE_SUFFIX" --json | jq -r .token)

if [ -z "$PROD_TOKEN" ] || [ -z "$STAGING_TOKEN" ]; then
    echo "❌ Failed to generate tokens"
    exit 1
fi

echo "✅ Production token generated"
echo "✅ Staging token generated"

# Set GitHub secrets
echo ""
echo "🔒 Setting GitHub repository secrets..."

echo "Setting FLY_API_TOKEN (production)..."
echo "$PROD_TOKEN" | gh secret set FLY_API_TOKEN

echo "Setting FLY_API_TOKEN_STAGING..."
echo "$STAGING_TOKEN" | gh secret set FLY_API_TOKEN_STAGING

echo "✅ GitHub secrets configured"

# Verify setup
echo ""
echo "✅ Verifying setup..."

echo "GitHub secrets:"
gh secret list | grep FLY_API_TOKEN

echo ""
echo "Fly.io tokens:"
fly tokens list | grep github-actions

# Test tokens
echo ""
echo "🧪 Testing tokens..."

echo "Testing production token..."
if FLY_API_TOKEN="$PROD_TOKEN" fly auth whoami &>/dev/null; then
    echo "✅ Production token works"
else
    echo "❌ Production token failed"
fi

echo "Testing staging token..."
if FLY_API_TOKEN="$STAGING_TOKEN" fly auth whoami &>/dev/null; then
    echo "✅ Staging token works"
else
    echo "❌ Staging token failed"
fi

# Security recommendations
echo ""
echo "🔒 Security Recommendations"
echo "=========================="
echo "1. ✅ Separate tokens for staging and production"
echo "2. ✅ Limited to deploy permissions only"
echo "3. ✅ Tokens expire in 1 year (8760 hours)"
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
unset PROD_TOKEN
unset STAGING_TOKEN
