#!/bin/bash

# Harmoni360 Fly.io Deployment Script
# This script automates the deployment process to Fly.io

set -e  # Exit on any error

echo "🚀 Starting Harmoni360 Fly.io Deployment"
echo "============================================"

# Check if fly CLI is installed
if ! command -v fly &> /dev/null; then
    echo "❌ Fly CLI is not installed. Please install it first:"
    echo "   curl -L https://fly.io/install.sh | sh"
    exit 1
fi

# Check if user is authenticated
if ! fly auth whoami &> /dev/null; then
    echo "❌ Not authenticated with Fly.io. Please run:"
    echo "   fly auth login"
    exit 1
fi

# Variables
APP_NAME="harmoni360-app"
DB_NAME="harmoni360-db"
REDIS_NAME="harmoni360-redis"
REGION="sjc"
VOLUME_NAME="harmoni360_uploads"

echo "📋 Configuration:"
echo "   App Name: $APP_NAME"
echo "   Database: $DB_NAME"
echo "   Redis: $REDIS_NAME"
echo "   Region: $REGION"
echo ""

# Function to check if app exists
app_exists() {
    fly apps list | grep -q "^$1"
}

# Function to check if postgres app exists
postgres_exists() {
    fly postgres list | grep -q "^$1"
}

# Step 1: Create PostgreSQL database if it doesn't exist
echo "🗄️  Setting up PostgreSQL database..."
if postgres_exists "$DB_NAME"; then
    echo "   ✅ Database $DB_NAME already exists"
else
    echo "   📦 Creating PostgreSQL database: $DB_NAME"
    fly postgres create --name "$DB_NAME" --region "$REGION" --initial-cluster-size 1
    echo "   ✅ Database created successfully"
fi

# Step 2: Create Redis instance if it doesn't exist
echo "🔴 Setting up Redis..."
if fly ext redis list | grep -q "$REDIS_NAME"; then
    echo "   ✅ Redis instance $REDIS_NAME already exists"
else
    echo "   📦 Creating Redis instance: $REDIS_NAME"
    fly ext redis create --name "$REDIS_NAME"
    echo "   ✅ Redis created successfully"
fi

# Step 3: Initialize Fly app if it doesn't exist
echo "🛠️  Setting up Fly application..."
if app_exists "$APP_NAME"; then
    echo "   ✅ App $APP_NAME already exists"
else
    echo "   📦 Initializing Fly app: $APP_NAME"
    
    # Check if fly.toml exists, if not copy from example
    if [ ! -f "fly.toml" ]; then
        if [ -f "fly.toml.example" ]; then
            cp fly.toml.example fly.toml
            echo "   📄 Copied fly.toml from example"
        else
            echo "   ❌ fly.toml.example not found. Please create fly.toml manually."
            exit 1
        fi
    fi
    
    fly launch --no-deploy --name "$APP_NAME" --region "$REGION"
    echo "   ✅ App initialized successfully"
fi

# Step 4: Create volume if it doesn't exist
echo "💾 Setting up persistent volume..."
if fly volumes list -a "$APP_NAME" | grep -q "$VOLUME_NAME"; then
    echo "   ✅ Volume $VOLUME_NAME already exists"
else
    echo "   📦 Creating volume: $VOLUME_NAME"
    fly volumes create "$VOLUME_NAME" --region "$REGION" --size 1 -a "$APP_NAME"
    echo "   ✅ Volume created successfully"
fi

# Step 5: Set up secrets (interactive)
echo "🔐 Setting up secrets..."
echo "   Please provide the following configuration values:"

# Get database connection string
echo "   📊 Getting database connection string..."
DB_CONNECTION=$(fly postgres connect -a "$DB_NAME" --command "SELECT 'postgres://' || current_user || ':' || 'password' || '@' || inet_server_addr() || ':' || inet_server_port() || '/' || current_database();" 2>/dev/null | tail -n 1 || echo "")

if [ -z "$DB_CONNECTION" ]; then
    echo "   ⚠️  Could not automatically retrieve database connection. Please set manually:"
    echo "   fly secrets set ConnectionStrings__DefaultConnection=\"postgres://username:password@hostname:5432/database\" -a $APP_NAME"
else
    echo "   ✅ Setting database connection string"
    fly secrets set "ConnectionStrings__DefaultConnection=$DB_CONNECTION" -a "$APP_NAME"
fi

# Get Redis connection string
echo "   🔴 Getting Redis connection string..."
REDIS_CONNECTION=$(fly ext redis status "$REDIS_NAME" --json | jq -r '.redis_url' 2>/dev/null || echo "")

if [ -z "$REDIS_CONNECTION" ]; then
    echo "   ⚠️  Could not automatically retrieve Redis connection. Please set manually:"
    echo "   fly secrets set ConnectionStrings__Redis=\"redis://username:password@hostname:6379\" -a $APP_NAME"
else
    echo "   ✅ Setting Redis connection string"
    fly secrets set "ConnectionStrings__Redis=$REDIS_CONNECTION" -a "$APP_NAME"
fi

# Set JWT key
echo "   🔑 Setting JWT key..."
JWT_KEY="Harmoni360-Production-JWT-Key-$(date +%s)-$(openssl rand -hex 16)"
fly secrets set "Jwt__Key=$JWT_KEY" -a "$APP_NAME"
echo "   ✅ JWT key set"

# Step 6: Deploy application
echo "🚀 Deploying application..."
fly deploy -a "$APP_NAME"

# Step 7: Run database migrations
echo "🔄 Running database migrations..."
fly ssh console -a "$APP_NAME" -C "cd /app && dotnet ef database update"

echo ""
echo "🎉 Deployment completed successfully!"
echo "============================================"
echo "📱 Application URL: https://$APP_NAME.fly.dev"
echo "🔍 Status: fly status -a $APP_NAME"
echo "📊 Logs: fly logs -a $APP_NAME"
echo "🖥️  Dashboard: https://fly.io/apps/$APP_NAME"
echo ""
echo "🔧 Next steps:"
echo "   1. Test the application at the URL above"
echo "   2. Set up custom domain if needed: fly certs create yourdomain.com -a $APP_NAME"
echo "   3. Load demo data: fly ssh console -a $APP_NAME"
echo ""
