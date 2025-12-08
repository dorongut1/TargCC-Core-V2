# TargCC Core V2 - Status Document
**Last Updated:** 2025-12-08
**Branch:** claude/fix-filter-ui-and-forms-01XB9ydpguAM9AdnhRtBeuqu

## Overview
This document tracks the current status of TargCC Core V2 code generator, specifically focusing on SQL VIEW support, UI enhancements, and TypeScript improvements.

---

## ✅ Features That Should Be Working

### 1. SQL VIEW Support
**What it does:** Treats SQL VIEWs as read-only entities (no Create/Update/Delete operations)

**Expected behavior:**
- ✅ VIEWs are detected via `table.IsView` flag
- ✅ Backend API only generates `GetAll()` endpoint for VIEWs (no GetById, Create, Update, Delete)
- ✅ Frontend API client (`{name}Api.ts`) only has `getAll()` method for VIEWs
- ✅ Frontend hooks only generate `use{ClassName}s` (list) hook for VIEWs
- ✅ No write hooks generated: no `use{ClassName}` (single), `useCreate`, `useUpdate`, `useDelete`
- ✅ Detail components for VIEWs show placeholder/read-only message
- ✅ List components work normally for VIEWs with DataGrid

**Files modified:**
- `ReactApiGenerator.cs` - Lines 51-76 (conditionally generate methods based on IsView)
- `ReactHookGenerator.cs` - Lines 79-155 (skip write hooks for VIEWs)
- `ReactDetailComponentGenerator.cs` - Lines 132-167 (don't import/use write hooks for VIEWs)

### 2. Excel Export with Styling
**What it does:** Export DataGrid data to Excel with professional styling

**Expected behavior:**
- ✅ Blue header row with white bold text (RGB: 4472C4 background, FFFFFF text)
- ✅ Auto-fit column widths based on content (max 50 characters)
- ✅ Frozen header row (first row stays visible when scrolling)
- ✅ Filename includes date: `{ClassName}s_YYYY-MM-DD.xlsx`
- ✅ Uses XLSX library for Excel generation

**Implementation location:**
- `ReactListComponentGenerator.cs` - Lines 389-443 (`handleExportToExcel` method)

**Code snippet:**
```typescript
ws[cellAddress].s = {
  font: { bold: true, color: { rgb: 'FFFFFF' } },
  fill: { fgColor: { rgb: '4472C4' } },
  alignment: { horizontal: 'center', vertical: 'center' }
};
ws['!cols'] = colWidths; // Auto-width
ws['!freeze'] = { xSplit: 0, ySplit: 1 }; // Freeze header
```

### 3. Multiple Column Filters
**What it does:** Allow users to filter multiple columns simultaneously in DataGrid

**Expected behavior:**
- ✅ Clicking filter on Column A and then Column B keeps both filters active
- ✅ Uses controlled `filterModel` state
- ✅ `filterMode="client"` enables multiple filters
- ✅ "Clear All Filters" button clears both custom filters and DataGrid filters

**Implementation location:**
- `ReactListComponentGenerator.cs` - Lines 280-285 (state), Lines 336-345 (DataGrid props)

**Code snippet:**
```typescript
const [filterModel, setFilterModel] = React.useState<any>({ items: [] });

<DataGrid
  filterMode="client"
  filterModel={filterModel}
  onFilterModelChange={setFilterModel}
/>

const handleClearAllFilters = () => {
  setLocalFilters({});
  setFilters({});
  setFilterModel({ items: [] });
};
```

### 4. TypeScript Error Fixes
**What it does:** Resolve type safety issues in generated code

**Expected behavior:**
- ✅ No TS2339 errors (property does not exist)
- ✅ No TS6133 warnings (unused variables)
- ✅ Unused parameters use underscore convention: `(_, row) => ...`
- ✅ Dynamic filter properties use type assertion: `(localFilters as any).{property}`

**Files modified:**
- `ReactListComponentGenerator.cs` - Line 234 (valueGetter unused param), Line 513 (filter state type assertion)

---

## 🔧 Current Status

### Recently Applied Fixes (2025-12-08)

1. **ReactHookGenerator.cs** - VIEW Support
   - ✅ Committed (7888da6)
   - Skip generating write hooks for VIEWs
   - Only import read types for VIEWs
   - Generate only `use{ClassName}s` (getAll) hook for VIEWs

2. **ReactDetailComponentGenerator.cs** - VIEW Support
   - ✅ Committed (7888da6)
   - Don't import write hooks for VIEWs
   - Show placeholder content for VIEW detail pages
   - No delete/edit functionality for VIEWs

3. **ReactListComponentGenerator.cs** - Multiple fixes
   - ✅ Committed (fcc4b60, 17a93e5)
   - Excel export with styling and frozen panes
   - Multiple column filters with controlled state
   - TypeScript fixes for unused parameters
   - Filter state type assertions

4. **ReactApiGenerator.cs** - VIEW Support
   - ✅ Previously committed
   - Only generate read methods for VIEWs
   - No write method imports or definitions for VIEWs

### Build Status

**IMPORTANT:** After any C# source code changes:
```bash
cd /home/user/TargCC-Core-V2
dotnet build --configuration Release
```

This compiles the C# generator code into DLL files that the tool uses. Without rebuilding, changes in `.cs` files won't affect generated output.

**Generated code location:** The test output is in folders like `TargCCTest_YYYYMMDD_HHMMSS/`

---

## 🐛 Known Issues & Pending Tasks

### 1. **Missing NPM Dependencies** ⚠️ PRIORITY
**Problem:** Generated React apps require manual installation of dependencies

**Manual workaround currently needed:**
```bash
cd TargCCTest_*/client
npm install xlsx
npm install react-router-dom@latest
```

**What needs to be done:**
- [ ] Find the generator that creates `package.json` for React client
- [ ] Add to dependencies: `"xlsx": "^0.18.5"`
- [ ] Update: `"react-router-dom": "^6.20.0"` (to support modern router props)
- [ ] This will make `npm install` work automatically without manual steps

**Files to search:**
- Look for generators that create `package.json` content
- Likely in `/home/user/TargCC-Core-V2/src/TargCC.Core.Generators/UI/`
- Search pattern: generators that output package manager config

### 2. **react-router-dom Version Compatibility**
**Problem:** Old version doesn't support `future` prop in BrowserRouter

**Current error:**
```
error TS2322: Property 'future' does not exist on type 'BrowserRouterProps'
```

**Solution:** Upgrade to latest version (will be fixed by Task #1)

### 3. **TypeScript Warnings in VIEW Components** (Low priority)
**Problem:** TS6133 warnings for unused imports in VIEW detail components

**Example:**
```
'EditIcon' is declared but its value is never read
'DeleteIcon' is declared but its value is never read
```

**Why it happens:** VIEW detail components import icons but don't use them (no edit/delete for VIEWs)

**Potential fix:** Modify `ReactDetailComponentGenerator.cs` to conditionally import icons only for tables

**Impact:** Non-critical, code still compiles and runs

---

## 📋 Testing Checklist

After rebuilding and regenerating, verify:

### Backend (Swagger)
- [ ] VIEWs only show `GET /api/{ViewName}` endpoint (no POST/PUT/DELETE)
- [ ] Tables show all CRUD endpoints

### Frontend TypeScript Build
- [ ] `npm run build` succeeds with 0 errors
- [ ] No TS2339 errors about missing API methods
- [ ] Only TS6133 warnings remain (unused imports - non-critical)

### Frontend Runtime - Tables
- [ ] List page shows data in DataGrid
- [ ] Can filter multiple columns simultaneously
- [ ] Filters persist when adding more filters
- [ ] "Clear All Filters" clears everything
- [ ] Excel export downloads file with:
  - Blue header row
  - Auto-width columns
  - Frozen first row
- [ ] Create button opens form
- [ ] Edit button opens form with data
- [ ] Delete button works

### Frontend Runtime - VIEWs
- [ ] List page shows data in DataGrid
- [ ] Can filter multiple columns
- [ ] Excel export works (same as tables)
- [ ] No Create button (read-only)
- [ ] No Edit/Delete buttons
- [ ] Clicking row doesn't navigate to detail (or shows read-only message)

---

## 🔄 Development Workflow

### Making Changes to Generator

1. **Edit C# source files** in `/home/user/TargCC-Core-V2/src/TargCC.Core.Generators/`

2. **Build the project:**
   ```bash
   cd /home/user/TargCC-Core-V2
   dotnet build --configuration Release
   ```

3. **Run generator** (exact command depends on your setup)

4. **Test generated code:**
   ```bash
   cd TargCCTest_*/client
   npm install
   npm run build  # Check for TypeScript errors
   npm start      # Test runtime behavior
   ```

5. **Commit changes:**
   ```bash
   git add src/TargCC.Core.Generators/
   git commit -m "fix: Description of what was fixed"
   git push -u origin claude/fix-filter-ui-and-forms-01XB9ydpguAM9AdnhRtBeuqu
   ```

### Important Notes

- **Generated output folders** (`TargCCTest_*`) are NOT committed to Git
- Only commit changes to generator source code (`.cs` files)
- Always rebuild after editing C# files
- Test with both tables and VIEWs to ensure both work

---

## 🎯 Next Steps (Priority Order)

1. **HIGH:** Add xlsx and react-router-dom to package.json generator
   - This completes the automation - users can just run `npm install`
   - No more manual dependency installation needed

2. **MEDIUM:** Test full regeneration after all fixes
   - Ensure all 82 TypeScript errors are resolved
   - Verify Excel export styling works
   - Verify multiple filters work

3. **LOW:** Clean up unused imports in VIEW components
   - Reduce TS6133 warnings
   - Makes code cleaner but not functionally required

---

## 📚 Key Architecture Concepts

### Generator Types in TargCC

1. **Backend Generators** - Create ASP.NET Core API
   - Controllers
   - Services
   - DTOs
   - Repositories

2. **Frontend Generators** - Create React TypeScript app
   - **ReactApiGenerator** - API client (`{name}Api.ts`)
   - **ReactHookGenerator** - React Query hooks (`use{Name}.ts`)
   - **ReactListComponentGenerator** - List view with DataGrid
   - **ReactDetailComponentGenerator** - Detail/Edit form
   - **ReactTypesGenerator** - TypeScript types (`{Name}.types.ts`)

### VIEW vs Table Distinction

The `table.IsView` flag is checked throughout generators to skip write operations:

```csharp
if (!table.IsView)
{
    // Generate create/update/delete code
}
```

This ensures VIEWs are treated as read-only at every layer.

---

## 🆘 Common Issues & Solutions

### "Changes don't appear in generated code"
**Problem:** Modified C# files but generated output is the same
**Solution:** Run `dotnet build --configuration Release` to recompile

### "TypeScript errors about missing API methods"
**Problem:** Hooks trying to call methods that don't exist
**Solution:** Ensure all generators respect `table.IsView` flag, rebuild, regenerate

### "First filter clears when adding second filter"
**Problem:** DataGrid filterModel not controlled
**Solution:** Use `filterModel` state with `filterMode="client"` (already fixed)

### "Excel export has no styling"
**Problem:** Old generated code without styling
**Solution:** Rebuild generator and regenerate (styling code is in ReactListComponentGenerator.cs)

---

## 📞 Contact / Notes

- This document should be updated whenever significant changes are made
- Keep this as the source of truth for current status
- When starting new work, review this document first
- Update the "Last Updated" date at the top when making changes
