# TargCC Bridge - Clean, Restore & Build
# Run this from PowerShell in Visual Studio or Windows Terminal

Write-Host "=======================================" -ForegroundColor Cyan
Write-Host "TargCC Bridge - Full Rebuild" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""

$SolutionPath = "C:\Disk1\TargCC-Core-V2"
$ErrorCount = 0

# Step 1: Clean old binaries
Write-Host "[Step 1/3] Cleaning old binaries..." -ForegroundColor Yellow

$FoldersToClean = @(
    "$SolutionPath\src\TargCC.Core.Interfaces\obj",
    "$SolutionPath\src\TargCC.Core.Interfaces\bin",
    "$SolutionPath\src\TargCC.Core.Analyzers\obj",
    "$SolutionPath\src\TargCC.Core.Analyzers\bin",
    "$SolutionPath\src\TargCC.Bridge\obj",
    "$SolutionPath\src\TargCC.Bridge\bin",
    "$SolutionPath\src\TargCC.Bridge.Tests\obj",
    "$SolutionPath\src\TargCC.Bridge.Tests\bin",
    "$SolutionPath\examples\VBNetBridgeExample\obj",
    "$SolutionPath\examples\VBNetBridgeExample\bin"
)

foreach ($folder in $FoldersToClean) {
    if (Test-Path $folder) {
        Write-Host "  Removing: $folder" -ForegroundColor Gray
        Remove-Item -Path $folder -Recurse -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "  ✓ Clean completed" -ForegroundColor Green
Write-Host ""

# Step 2: Restore NuGet packages
Write-Host "[Step 2/3] Restoring NuGet packages..." -ForegroundColor Yellow

$RestoreCommand = "dotnet restore `"$SolutionPath\TargCC.Core.sln`""
Write-Host "  Running: $RestoreCommand" -ForegroundColor Gray

try {
    $RestoreOutput = Invoke-Expression $RestoreCommand 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ Restore completed successfully" -ForegroundColor Green
    } else {
        Write-Host "  ✗ Restore failed with exit code: $LASTEXITCODE" -ForegroundColor Red
        Write-Host $RestoreOutput -ForegroundColor Red
        $ErrorCount++
    }
} catch {
    Write-Host "  ✗ Restore failed: $_" -ForegroundColor Red
    $ErrorCount++
}

Write-Host ""

# Step 3: Build in correct order
Write-Host "[Step 3/3] Building projects..." -ForegroundColor Yellow

$Projects = @(
    @{Name="Core.Interfaces"; Path="$SolutionPath\src\TargCC.Core.Interfaces\TargCC.Core.Interfaces.csproj"},
    @{Name="Core.Analyzers"; Path="$SolutionPath\src\TargCC.Core.Analyzers\TargCC.Core.Analyzers.csproj"},
    @{Name="Bridge"; Path="$SolutionPath\src\TargCC.Bridge\TargCC.Bridge.vbproj"},
    @{Name="Bridge.Tests"; Path="$SolutionPath\src\TargCC.Bridge.Tests\TargCC.Bridge.Tests.vbproj"},
    @{Name="VBNetBridgeExample"; Path="$SolutionPath\examples\VBNetBridgeExample\VBNetBridgeExample.vbproj"}
)

foreach ($project in $Projects) {
    Write-Host "  Building: $($project.Name)..." -ForegroundColor Cyan
    
    $BuildCommand = "dotnet build `"$($project.Path)`" -c Debug"
    
    try {
        $BuildOutput = Invoke-Expression $BuildCommand 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "    ✓ Build successful" -ForegroundColor Green
        } else {
            Write-Host "    ✗ Build failed with exit code: $LASTEXITCODE" -ForegroundColor Red
            Write-Host $BuildOutput -ForegroundColor Red
            $ErrorCount++
        }
    } catch {
        Write-Host "    ✗ Build failed: $_" -ForegroundColor Red
        $ErrorCount++
    }
}

Write-Host ""
Write-Host "=======================================" -ForegroundColor Cyan

if ($ErrorCount -eq 0) {
    Write-Host "✓ All operations completed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "  1. Open Visual Studio" -ForegroundColor White
    Write-Host "  2. Reload the solution if it's open" -ForegroundColor White
    Write-Host "  3. Verify no errors in Error List" -ForegroundColor White
    Write-Host "  4. Run tests: dotnet test" -ForegroundColor White
} else {
    Write-Host "✗ Completed with $ErrorCount error(s)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "  1. Check the error messages above" -ForegroundColor White
    Write-Host "  2. Ensure .NET SDK is installed (dotnet --version)" -ForegroundColor White
    Write-Host "  3. Try running from Developer PowerShell for VS" -ForegroundColor White
    Write-Host "  4. Check project files for syntax errors" -ForegroundColor White
}

Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""

# Pause at the end so you can see the results
Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
