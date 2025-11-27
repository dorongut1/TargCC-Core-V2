# Known Issues & Technical Debt 🐛

**Last Updated:** 27/11/2025  
**Project:** TargCC Core V2  
**Phase:** 3B - AI Integration  

---

## 🔴 High Priority

### None Currently

---

## 🟡 Medium Priority

### Issue #1: AnsiConsole Direct Usage in SuggestCommand

**Status:** 🟡 Documented - Needs Refactoring  
**Discovered:** Day 14 (27/11/2025)  
**Impact:** 3 unit tests must be skipped  
**Effort:** ~30 minutes  

**Problem:**
`SuggestCommand.DisplaySuggestions()` uses `Spectre.Console.AnsiConsole` static methods directly, making it impossible to mock in unit tests. This causes test failures because there's no real console in the test environment.

**Affected Code:**
```csharp
// File: SuggestCommand.cs, Line ~210
private void DisplaySuggestions(SchemaAnalysisResult result, string groupBy, bool useColors)
{
    var formatter = new SuggestionFormatter();
    var formattedOutput = formatter.FormatSuggestions(result, useColors);
    
    var panel = new Panel(formattedOutput)
    {
        Header = new PanelHeader("[bold cyan]💡 AI Suggestions[/]"),
        Border = BoxBorder.Rounded,
    };

    AnsiConsole.Write(panel);  // ← Problem: Static call, can't mock
    AnsiConsole.MarkupLine($"[dim]{summary}[/]");  // ← Problem: Static call, can't mock
}
```

**Affected Tests:**
1. `Execute_WithValidTable_ShouldReturnSuccess`
2. `Execute_WithCategoryFilter_ShouldFilterSuggestions`
3. `Execute_WithSeverityFilter_ShouldFilterSuggestions`

All three tests are currently marked as:
```csharp
[Fact(Skip = "AnsiConsole.Write causes test failures - requires IAnsiConsoleWrapper injection refactor")]
```

**Proposed Solution:**

1. **Create Interface:**
```csharp
// File: TargCC.CLI/Services/IAnsiConsoleWrapper.cs
namespace TargCC.CLI.Services;

public interface IAnsiConsoleWrapper
{
    void Write(IRenderable renderable);
    void MarkupLine(string markup);
}
```

2. **Create Implementation:**
```csharp
// File: TargCC.CLI/Services/AnsiConsoleWrapper.cs
using Spectre.Console;

namespace TargCC.CLI.Services;

public class AnsiConsoleWrapper : IAnsiConsoleWrapper
{
    public void Write(IRenderable renderable) 
        => AnsiConsole.Write(renderable);
    
    public void MarkupLine(string markup) 
        => AnsiConsole.MarkupLine(markup);
}
```

3. **Update SuggestCommand:**
```csharp
public class SuggestCommand : Command
{
    private readonly IAnsiConsoleWrapper ansiConsole;  // Add this
    
    public SuggestCommand(
        IAIService aiService,
        IDatabaseAnalyzer databaseAnalyzer,
        IConfigurationService configurationService,
        IOutputService outputService,
        IAnsiConsoleWrapper ansiConsole,  // Add this
        ILoggerFactory loggerFactory)
        : base("suggest", "Get AI-powered suggestions for schema improvements")
    {
        // ... existing code ...
        this.ansiConsole = ansiConsole ?? throw new ArgumentNullException(nameof(ansiConsole));
    }
    
    private void DisplaySuggestions(...)
    {
        // Change from:
        // AnsiConsole.Write(panel);
        // To:
        this.ansiConsole.Write(panel);
        
        // Change from:
        // AnsiConsole.MarkupLine($"[dim]{summary}[/]");
        // To:
        this.ansiConsole.MarkupLine($"[dim]{summary}[/]");
    }
}
```

4. **Update Dependency Injection:**
```csharp
// File: RootCommand.cs or Program.cs
services.AddSingleton<IAnsiConsoleWrapper, AnsiConsoleWrapper>();
```

5. **Update Tests:**
```csharp
// File: SuggestCommandTests.cs
private readonly Mock<IAnsiConsoleWrapper> ansiConsoleMock;

public SuggestCommandTests()
{
    this.ansiConsoleMock = new Mock<IAnsiConsoleWrapper>();
    
    // Setup mocks
    this.ansiConsoleMock.Setup(x => x.Write(It.IsAny<IRenderable>()));
    this.ansiConsoleMock.Setup(x => x.MarkupLine(It.IsAny<string>()));
    
    this.command = new SuggestCommand(
        this.aiServiceMock.Object,
        this.databaseAnalyzerMock.Object,
        this.configurationServiceMock.Object,
        this.outputServiceMock.Object,
        this.ansiConsoleMock.Object,  // Add this
        this.loggerFactoryMock.Object);
}

// Remove [Skip] from the 3 affected tests
```

**When to Fix:**
- **Option 1:** Day 10 - CLI Polish & Documentation (recommended)
- **Option 2:** Phase 3D, Days 41-45 - Final Testing & Polish

**Related Files:**
- `src/TargCC.CLI/Commands/SuggestCommand.cs`
- `src/tests/TargCC.CLI.Tests/Commands/SuggestCommandTests.cs`

---

## 🟢 Low Priority / Nice to Have

### None Currently

---

## ✅ Resolved Issues

### Issue #2: Enum Value Mismatch in SuggestionFormatter
**Status:** ✅ Resolved (Day 14)  
**Fixed By:** Changing Low/Medium/High → Info/BestPractice/Warning/Critical

### Issue #3: Missing IConfigurationService in SuggestCommand
**Status:** ✅ Resolved (Day 14)  
**Fixed By:** Added IConfigurationService dependency injection

### Issue #4: Method Signature Mismatches in Tests
**Status:** ✅ Resolved (Day 14)  
**Fixed By:** Updated all mock setups to use It.IsAny<> for proper matching

---

## 📊 Technical Debt Summary

| Category | Count | Priority |
|----------|-------|----------|
| 🔴 High Priority | 0 | Immediate |
| 🟡 Medium Priority | 1 | This Phase |
| 🟢 Low Priority | 0 | Future |
| **Total Open** | **1** | |
| ✅ Resolved | 3 | |

---

## 📝 Notes

- This document tracks issues discovered during development that require future attention
- Issues are marked as High/Medium/Low based on impact and urgency
- Each issue includes a clear solution path to make fixing easier
- Resolved issues are kept for historical reference

---

**Next Review:** Day 20 (End of Phase 3B)
