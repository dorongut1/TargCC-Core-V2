# Quick Git Init
# Initialize Git repository and make first commit

$projectRoot = Split-Path $PSScriptRoot
Set-Location $projectRoot

Write-Host "Initializing Git repository..." -ForegroundColor Yellow

# Initialize Git
git init

# Add all files
Write-Host "Adding files..." -ForegroundColor Yellow
git add .

# First commit
Write-Host "Creating initial commit..." -ForegroundColor Yellow
git commit -m "Initial commit - TargCC Core V2 project structure"

Write-Host ""
Write-Host "OK Git initialized successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Next: Run connect-github.ps1 to push to GitHub" -ForegroundColor Cyan
