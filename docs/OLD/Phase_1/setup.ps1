# TargCC Core V2 - Setup Script
# Quick setup for DatabaseAnalyzer

Write-Host "===================================="
Write-Host "   TargCC Core V2 - Setup"
Write-Host "   DatabaseAnalyzer Installation"
Write-Host "===================================="
Write-Host ""

# Check .NET 8
Write-Host "Checking for .NET 8 SDK..."
$dotnetVersion = dotnet --version
if ($dotnetVersion -like "8.*") {
    Write-Host ".NET 8 SDK found: $dotnetVersion"
} else {
    Write-Host ".NET 8 SDK NOT found. Please install from: https://dotnet.microsoft.com/download"
    exit 1
}

# Create folder structure
Write-Host ""
Write-Host "Creating folder structure..."

$folders = @(
    "src\TargCC.Core.Interfaces\Models",
    "src\TargCC.Core.Analyzers\Database",
    "tests\TargCC.Core.Tests\Unit\Analyzers"
)

foreach ($folder in $folders) {
    if (!(Test-Path $folder)) {
        New-Item -ItemType Directory -Path $folder -Force | Out-Null
        Write-Host "  Created: $folder"
    }
}

# Copy source files
Write-Host ""
Write-Host "Copying source files..."

# Analyzers
Copy-Item "DatabaseAnalyzer.cs" "src\TargCC.Core.Analyzers\Database\" -Force
Copy-Item "TableAnalyzer.cs" "src\TargCC.Core.Analyzers\Database\" -Force
Copy-Item "ColumnAnalyzer.cs" "src\TargCC.Core.Analyzers\Database\" -Force
Copy-Item "RelationshipAnalyzer.cs" "src\TargCC.Core.Analyzers\Database\" -Force
Write-Host "  Analyzers copied."

# Models
Copy-Item "Enums.cs" "src\TargCC.Core.Interfaces\Models\" -Force
Write-Host "  Models copied."

# Tests
Copy-Item "DatabaseAnalyzerTests.cs" "tests\TargCC.Core.Tests\Unit\Analyzers\" -Force
Write-Host "  Tests copied."

# Project files
Copy-Item "TargCC.Core.Analyzers.csproj" "src\TargCC.Core.Analyzers\" -Force
Copy-Item "TargCC.Core.Tests.csproj" "tests\TargCC.Core.Tests\" -Force
Write-Host "  Project files copied."

# Restore NuGet packages
Write-Host ""
Write-Host "Restoring NuGet packages..."
dotnet restore "src\TargCC.Core.Analyzers\TargCC.Core.Analyzers.csproj"
dotnet restore "tests\TargCC.Core.Tests\TargCC.Core.Tests.csproj"

# Build project
Write-Host ""
Write-Host "Building project..."
$buildResult = dotnet build "src\TargCC.Core.Analyzers\TargCC.Core.Analyzers.csproj" --configuration Debug

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build succeeded."
} else {
    Write-Host "Build FAILED. Check errors above."
    exit 1
}

# Run tests (optional)
Write-Host ""
$runTests = Read-Host "Run tests now? (y/n)"
if ($runTests -eq "y") {
    Write-Host "Running tests..."
    dotnet test "tests\TargCC.Core.Tests\TargCC.Core.Tests.csproj" --verbosity normal
}

# Done
Write-Host ""
Write-Host "===================================="
Write-Host "   Setup completed successfully!"
Write-Host "===================================="
Write-Host ""
Write-Host "Next steps:"
Write-Host "  1. Open the solution in Visual Studio 2022"
Write-Host "  2. Update Connection String in DatabaseAnalyzerTests.cs"
Write-Host "  3. Run the tests"
Write-Host "  4. Read README_DatabaseAnalyzer.md"
Write-Host ""
Write-Host "Happy Coding!"
