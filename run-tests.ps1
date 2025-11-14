# ===================================================================
# 🧪 Test Runner Script - Phase 1 Task 10
# ===================================================================

Write-Host "🚀 Starting Test Suite..." -ForegroundColor Cyan
Write-Host ""

# Navigate to project root
Set-Location "C:\Disk1\TargCC-Core-V2"

# Step 1: Build the solution
Write-Host "📦 Step 1: Building solution..." -ForegroundColor Yellow
dotnet build --configuration Debug

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Build successful!" -ForegroundColor Green
Write-Host ""

# Step 2: Run all tests
Write-Host "🧪 Step 2: Running all tests..." -ForegroundColor Yellow
dotnet test --no-build --verbosity normal

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Some tests failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ All tests passed!" -ForegroundColor Green
Write-Host ""

# Step 3: Run tests with coverage
Write-Host "📊 Step 3: Running tests with code coverage..." -ForegroundColor Yellow
dotnet test --no-build --collect:"XPlat Code Coverage" --results-directory:"./TestResults"

if ($LASTEXITCODE -ne 0) {
    Write-Host "⚠️  Coverage collection had issues" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "✅ Test execution complete!" -ForegroundColor Green
Write-Host ""
Write-Host "📊 Coverage Results:" -ForegroundColor Cyan
Write-Host "   Check: ./TestResults/ folder for coverage.cobertura.xml" -ForegroundColor White
Write-Host ""
Write-Host "📈 To view coverage in Visual Studio:" -ForegroundColor Cyan
Write-Host "   1. Open Test Explorer" -ForegroundColor White
Write-Host "   2. Click 'Analyze Code Coverage'" -ForegroundColor White
Write-Host ""
Write-Host "🎉 Done!" -ForegroundColor Green
