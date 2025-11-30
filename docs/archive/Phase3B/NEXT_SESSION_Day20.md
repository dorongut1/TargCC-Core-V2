# 🎯 START HERE - Day 20: AI Integration Testing

**Date:** [Next Session Date]  
**Phase:** 3B - AI Integration  
**Day:** 20 of 45  
**Status:** 🚀 READY TO START  
**Previous:** Day 18-19 COMPLETE ✅

---

## 📋 Quick Start Checklist

### Before You Begin:
- [ ] Read `HANDOFF_Day18-19_COMPLETE.md` for context
- [ ] Verify current test count: **675 tests passing** ✅
- [ ] Confirm build succeeds: `dotnet build` (0 errors)
- [ ] Open project in Visual Studio

---

## 🎯 Today's Mission: Complete Day 18-19 Testing

**Goal:** Write all missing tests to reach 705+ total tests  
**Duration:** 3-4 hours  
**Test Target:** +30 tests (15 Service + 15 CLI)

---

## 📝 Task 1: Service Unit Tests (2 hours)

### Create Test File
**Path:** `src/tests/TargCC.AI.Tests/Services/CodeQualityAnalyzerServiceTests.cs`

### Setup Structure:
```csharp
// <copyright file="CodeQualityAnalyzerServiceTests.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.AI.Models;
using TargCC.AI.Models.Quality;
using TargCC.AI.Services;
using TargCC.Core.Interfaces;
using Xunit;

namespace TargCC.AI.Tests.Services;

/// <summary>
/// Tests for <see cref="CodeQualityAnalyzerService"/>.
/// </summary>
public class CodeQualityAnalyzerServiceTests
{
    private readonly Mock<IAIService> mockAIService;
    private readonly Mock<ILogger<CodeQualityAnalyzerService>> mockLogger;
    private readonly CodeQualityAnalyzerService service;

    public CodeQualityAnalyzerServiceTests()
    {
        this.mockAIService = new Mock<IAIService>();
        this.mockLogger = new Mock<ILogger<CodeQualityAnalyzerService>>();
        this.service = new CodeQualityAnalyzerService(
            this.mockAIService.Object,
            this.mockLogger.Object);
    }

    // Tests go here...
}
```

### Test Categories (15 tests total):

#### Category 1: Constructor Tests (2 tests)
```csharp
[Fact]
public void Constructor_WithValidParameters_CreatesService()
{
    // Arrange & Act
    var service = new CodeQualityAnalyzerService(
        this.mockAIService.Object,
        this.mockLogger.Object);

    // Assert
    service.Should().NotBeNull();
}

[Fact]
public void Constructor_WithNullAIService_ThrowsArgumentNullException()
{
    // Act
    var act = () => new CodeQualityAnalyzerService(null!, this.mockLogger.Object);

    // Assert
    act.Should().Throw<ArgumentNullException>()
        .WithParameterName("aiService");
}
```

