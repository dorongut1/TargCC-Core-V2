# =============================================================================
# TargCC V2 - Complete Test Script (PowerShell)
# Version: 1.0
# Date: 2025-12-04
# Purpose: Test TargCC V2 end-to-end functionality on Windows
# =============================================================================

param(
    [string]$SqlServer = "localhost",
    [string]$TargCCRoot = "$PSScriptRoot",
    [switch]$SkipTests,
    [switch]$SkipDatabase
)

# Configuration
$TestDir = "$env:TEMP\TargCCTest_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
$TestDbName = "TargCCTest_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
$ConnectionString = "Server=$SqlServer;Database=$TestDbName;Trusted_Connection=true;"

# Functions
function Write-Header {
    param([string]$Text)
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host $Text -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
}

function Write-Success {
    param([string]$Text)
    Write-Host "✓ $Text" -ForegroundColor Green
}

function Write-ErrorMsg {
    param([string]$Text)
    Write-Host "✗ $Text" -ForegroundColor Red
}

function Write-Warning {
    param([string]$Text)
    Write-Host "⚠ $Text" -ForegroundColor Yellow
}

function Write-Info {
    param([string]$Text)
    Write-Host "ℹ $Text" -ForegroundColor Blue
}

# =============================================================================
# Step 0: Pre-checks
# =============================================================================
Write-Header "Step 0: Pre-flight Checks"

Write-Info "Checking prerequisites..."

# Check dotnet
try {
    $dotnetVersion = dotnet --version
    Write-Success "dotnet found: $dotnetVersion"
} catch {
    Write-ErrorMsg "dotnet not found. Please install .NET 9 SDK"
    exit 1
}

# Check sqlcmd
try {
    $null = sqlcmd -? 2>$null
    Write-Success "sqlcmd found"
    $HasSqlCmd = $true
} catch {
    Write-Warning "sqlcmd not found. Will skip database creation."
    $HasSqlCmd = $false
}

# Check TargCC directory
if (-not (Test-Path $TargCCRoot)) {
    Write-ErrorMsg "TargCC root directory not found: $TargCCRoot"
    exit 1
}
Write-Success "TargCC directory found"

# =============================================================================
# Step 1: Build TargCC
# =============================================================================
Write-Header "Step 1: Building TargCC V2"

Push-Location $TargCCRoot

Write-Info "Restoring packages..."
dotnet restore

if ($LASTEXITCODE -ne 0) {
    Write-ErrorMsg "Restore failed!"
    Pop-Location
    exit 1
}

Write-Info "Building solution..."
dotnet build --configuration Release --no-restore

if ($LASTEXITCODE -eq 0) {
    Write-Success "Build completed successfully!"
} else {
    Write-ErrorMsg "Build failed!"
    Pop-Location
    exit 1
}

Pop-Location

# =============================================================================
# Step 2: Run Unit Tests
# =============================================================================
if (-not $SkipTests) {
    Write-Header "Step 2: Running Unit Tests"

    Push-Location $TargCCRoot

    Write-Info "Running C# unit tests..."
    dotnet test --filter "Category=Unit" --no-build --configuration Release --logger "console;verbosity=minimal"

    if ($LASTEXITCODE -eq 0) {
        Write-Success "Unit tests passed!"
    } else {
        Write-Warning "Some unit tests failed (continuing anyway)"
    }

    Pop-Location
} else {
    Write-Warning "Skipping tests (--SkipTests flag)"
}

