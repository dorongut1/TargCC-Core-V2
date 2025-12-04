# Generator Fixes Required for V2

## Summary
The generated project has 5 critical bugs that prevent compilation. All stem from hardcoded values in generators.

---

## Bug #1: Hardcoded Namespace "TargCC" (CRITICAL)

### Files Affected:
1. `src/TargCC.Core.Generators/Repositories/RepositoryInterfaceGenerator.cs`
2. `src/TargCC.Core.Generators/Repositories/RepositoryGenerator.cs`
3. `src/TargCC.Core.Generators/Entities/EntityGenerator.cs`
4. `src/TargCC.Core.Generators/API/ApiControllerGenerator.cs`

### Current Code (WRONG):
```csharp
sb.AppendLine("namespace TargCC.Domain.Interfaces;");
sb.AppendLine("using TargCC.Domain.Entities;");
```

### Required Fix:
```csharp
sb.AppendLine($"namespace {rootNamespace}.Domain.Interfaces;");
sb.AppendLine($"using {rootNamespace}.Domain.Entities;");
```

### Changes Needed:

#### 1. `IRepositoryInterfaceGenerator.cs` & `RepositoryInterfaceGenerator.cs`
- ✅ DONE - Added `rootNamespace` parameter
- ✅ DONE - Changed hardcoded "TargCC" to `{rootNamespace}`

#### 2. `IRepositoryGenerator.cs` Line 123
```csharp
// OLD:
Task<string> GenerateAsync(Table table);

// NEW:
Task<string> GenerateAsync(Table table, string rootNamespace = "YourApp");
```

#### 3. `RepositoryGenerator.cs` (multiple lines)
Find & replace in entire file:
- `"namespace TargCC.Infrastructure.Repositories"` → `$"namespace {rootNamespace}.Infrastructure.Repositories"`
- `"using TargCC.Domain.Entities"` → `$"using {rootNamespace}.Domain.Entities"`
- `"using TargCC.Domain.Interfaces"` → `$"using {rootNamespace}.Domain.Interfaces"`

Add parameter:
```csharp
public async Task<string> GenerateAsync(Table table, string rootNamespace = "YourApp")
```

#### 4. `IEntityGenerator.cs`
```csharp
// Line ~33:
Task<string> GenerateAsync(Table table, DatabaseSchema schema, string @namespace = "YourNamespace.Entities");
// ✅ Already has namespace parameter!
```

#### 5. `EntityGenerator.cs`
No changes needed - already accepts `@namespace` parameter! ✅

#### 6. `ApiControllerGenerator.cs` (see Bug #5)

---

## Bug #2: Wrong SDK in API Project (CRITICAL)

### File: `src/TargCC.Core.Generators/Project/ProjectFileGenerator.cs`

### Current Code (Line ~200-250):
```xml
<Project Sdk="Microsoft.NET.Sdk">
```

### Required Fix:
```csharp
// In Generate method, detect project type:
if (projectInfo.Type == ProjectType.Api)
{
    sb.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk.Web\">");
}
else
{
    sb.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
}
```

---

## Bug #3: Missing NuGet Packages (CRITICAL)

### File: `src/TargCC.Core.Generators/Project/ProjectFileGenerator.cs`

### Current Packages for API:
```xml
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.10" />
```

### Missing Packages:
```xml
<PackageReference Include="AutoMapper" Version="12.0.1" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
```

### Required Fix:
Add these packages when `projectInfo.Type == ProjectType.Api`

---

## Bug #4: DTOs Not Generated (HIGH)

### Problem:
Controllers reference DTOs that don't exist:
- `CustomerDto`
- `CreateCustomerRequest`
- `UpdateCustomerRequest`
- `CustomerFilters`

### Solutions (Pick ONE):

#### Option A: Remove DTOs (Quick Fix)
Change `ApiControllerGenerator.cs` to use entities directly:
```csharp
// Instead of:
public async Task<ActionResult<CustomerDto>> GetById(int id)

// Use:
public async Task<ActionResult<Customer>> GetById(int id)
```

#### Option B: Create DTO Generator (Proper Fix)
Create new generator: `DtoGenerator.cs`
- Generate `CustomerDto` with only public properties
- Generate `CreateCustomerRequest` with required fields
- Generate `UpdateCustomerRequest` with updatable fields
- Generate `CustomerFilters` for query parameters

**Recommendation:** Use Option A for now, add Option B later.

---

## Bug #5: IRepository<T> Doesn't Exist (CRITICAL)