#### Category 2: AnalyzeNamingConventionsAsync (4 tests)
```csharp
[Fact]
public async Task AnalyzeNamingConventionsAsync_WithValidTable_ReturnsIssues()
{
    // Arrange
    var table = TableBuilder.Create()
        .WithName("customers")  // lowercase should be flagged
        .Build();
    
    var jsonResponse = "[{\"elementName\":\"customers\",\"elementType\":\"Table\",\"issue\":\"Not PascalCase\",\"recommendation\":\"Use CustomersTable\",\"severity\":\"Medium\"}]";
    
    this.mockAIService
        .Setup(x => x.CompleteAsync(It.IsAny<AIRequest>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new AIResponse { Success = true, Content = jsonResponse });

    // Act
    var result = await this.service.AnalyzeNamingConventionsAsync(table);

    // Assert
    result.Should().NotBeEmpty();
    result.Should().ContainSingle(i => i.ElementName == "customers");
}

[Fact]
public async Task AnalyzeNamingConventionsAsync_WithAIFailure_ReturnsEmpty()
{
    // Arrange
    var table = TableBuilder.Create().WithName("Customer").Build();
    
    this.mockAIService
        .Setup(x => x.CompleteAsync(It.IsAny<AIRequest>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new AIResponse { Success = false, ErrorMessage = "AI Error" });

    // Act
    var result = await this.service.AnalyzeNamingConventionsAsync(table);

    // Assert
    result.Should().BeEmpty();
}

[Fact]
public async Task AnalyzeNamingConventionsAsync_WithNullTable_ThrowsArgumentNullException()
{
    // Act
    var act = async () => await this.service.AnalyzeNamingConventionsAsync(null!);

    // Assert
    await act.Should().ThrowAsync<ArgumentNullException>();
}

[Fact]
public async Task AnalyzeNamingConventionsAsync_WithCancellation_ThrowsOperationCanceledException()
{
    // Arrange
    var table = TableBuilder.Create().WithName("Customer").Build();
    var cts = new CancellationTokenSource();
    cts.Cancel();

    // Act
    var act = async () => await this.service.AnalyzeNamingConventionsAsync(table, cts.Token);

    // Assert
    await act.Should().ThrowAsync<OperationCanceledException>();
}
```

#### Category 3: CheckBestPracticesAsync (4 tests)
```csharp
[Fact]
public async Task CheckBestPracticesAsync_WithValidTable_ReturnsViolations()
{
    // Arrange
    var table = TableBuilder.Create()
        .WithName("Customer")
        .Build();
    
    var jsonResponse = "[{\"category\":\"Architecture\",\"elementName\":\"Customer\",\"violation\":\"Missing primary key\",\"recommendation\":\"Add Id column\",\"severity\":\"Critical\"}]";
    
    this.mockAIService
        .Setup(x => x.CompleteAsync(It.IsAny<AIRequest>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new AIResponse { Success = true, Content = jsonResponse });

    // Act
    var result = await this.service.CheckBestPracticesAsync(table);

    // Assert
    result.Should().NotBeEmpty();
    result.Should().ContainSingle(v => v.Category == "Architecture");
}

[Fact]
public async Task CheckBestPracticesAsync_WithAIFailure_ReturnsEmpty()
{
    // Arrange
    var table = TableBuilder.Create().WithName("Customer").Build();
    
    this.mockAIService
        .Setup(x => x.CompleteAsync(It.IsAny<AIRequest>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new AIResponse { Success = false });

    // Act
    var result = await this.service.CheckBestPracticesAsync(table);

    // Assert
    result.Should().BeEmpty();
}

[Fact]
public async Task CheckBestPracticesAsync_WithNullTable_ThrowsArgumentNullException()
{
    // Act
    var act = async () => await this.service.CheckBestPracticesAsync(null!);

    // Assert
    await act.Should().ThrowAsync<ArgumentNullException>();
}

[Fact]
public async Task CheckBestPracticesAsync_WithCancellation_ThrowsOperationCanceledException()
{
    // Arrange
    var table = TableBuilder.Create().WithName("Customer").Build();
    var cts = new CancellationTokenSource();
    cts.Cancel();

    // Act
    var act = async () => await this.service.CheckBestPracticesAsync(table, cts.Token);

    // Assert
    await act.Should().ThrowAsync<OperationCanceledException>();
}
```

#### Category 4: ValidateRelationshipsAsync (3 tests)
```csharp
[Fact]
public async Task ValidateRelationshipsAsync_WithValidTable_ReturnsIssues()
{
    // Similar pattern to above...
}

[Fact]
public async Task ValidateRelationshipsAsync_WithAIFailure_ReturnsEmpty()
{
    // Similar pattern...
}

[Fact]
public async Task ValidateRelationshipsAsync_WithNullTable_ThrowsArgumentNullException()
{
    // Similar pattern...
}
```