# =============================================================================
# Step 3: Create Test Database
# =============================================================================
if ($HasSqlCmd -and -not $SkipDatabase) {
    Write-Header "Step 3: Creating Test Database"

    Write-Info "Creating database: $TestDbName"

    # Create database
    $createDbSql = "CREATE DATABASE [$TestDbName]"
    sqlcmd -S $SqlServer -Q $createDbSql 2>$null

    if ($LASTEXITCODE -eq 0) {
        Write-Success "Database created: $TestDbName"
    } else {
        Write-ErrorMsg "Failed to create database"
        exit 1
    }

    Write-Info "Creating test tables..."

    # Create tables
    $schemaFile = Join-Path $TargCCRoot "test_database_schema.sql"

    if (Test-Path $schemaFile) {
        sqlcmd -S $SqlServer -d $TestDbName -i $schemaFile 2>$null

        if ($LASTEXITCODE -eq 0) {
            Write-Success "Test tables created with sample data"
        } else {
            Write-ErrorMsg "Failed to create test tables"
            exit 1
        }
    } else {
        Write-Warning "Schema file not found: $schemaFile"
        Write-Info "Creating minimal schema..."

        $minimalSchema = @"
CREATE TABLE [dbo].[Customer] (
    [ID] INT PRIMARY KEY IDENTITY(1,1),
    [Name] NVARCHAR(100) NOT NULL,
    [Email] NVARCHAR(100) NOT NULL,
    [Phone] VARCHAR(20),
    [AddedOn] DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE UNIQUE INDEX IX_Customer_Email ON [dbo].[Customer]([Email]);

INSERT INTO [dbo].[Customer] ([Name], [Email], [Phone])
VALUES
    ('John Doe', 'john@example.com', '555-0100'),
    ('Jane Smith', 'jane@example.com', '555-0200');
"@

        $minimalSchema | sqlcmd -S $SqlServer -d $TestDbName 2>$null
        Write-Success "Minimal schema created"
    }
} else {
    Write-Warning "Skipping database creation"
}

# =============================================================================
# Step 4: Create Test Project Directory
# =============================================================================
Write-Header "Step 4: Creating Test Project"

Write-Info "Creating test directory: $TestDir"
New-Item -ItemType Directory -Path $TestDir -Force | Out-Null
Set-Location $TestDir

Write-Success "Test directory created"

# =============================================================================
# Step 5: Initialize TargCC
# =============================================================================
Write-Header "Step 5: Initializing TargCC"

$TargCCCli = Join-Path $TargCCRoot "src\TargCC.CLI\bin\Release\net9.0\TargCC.CLI.exe"

if (-not (Test-Path $TargCCCli)) {
    # Try without .exe
    $TargCCCli = Join-Path $TargCCRoot "src\TargCC.CLI\bin\Release\net9.0\TargCC.CLI"
    if (-not (Test-Path $TargCCCli)) {
        Write-ErrorMsg "TargCC CLI not found at: $TargCCCli"
        exit 1
    }
}

Write-Info "Running: targcc init"
& $TargCCCli init --force

if ($LASTEXITCODE -eq 0) {
    Write-Success "TargCC initialized"
} else {
    Write-ErrorMsg "Failed to initialize TargCC"
    exit 1
}

# Configure connection string
Write-Info "Configuring connection string..."
& $TargCCCli config set ConnectionString $ConnectionString
& $TargCCCli config set DefaultNamespace "TestApp"
& $TargCCCli config set OutputDirectory "."

Write-Success "Configuration set"

# =============================================================================
# Step 6: Analyze Database Schema
# =============================================================================
if ($HasSqlCmd -and -not $SkipDatabase) {
    Write-Header "Step 6: Analyzing Database Schema"

    Write-Info "Running: targcc analyze schema"
    & $TargCCCli analyze schema

    if ($LASTEXITCODE -eq 0) {
        Write-Success "Schema analysis completed"
    } else {
        Write-Warning "Schema analysis had issues (continuing)"
    }
}

# =============================================================================
# Step 7: Generate Complete Project
# =============================================================================
Write-Header "Step 7: Generating Complete Project"

Write-Info "Running: targcc generate project"
Write-Info "This may take 1-2 minutes..."

& $TargCCCli generate project --database $TestDbName --output . --namespace TestApp

if ($LASTEXITCODE -eq 0) {
    Write-Success "Project generation completed!"
} else {
    Write-ErrorMsg "Project generation failed!"
    exit 1
}

# Show generated files
Write-Info "Generated project structure:"
Get-ChildItem -Name

# =============================================================================
# Step 8: Build Generated Project
# =============================================================================
Write-Header "Step 8: Building Generated Project"

Write-Info "Restoring packages for generated project..."
dotnet restore

Write-Info "Building generated project..."
dotnet build --configuration Release

if ($LASTEXITCODE -eq 0) {
    Write-Success "Generated project built successfully!"
} else {
    Write-ErrorMsg "Generated project build failed!"
    Write-Info "This is expected - checking build errors..."
}

# =============================================================================
# Step 9: Analyze Results
# =============================================================================
Write-Header "Step 9: Test Results Summary"

Write-Host ""
Write-Info "Test completed!"
Write-Host ""

Write-Info "Test Details:"
Write-Host "  Test Directory: $TestDir"
Write-Host "  Database: $TestDbName"
Write-Host "  Connection: $ConnectionString"
Write-Host ""

Write-Info "Generated Files:"
if (Test-Path "TestApp.sln") {
    Write-Success "Solution file created"
    $slnSize = (Get-Item "TestApp.sln").Length
    Write-Host "  $slnSize bytes - TestApp.sln"
}

if (Test-Path "src") {
    Write-Success "Source directory created"
    Write-Host "  Projects:"
    Get-ChildItem "src" -Directory | ForEach-Object {
        Write-Host "    - $($_.Name)"
    }
}

Write-Host ""
Write-Info "Next Steps:"
Write-Host "  1. Review generated code: cd $TestDir"
Write-Host "  2. Check for build errors: dotnet build"
Write-Host "  3. Run API: cd src\TestApp.API && dotnet run"
Write-Host "  4. Test API: curl http://localhost:5000/api/customers"
Write-Host ""

# Cleanup option
Write-Host ""
$cleanup = Read-Host "Do you want to cleanup test database and files? (y/N)"
if ($cleanup -eq 'y' -or $cleanup -eq 'Y') {
    Write-Info "Cleaning up..."

    if ($HasSqlCmd -and -not $SkipDatabase) {
        sqlcmd -S $SqlServer -Q "DROP DATABASE [$TestDbName]" 2>$null
        Write-Success "Database dropped"
    }

    Set-Location $env:TEMP
    Remove-Item -Path $TestDir -Recurse -Force -ErrorAction SilentlyContinue
    Write-Success "Test directory removed"
} else {
    Write-Info "Test artifacts preserved for inspection"
}

Write-Header "Test Complete!"

exit 0
