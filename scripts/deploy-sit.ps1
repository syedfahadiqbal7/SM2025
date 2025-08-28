# SIT Environment Deployment Script
# This script deploys the SmartCenter applications to SIT environment

param(
    [string]$BuildPath = ".",
    [string]$Environment = "sit"
)

Write-Host "Starting SIT Environment Deployment..." -ForegroundColor Green
Write-Host "Build Path: $BuildPath" -ForegroundColor Yellow
Write-Host "Environment: $Environment" -ForegroundColor Yellow

try {
    # Create SIT directory if it doesn't exist
    if (!(Test-Path $Environment)) {
        New-Item -ItemType Directory -Path $Environment -Force
        Write-Host "Created $Environment directory" -ForegroundColor Green
    }

    # Deploy AFFZ_API
    Write-Host "Deploying AFFZ_API..." -ForegroundColor Cyan
    dotnet publish "$BuildPath\AFFZ_API\AFFZ_API.csproj" -c Release -o ".\$Environment\AFFZ_API"
    
    # Deploy AFFZ_Admin
    Write-Host "Deploying AFFZ_Admin..." -ForegroundColor Cyan
    dotnet publish "$BuildPath\AFFZ_Admin\AFFZ_Admin.csproj" -c Release -o ".\$Environment\AFFZ_Admin"
    
    # Deploy AFFZ_Customer
    Write-Host "Deploying AFFZ_Customer..." -ForegroundColor Cyan
    dotnet publish "$BuildPath\AFFZ_MVC\AFFZ_Customer.csproj" -c Release -o ".\$Environment\AFFZ_Customer"
    
    # Deploy AFFZ_Provider
    Write-Host "Deploying AFFZ_Provider..." -ForegroundColor Cyan
    dotnet publish "$BuildPath\AFFZ_Provider\AFFZ_Provider.csproj" -c Release -o ".\$Environment\AFFZ_Provider"
    
    # Copy environment-specific configuration
    Write-Host "Copying environment configuration..." -ForegroundColor Cyan
    Copy-Item ".\environments\$Environment\appsettings.json" ".\$Environment\AFFZ_API\" -Force
    Copy-Item ".\environments\$Environment\appsettings.json" ".\$Environment\AFFZ_Admin\" -Force
    Copy-Item ".\environments\$Environment\appsettings.json" ".\$Environment\AFFZ_Customer\" -Force
    Copy-Item ".\environments\$Environment\appsettings.json" ".\$Environment\AFFZ_Provider\" -Force
    
    Write-Host "SIT Environment Deployment completed successfully!" -ForegroundColor Green
    
    # Display deployment summary
    Write-Host "`nDeployment Summary:" -ForegroundColor Yellow
    Get-ChildItem -Path $Environment -Directory | ForEach-Object {
        $size = (Get-ChildItem -Path $_.FullName -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
        Write-Host "$($_.Name): $([math]::Round($size, 2)) MB" -ForegroundColor White
    }
    
} catch {
    Write-Host "Deployment failed with error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
