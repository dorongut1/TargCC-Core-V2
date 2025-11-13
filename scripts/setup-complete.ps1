# TargCC Core V2 - Complete Setup and GitHub Connection
# This script does EVERYTHING in the right order

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  TargCC Core V2 - Complete Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"
$projectRoot = $PSScriptRoot | Split-Path
Set-Location $projectRoot

Write-Host "Working directory: $projectRoot" -ForegroundColor Cyan
Write-Host ""

# Step 1: Initialize Git if needed
Write-Host "[1/6] Initializing Git..." -ForegroundColor Yellow

if (-not (Test-Path ".git")) {
    git init
    Write-Host "OK Git initialized" -ForegroundColor Green
} else {
    Write-Host "OK Git already initialized" -ForegroundColor Green
}

Write-Host ""

# Step 2: Add all files
Write-Host "[2/6] Adding files to Git..." -ForegroundColor Yellow
git add .
Write-Host "OK Files added" -ForegroundColor Green
Write-Host ""

# Step 3: Create initial commit
Write-Host "[3/6] Creating initial commit..." -ForegroundColor Yellow

$status = git status --short
if ($status) {
    git commit -m "Initial commit - TargCC Core V2 project structure"
    Write-Host "OK Initial commit created" -ForegroundColor Green
} else {
    Write-Host "OK No changes to commit" -ForegroundColor Green
}

Write-Host ""

# Step 4: Rename branch to main
Write-Host "[4/6] Setting branch to 'main'..." -ForegroundColor Yellow
git branch -M main
Write-Host "OK Branch renamed to 'main'" -ForegroundColor Green
Write-Host ""

# Step 5: Add GitHub remote
Write-Host "[5/6] Connecting to GitHub..." -ForegroundColor Yellow

$githubRepo = "https://github.com/dorongut1/TargCC-Core-V2.git"
$remotes = git remote -v 2>&1

if ($remotes -match "origin") {
    Write-Host "Removing existing remote..." -ForegroundColor Yellow
    git remote remove origin
}

git remote add origin $githubRepo
Write-Host "OK Remote added: $githubRepo" -ForegroundColor Green
Write-Host ""

# Step 6: Push to GitHub
Write-Host "[6/6] Pushing to GitHub..." -ForegroundColor Yellow
Write-Host "Note: You may be asked for GitHub credentials" -ForegroundColor Cyan
Write-Host ""

try {
    git push -u origin main
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  SUCCESS!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Your code is now on GitHub!" -ForegroundColor Green
    Write-Host "  $githubRepo" -ForegroundColor Cyan
    Write-Host ""
    
} catch {
    Write-Host ""
    Write-Host "Push failed - this is usually an authentication issue" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Solutions:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Option 1 - Personal Access Token (Recommended):" -ForegroundColor Cyan
    Write-Host "  1. Go to: https://github.com/settings/tokens" -ForegroundColor White
    Write-Host "  2. Click 'Generate new token (classic)'" -ForegroundColor White
    Write-Host "  3. Select 'repo' permissions" -ForegroundColor White
    Write-Host "  4. Copy the token" -ForegroundColor White
    Write-Host "  5. When pushing, use token as password" -ForegroundColor White
    Write-Host ""
    Write-Host "Option 2 - Git Credential Manager:" -ForegroundColor Cyan
    Write-Host "  git config --global credential.helper wincred" -ForegroundColor White
    Write-Host "  Then run: git push -u origin main" -ForegroundColor White
    Write-Host ""
    Write-Host "Option 3 - SSH Keys:" -ForegroundColor Cyan
    Write-Host "  Change remote to SSH:" -ForegroundColor White
    Write-Host "  git remote set-url origin git@github.com:dorongut1/TargCC-Core-V2.git" -ForegroundColor White
    Write-Host ""
}

Write-Host ""
Write-Host "Current Git status:" -ForegroundColor Yellow
git status
Write-Host ""
Write-Host "Remote configuration:" -ForegroundColor Yellow
git remote -v
Write-Host ""
