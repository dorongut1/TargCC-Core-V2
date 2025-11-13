# TargCC Core V2 - Connect to GitHub
# This script connects your local repository to GitHub

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Connect to GitHub" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"
$projectRoot = $PSScriptRoot | Split-Path

Set-Location $projectRoot

# Check if Git is initialized
if (-not (Test-Path ".git")) {
    Write-Host "ERROR: Git repository not initialized!" -ForegroundColor Red
    Write-Host "Run setup.ps1 first" -ForegroundColor Yellow
    exit 1
}

# GitHub repository URL
$githubRepo = "https://github.com/dorongut1/TargCC-Core-V2.git"

Write-Host "Connecting to GitHub repository:" -ForegroundColor Yellow
Write-Host "  $githubRepo" -ForegroundColor Cyan
Write-Host ""

# Check if remote already exists
$remotes = git remote -v 2>&1

if ($remotes -match "origin") {
    Write-Host "Remote 'origin' already exists. Removing..." -ForegroundColor Yellow
    git remote remove origin
}

# Add remote
Write-Host "Adding remote 'origin'..." -ForegroundColor Yellow
git remote add origin $githubRepo

# Set main branch
Write-Host "Setting branch to 'main'..." -ForegroundColor Yellow
git branch -M main

# Try to push
Write-Host ""
Write-Host "Pushing to GitHub..." -ForegroundColor Yellow
Write-Host "You may be asked for your GitHub credentials" -ForegroundColor Cyan
Write-Host ""

$pushResult = git push -u origin main 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  SUCCESS!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Your code is now on GitHub:" -ForegroundColor Green
    Write-Host "  $githubRepo" -ForegroundColor Cyan
    Write-Host ""
}
else {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Yellow
    Write-Host "  Push Info" -ForegroundColor Yellow
    Write-Host "========================================" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "If you see authentication errors:" -ForegroundColor Yellow
    Write-Host "1. Make sure you have a GitHub Personal Access Token" -ForegroundColor White
    Write-Host "2. Go to: https://github.com/settings/tokens" -ForegroundColor Cyan
    Write-Host "3. Generate new token (classic)" -ForegroundColor White
    Write-Host "4. Select 'repo' permissions" -ForegroundColor White
    Write-Host "5. Use the token as your password" -ForegroundColor White
    Write-Host ""
    Write-Host "Or configure Git credentials:" -ForegroundColor Yellow
    Write-Host "  git config --global credential.helper wincred" -ForegroundColor Cyan
    Write-Host ""
}

Write-Host "Remote configuration:" -ForegroundColor Yellow
git remote -v
Write-Host ""
