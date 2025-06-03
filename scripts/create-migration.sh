#!/bin/bash
# Script to create and apply database migrations

echo "Creating database migration for HarmoniHSE360..."

# Check if running in Docker or local
if [ -f /.dockerenv ]; then
    echo "Running in Docker container..."
    cd /src/HarmoniHSE360.Web
else
    echo "Running locally..."
    cd src/HarmoniHSE360.Web
fi

# Create migration
echo "Creating InitialCreate migration..."
dotnet ef migrations add InitialCreate -p ../HarmoniHSE360.Infrastructure -s . -c ApplicationDbContext

# Apply migration
echo "Applying migration to database..."
dotnet ef database update -p ../HarmoniHSE360.Infrastructure -s . -c ApplicationDbContext

echo "Migration completed successfully!"