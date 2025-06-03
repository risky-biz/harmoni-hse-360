# PowerShell script to create and apply database migrations

Write-Host "Creating database migration for HarmoniHSE360..." -ForegroundColor Green

# Navigate to Web project
Set-Location src\HarmoniHSE360.Web

# Create migration
Write-Host "Creating InitialCreate migration..." -ForegroundColor Yellow
dotnet ef migrations add InitialCreate -p ..\HarmoniHSE360.Infrastructure -s . -c ApplicationDbContext

# Apply migration
Write-Host "Applying migration to database..." -ForegroundColor Yellow
dotnet ef database update -p ..\HarmoniHSE360.Infrastructure -s . -c ApplicationDbContext

Write-Host "Migration completed successfully!" -ForegroundColor Green

# Return to root
Set-Location ..\..