#### Category 5: GenerateQualityReportAsync (2 tests)
```csharp
[Fact]
public async Task GenerateQualityReportAsync_WithValidTable_ReturnsReport()
{
    // Test that all 3 methods are called and report is generated
}

[Fact]
public async Task GenerateQualityReportAsync_CalculatesCorrectScore()
{
    // Test score calculation based on issue severity
}
```

---

## 📝 Task 2: Enhanced CLI Tests (1 hour)

### Enhance Existing File
**Path:** `src/tests/TargCC.CLI.Tests/Commands/Analyze/AnalyzeQualityCommandTests.cs`

### Add These Test Categories (15 new tests):

#### Category 1: Command Execution (5 tests)
```csharp
[Fact]
public async Task Execute_WithValidTable_ReturnsSuccess()
{
    // Test successful execution
}

[Fact]
public async Task Execute_WithInvalidTable_ReturnsError()
{
    // Test error handling
}

[Fact]
public async Task Execute_WithOutputFlag_GeneratesJsonFile()
{
    // Test JSON export
}

[Fact]
public async Task Execute_WithDatabaseError_ReturnsError()
{
    // Test DB connection failure
}

[Fact]
public async Task Execute_WithCancellation_Cancels()
{
    // Test cancellation
}
```

#### Category 2: Output Formatting (5 tests)
```csharp
[Fact]
public void FormatQualityReport_WithHighScore_ShowsGreen()
{
    // Test colored output for good score
}

[Fact]
public void FormatQualityReport_WithLowScore_ShowsRed()
{
    // Test colored output for bad score
}

[Fact]
public void FormatNamingIssues_FormatsAsTable()
{
    // Test table formatting
}

[Fact]
public void FormatBestPractices_ShowsSeverity()
{
    // Test severity display
}

[Fact]
public void FormatSummary_ShowsGrade()
{
    // Test grade display
}
```

#### Category 3: Error Scenarios (5 tests)
```csharp
[Fact]
public async Task Execute_WithMissingTable_ShowsError()
{
    // Test missing table
}

[Fact]
public async Task Execute_WithAIServiceFailure_ShowsError()
{
    // Test AI failure
}

[Fact]
public async Task Execute_WithInvalidConfig_ShowsError()
{
    // Test config error
}

[Fact]
public async Task Execute_WithFileWriteError_ShowsError()
{
    // Test file write failure
}

[Fact]
public async Task Execute_WithException_LogsAndReturnsError()
{
    // Test exception handling
}
```

---

## 📝 Task 3: Build & Verify (30 minutes)

### Step 1: Build All
```powershell
cd C:\Disk1\TargCC-Core-V2
dotnet build --no-restore
```

**Expected:** 0 Errors, warnings OK

### Step 2: Run All Tests
```powershell
dotnet test --no-build --verbosity normal
```

**Expected:** 705+ tests passing

### Step 3: Run Specific Project Tests
```powershell
# AI Tests
dotnet test src/tests/TargCC.AI.Tests --no-build

# CLI Tests
dotnet test src/tests/TargCC.CLI.Tests --no-build
```

### Step 4: Check Coverage (if time)
```powershell
dotnet test --collect:"XPlat Code Coverage"
```

---

## 📝 Task 4: Documentation Update (15 minutes)

### Update Files:

#### 1. Phase3_Checklist.md
Mark Day 18-19 as complete:
```markdown
#### 📆 Day 18-19: Code Quality Analyzer

**Date:** 28/11/2025  
**Status:** ✅ Complete

**Completed:**
- [x] CodeQualityAnalyzerService implementation (551 lines)
- [x] ICodeQualityAnalyzer interface
- [x] 15 CLI tests
- [x] 15 Service unit tests
- [x] 15 Enhanced CLI tests
- [x] Build verification

**Tests:** 30/30 (100%) ✅
```

