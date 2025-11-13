# TargCC Core V2 - Setup Script
# This script initializes the Git repository and restores NuGet packages

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  TargCC Core V2 - Setup Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"
$projectRoot = $PSScriptRoot | Split-Path

# Check if Git is installed
Write-Host "Checking Git installation..." -ForegroundColor Yellow
try {
    $gitVersion = git --version
    Write-Host "OK Git found: $gitVersion" -ForegroundColor Green
}
catch {
    Write-Host "ERROR Git is not installed! Please install Git first." -ForegroundColor Red
    Write-Host "  Download from: https://git-scm.com/downloads" -ForegroundColor Yellow
    exit 1
}

# Check if .NET 8 SDK is installed
Write-Host "Checking .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "OK .NET SDK found: $dotnetVersion" -ForegroundColor Green
    
    if ($dotnetVersion -notmatch "^8\.") {
        Write-Host "WARNING: .NET 8 SDK is recommended. Current version: $dotnetVersion" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "ERROR .NET SDK is not installed! Please install .NET 8 SDK first." -ForegroundColor Red
    Write-Host "  Download from: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    exit 1
}

# Navigate to project root
Set-Location $projectRoot
Write-Host ""
Write-Host "Project root: $projectRoot" -ForegroundColor Cyan
Write-Host ""

# Initialize Git repository
if (-not (Test-Path ".git")) {
    Write-Host "Initializing Git repository..." -ForegroundColor Yellow
    git init
    Write-Host "OK Git repository initialized" -ForegroundColor Green
    
    Write-Host "Adding files to Git..." -ForegroundColor Yellow
    git add .
    
    Write-Host "Creating initial commit..." -ForegroundColor Yellow
    git commit -m "Initial commit - TargCC Core V2 project structure"
    Write-Host "OK Initial commit created" -ForegroundColor Green
}
else {
    Write-Host "OK Git repository already initialized" -ForegroundColor Green
}

Write-Host ""

# Restore NuGet packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore TargCC.Core.sln

if ($LASTEXITCODE -eq 0) {
    Write-Host "OK NuGet packages restored successfully" -ForegroundColor Green
}
else {
    Write-Host "ERROR Failed to restore NuGet packages" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Build the solution
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build TargCC.Core.sln --configuration Debug

if ($LASTEXITCODE -eq 0) {
    Write-Host "OK Solution built successfully" -ForegroundColor Green
}
else {
    Write-Host "ERROR Build failed" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Setup Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Open Visual Studio 2022" -ForegroundColor White
Write-Host "2. File -> Open -> Project/Solution" -ForegroundColor White
Write-Host "3. Select: TargCC.Core.sln" -ForegroundColor White
Write-Host "4. Press F6 to build" -ForegroundColor White
Write-Host ""
Write-Host "To connect to your GitHub repository:" -ForegroundColor Yellow
Write-Host "Run: .\scripts\connect-github.ps1" -ForegroundColor Cyan
Write-Host ""
Write-Host "Happy coding!" -ForegroundColor Green