### File: `src/TargCC.Core.Generators/API/ApiControllerGenerator.cs`

### Current Code (WRONG):
```csharp
private readonly IRepository<Customer> _repository;

public CustomersController(IRepository<Customer> repository, ...)
{
    _repository = repository;
}
```

### Required Fix:
```csharp
private readonly ICustomerRepository _repository;

public CustomersController(ICustomerRepository repository, ...)
{
    _repository = repository;
}
```

### Changes Needed in `ApiControllerGenerator.cs`:

1. **Line ~150** (field declaration):
```csharp
// OLD:
sb.AppendLine($"        private readonly IRepository<{entityName}> _repository;");

// NEW:
sb.AppendLine($"        private readonly I{entityName}Repository _repository;");
```

2. **Line ~160** (constructor parameter):
```csharp
// OLD:
sb.AppendLine($"            IRepository<{entityName}> repository,");

// NEW:
sb.AppendLine($"            I{entityName}Repository repository,");
```

3. **Line ~50** (using statement):
Add:
```csharp
sb.AppendLine($"using {rootNamespace}.Domain.Interfaces;");
```

4. **Namespace fix**:
```csharp
// OLD:
sb.AppendLine("namespace TestApp.API.Controllers");

// NEW:
sb.AppendLine($"namespace {config.Namespace}.Controllers");
```

---

## Bug #6: AutoMapper Removed (If Option A chosen for Bug #4)

If removing DTOs, also remove AutoMapper:

### In `ApiControllerGenerator.cs`:
Remove:
```csharp
sb.AppendLine("using AutoMapper;");
// ...
sb.AppendLine("        private readonly IMapper _mapper;");
// ...
sb.AppendLine("            IMapper mapper,");
// ...
sb.AppendLine("            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));");
```

And change:
```csharp
// OLD:
return Ok(_mapper.Map<CustomerDto>(entity));

// NEW:
return Ok(entity);
```

---

## Implementation Order (Recommended):

1. ✅ **DONE**: Fix `IRepositoryInterfaceGenerator` + `RepositoryInterfaceGenerator`
2. **TODO**: Fix `IRepositoryGenerator` + `RepositoryGenerator`
3. **TODO**: Fix `ApiControllerGenerator` (Bugs #1, #4, #5, #6)
4. **TODO**: Fix `ProjectFileGenerator` (Bugs #2, #3)
5. **TODO**: Update `ProjectGenerationService` to pass `rootNamespace` to all generators

---

## Files to Modify:

| File | Lines to Change | Priority |
|------|----------------|----------|
| `IRepositoryGenerator.cs` | Line 123 | HIGH |
| `RepositoryGenerator.cs` | Lines 70, 130-135, 200+ | HIGH |
| `ApiControllerGenerator.cs` | Lines 50, 150, 160, namespace | CRITICAL |
| `ProjectFileGenerator.cs` | SDK detection, packages | CRITICAL |
| `ProjectGenerationService.cs` | Lines 224, 227 (add rootNamespace) | HIGH |

---

## Testing After Fixes:

```bash
cd C:\Disk1\TargCC-Core-V2
dotnet build --configuration Release
.\test_targcc_v2.ps1 -SkipTests

# Check generated project:
cd C:\Users\User\AppData\Local\Temp\TargCCTest_*
dotnet build

# Should succeed with 0 errors!
```

---

## Quick Reference - sed Commands:

```bash
# Fix RepositoryGenerator namespace
sed -i 's/"namespace TargCC\.Infrastructure\.Repositories"/$"namespace {rootNamespace}.Infrastructure.Repositories"/g' \
    src/TargCC.Core.Generators/Repositories/RepositoryGenerator.cs

# Fix ApiControllerGenerator
sed -i 's/IRepository<\(.*\)>/I\1Repository/g' \
    src/TargCC.Core.Generators/API/ApiControllerGenerator.cs
```

---

## Summary Checklist:

- [ ] Fix `IRepositoryGenerator` signature
- [ ] Fix `RepositoryGenerator` implementation
- [ ] Fix `ApiControllerGenerator` namespaces
- [ ] Fix `ApiControllerGenerator` IRepository<T> → ICustomerRepository
- [ ] Remove DTOs from `ApiControllerGenerator` (or create DTOGenerator)
- [ ] Fix `ProjectFileGenerator` SDK type
- [ ] Add missing packages to `ProjectFileGenerator`
- [ ] Update all `GenerateAsync` calls in `ProjectGenerationService`
- [ ] Test end-to-end
- [ ] Verify generated project builds
