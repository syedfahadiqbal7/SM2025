# Production Environment Deployment Script
# This script deploys the SmartCenter applications to Production environment

param(
    [string]$BuildPath = ".",
    [string]$Environment = "production"
)

Write-Host "Starting Production Environment Deployment..." -ForegroundColor Green
Write-Host "Build Path: $BuildPath" -ForegroundColor Yellow
Write-Host "Environment: $Environment" -ForegroundColor Yellow

try {
    # Create Production directory if it doesn't exist
    if (!(Test-Path $Environment)) {
        New-Item -ItemType Directory -Path $Environment -Force
        Write-Host "Created $Environment directory" -ForegroundColor Green
    }

    # Create application-specific directories
    $apps = @("AFFZ_API", "AFFZ_Admin", "AFFZ_Customer", "AFFZ_Provider")
    foreach ($app in $apps) {
        $appPath = ".\$Environment\$app"
        if (!(Test-Path $appPath)) {
            New-Item -ItemType Directory -Path $appPath -Force
        }
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
    
    Write-Host "Production Environment Deployment completed successfully!" -ForegroundColor Green
    
    # Display deployment summary
    Write-Host "`nProduction Deployment Summary:" -ForegroundColor Yellow
    Get-ChildItem -Path $Environment -Directory | ForEach-Object {
        $size = (Get-ChildItem -Path $_.FullName -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
        Write-Host "$($_.Name): $([math]::Round($size, 2)) MB" -ForegroundColor White
    }
    
    # Create deployment manifest
    $manifest = @{
        DeploymentDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        Environment = $Environment
        Applications = $apps
        BuildPath = $BuildPath
        Status = "Success"
    }
    
    $manifest | ConvertTo-Json | Out-File ".\$Environment\deployment-manifest.json" -Encoding UTF8
    Write-Host "`nDeployment manifest created: .\$Environment\deployment-manifest.json" -ForegroundColor Green
    
} catch {
    Write-Host "Production deployment failed with error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