#### 2. PROGRESS.md (if exists)
Update test count and progress

#### 3. Git Commit
```bash
git add .
git commit -m "feat(ai): Complete Day 18-19 - Code Quality Analyzer with full test coverage

- Implemented CodeQualityAnalyzerService (551 lines)
- Created ICodeQualityAnalyzer interface
- Added 15 service unit tests
- Added 30 CLI tests (15 basic + 15 enhanced)
- Total tests: 705+ (all passing)
- Zero build errors
- Phase 3B Day 18-19 complete ✅"
```

---

## ✅ Success Criteria

By end of session, you must have:

1. ✅ **705+ tests passing** (current: 675, need +30)
2. ✅ **CodeQualityAnalyzerServiceTests.cs created** (15 tests)
3. ✅ **AnalyzeQualityCommandTests.cs enhanced** (15 more tests)
4. ✅ **All builds succeed** (0 errors)
5. ✅ **Documentation updated** (checklist, progress)
6. ✅ **Git commit done** (with proper message)

---

## 🚫 What NOT to Do

1. ❌ Don't implement CLI command HandleAsync yet (that's Day 20 Priority 2)
2. ❌ Don't test manually with `targcc` command
3. ❌ Don't add new features
4. ❌ Don't skip any test categories
5. ❌ Don't commit with failing tests

---

## 📊 Progress Tracking

### Before This Session:
- Total Tests: 675
- Service Tests: 0
- CLI Tests: 15

### After This Session (Target):
- Total Tests: 705+
- Service Tests: 15
- CLI Tests: 30
- Day 18-19: COMPLETE ✅

---

## 💡 Quick Reference

### TableBuilder Pattern:
```csharp
var table = TableBuilder.Create()
    .WithName("Customer")
    .WithSchema("dbo")
    .WithColumn("Id", "int", isPrimaryKey: true)
    .WithColumn("Name", "nvarchar(100)")
    .Build();
```

### AI Mock Pattern:
```csharp
this.mockAIService
    .Setup(x => x.CompleteAsync(
        It.IsAny<AIRequest>(), 
        It.IsAny<CancellationToken>()))
    .ReturnsAsync(new AIResponse 
    { 
        Success = true, 
        Content = jsonString 
    });
```

### FluentAssertions Pattern:
```csharp
result.Should().NotBeEmpty();
result.Should().HaveCount(3);
result.Should().ContainSingle(x => x.ElementName == "Id");
result[0].Severity.Should().Be("Critical");
```

---

## 🆘 If You Get Stuck

### Common Issues:

1. **TableBuilder not found**
   - Add using: `using TargCC.Core.Tests.Builders;`

2. **AIRequest not found**
   - Add using: `using TargCC.AI.Models;`

3. **Tests fail with JSON parsing**
   - Check JSON format matches model properties exactly

4. **Mock not returning expected**
   - Verify Setup() matches the actual call signature

---

## 📁 Files You'll Create/Modify

### Create:
1. `src/tests/TargCC.AI.Tests/Services/CodeQualityAnalyzerServiceTests.cs`

### Modify:
1. `src/tests/TargCC.CLI.Tests/Commands/Analyze/AnalyzeQualityCommandTests.cs`
2. `docs/Phase3_Checklist.md`
3. `docs/PROGRESS.md` (if exists)

---

## 🎯 Next Next Session - Day 20 Part 2

After completing all tests, the next session will:
1. Implement CLI HandleAsync
2. Add output formatting
3. Test manually with `targcc analyze quality Customer`
4. Move to Day 20: AI Integration Testing

---

**REMEMBER:** Focus ONLY on testing today. No implementation, just tests!

**Status:** 🚀 Ready to Start  
**Estimated Time:** 3-4 hours  
**Difficulty:** Medium  
**Blocker:** None

**Good luck! 🚀**

---

**Last Updated:** 28/11/2025  
**Created By:** Claude (Day 18-19 session)
