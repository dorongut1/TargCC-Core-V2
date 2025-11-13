# TargCC Core V2 - Complete Setup with Git Config Check
# This script does EVERYTHING including Git user configuration

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  TargCC Core V2 - Complete Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Continue"
$projectRoot = $PSScriptRoot | Split-Path
Set-Location $projectRoot

Write-Host "Working directory: $projectRoot" -ForegroundColor Cyan
Write-Host ""

# Step 0: Check and configure Git user
Write-Host "[0/7] Checking Git configuration..." -ForegroundColor Yellow

$gitUserName = git config --global user.name 2>$null
$gitUserEmail = git config --global user.email 2>$null

if (-not $gitUserName) {
    Write-Host "Git user.name not set. Configuring..." -ForegroundColor Yellow
    git config --global user.name "Doron Gutfreund"
    Write-Host "OK Set user.name to: Doron Gutfreund" -ForegroundColor Green
} else {
    Write-Host "OK Git user.name: $gitUserName" -ForegroundColor Green
}

if (-not $gitUserEmail) {
    Write-Host "Git user.email not set. Configuring..." -ForegroundColor Yellow
    git config --global user.email "dorongut1@gmail.com"
    Write-Host "OK Set user.email to: dorongut1@gmail.com" -ForegroundColor Green
} else {
    Write-Host "OK Git user.email: $gitUserEmail" -ForegroundColor Green
}

Write-Host ""

# Step 1: Initialize Git if needed
Write-Host "[1/7] Initializing Git..." -ForegroundColor Yellow

if (-not (Test-Path ".git")) {
    git init
    Write-Host "OK Git initialized" -ForegroundColor Green
} else {
    Write-Host "OK Git already initialized" -ForegroundColor Green
}

Write-Host ""

# Step 2: Add all files
Write-Host "[2/7] Adding files to Git..." -ForegroundColor Yellow
git add . 2>&1 | Out-Null
Write-Host "OK Files added" -ForegroundColor Green
Write-Host ""

# Step 3: Show what will be committed
Write-Host "[3/7] Files to commit:" -ForegroundColor Yellow
$filesToCommit = git status --short
if ($filesToCommit) {
    Write-Host $filesToCommit -ForegroundColor White
    Write-Host ""
} else {
    Write-Host "No files to commit" -ForegroundColor Yellow
    Write-Host ""
}

# Step 4: Create initial commit
Write-Host "[4/7] Creating initial commit..." -ForegroundColor Yellow

try {
    $commitOutput = git commit -m "Initial commit - TargCC Core V2 project structure" 2>&1
    Write-Host $commitOutput -ForegroundColor White
    Write-Host "OK Initial commit created" -ForegroundColor Green
} catch {
    Write-Host "ERROR: Failed to create commit" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

Write-Host ""

# Verify commit was created
$commitCount = git rev-list --count HEAD 2>$null
if ($commitCount -gt 0) {
    Write-Host "OK Verified: $commitCount commit(s) exist" -ForegroundColor Green
} else {
    Write-Host "ERROR: No commits found!" -ForegroundColor Red
    Write-Host "Git status:" -ForegroundColor Yellow
    git status
    exit 1
}

Write-Host ""

# Step 5: Rename branch to main
Write-Host "[5/7] Setting branch to 'main'..." -ForegroundColor Yellow
git branch -M main 2>&1 | Out-Null
Write-Host "OK Branch renamed to 'main'" -ForegroundColor Green
Write-Host ""

# Step 6: Add GitHub remote
Write-Host "[6/7] Connecting to GitHub..." -ForegroundColor Yellow

$githubRepo = "https://github.com/dorongut1/TargCC-Core-V2.git"
$remotes = git remote -v 2>&1

if ($remotes -match "origin") {
    Write-Host "Removing existing remote..." -ForegroundColor Yellow
    git remote remove origin 2>&1 | Out-Null
}

git remote add origin $githubRepo 2>&1 | Out-Null
Write-Host "OK Remote added: $githubRepo" -ForegroundColor Green
Write-Host ""

# Step 7: Push to GitHub
Write-Host "[7/7] Pushing to GitHub..." -ForegroundColor Yellow
Write-Host ""
Write-Host "IMPORTANT: When asked for password, use your Personal Access Token!" -ForegroundColor Cyan
Write-Host "  NOT your GitHub password - use the TOKEN you created" -ForegroundColor Yellow
Write-Host ""
Write-Host "Username: dorongut1" -ForegroundColor White
Write-Host "Password: [PASTE YOUR TOKEN HERE]" -ForegroundColor White
Write-Host ""
Write-Host "Pushing..." -ForegroundColor Yellow

$pushOutput = git push -u origin main 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  SUCCESS! All Done!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Your code is now on GitHub!" -ForegroundColor Green
    Write-Host "  $githubRepo" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. Open Visual Studio 2022" -ForegroundColor White
    Write-Host "2. Open: TargCC.Core.sln" -ForegroundColor White
    Write-Host "3. Press F6 to build" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "Push output:" -ForegroundColor Yellow
    Write-Host $pushOutput -ForegroundColor White
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Yellow
    Write-Host "  Authentication Help" -ForegroundColor Yellow
    Write-Host "========================================" -ForegroundColor Yellow
    Write-Host ""
    
    if ($pushOutput -match "authentication failed" -or $pushOutput -match "Authentication failed") {
        Write-Host "Your token might be incorrect. Let's try again:" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "1. Make sure your token has 'repo' permissions" -ForegroundColor White
        Write-Host "2. Copy the token again (it's case-sensitive!)" -ForegroundColor White
        Write-Host "3. Run this command manually:" -ForegroundColor White
        Write-Host ""
        Write-Host "   git push -u origin main" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "4. When prompted:" -ForegroundColor White
        Write-Host "   Username: dorongut1" -ForegroundColor Cyan
        Write-Host "   Password: [PASTE YOUR TOKEN]" -ForegroundColor Cyan
        Write-Host ""
    } else {
        Write-Host "If you don't have a token yet:" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "1. Go to: https://github.com/settings/tokens" -ForegroundColor Cyan
        Write-Host "2. Click 'Generate new token (classic)'" -ForegroundColor White
        Write-Host "3. Select 'repo' permissions" -ForegroundColor White
        Write-Host "4. Generate and copy the token" -ForegroundColor White
        Write-Host "5. Run: git push -u origin main" -ForegroundColor Cyan
        Write-Host "6. Use the token as your password" -ForegroundColor White
        Write-Host ""
    }
}

Write-Host ""
Write-Host "Current Git status:" -ForegroundColor Yellow
git status
Write-Host ""
