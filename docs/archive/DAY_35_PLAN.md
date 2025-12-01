# Day 35 Plan - Generation System Foundation
**Date:** December 1, 2025 (Planned)  
**Phase:** 3C - Local Web UI (Day 26-31)  
**Target:** Day 35/45 (62%)

---

## 🎯 Primary Objectives

### 1. Connection Form Implementation ⚡ HIGH PRIORITY
**Goal:** Allow users to create and edit database connections through UI

**Tasks:**
- [ ] Create `ConnectionForm.tsx` component
  - Form fields: Name, Server, Database, Authentication type
  - Integrated Security checkbox
  - Username/Password fields (conditional)
  - Connection string preview (read-only)
  - Test connection button
  - Save/Cancel buttons
- [ ] Add form validation
  - Required field validation
  - Connection string format validation
  - Server/Database name validation
- [ ] Wire up to ConnectionManager
  - Add connection flow
  - Edit connection flow
  - Form submission to API
- [ ] Add success/error notifications
  - Toast notifications (react-hot-toast or MUI Snackbar)
  - Error handling with user-friendly messages

**Files to Create:**
- `src/components/connections/ConnectionForm.tsx` (~200 lines)
- `src/components/connections/ConnectionFormDialog.tsx` (~100 lines)

**Files to Modify:**
- `src/pages/Connections.tsx` - Wire up form dialogs

**Tests to Write:**
- `src/__tests__/components/ConnectionForm.test.ts` (~150 lines, 8-10 tests)

**Estimated Time:** 3-4 hours

---

### 2. Generation History System 🗄️ HIGH PRIORITY
**Goal:** Track what was generated, when, and with what options

#### Backend Tasks
- [ ] Create `GenerationHistory` model
  ```csharp
  public class GenerationHistory
  {
      public string Id { get; set; }
      public string TableName { get; set; }
      public string SchemaName { get; set; }
      public DateTime GeneratedAt { get; set; }
      public string[] FilesGenerated { get; set; }
      public GenerationOptions Options { get; set; }
      public bool Success { get; set; }
      public string[] Errors { get; set; }
      public string[] Warnings { get; set; }
  }
  ```

- [ ] Create `IGenerationHistoryService` interface
  - GetHistoryAsync(tableName?)
  - AddHistoryAsync(history)
  - GetLastGenerationAsync(tableName)
  - ClearHistoryAsync()

- [ ] Create `GenerationHistoryService` implementation
  - JSON file storage at `%AppData%\TargCC\generation-history.json`
  - Thread-safe operations
  - Automatic cleanup (keep last 100 entries per table)

- [ ] Add API endpoints to Program.cs
  ```
  GET  /api/generation/history              - List all history
  GET  /api/generation/history/{tableName}  - History for specific table
  GET  /api/generation/status/{tableName}   - Current generation status
  POST /api/generation/history              - Add history entry (internal)
  ```

- [ ] Update `SchemaService` to populate generation status
  - Add `GetGenerationStatusAsync(tableName)`
  - Integrate with history service
  - Return "Generated", "Modified", "Not Generated", "Error"

**Files to Create:**
- `src/TargCC.WebAPI/Models/GenerationHistory.cs` (~40 lines)
- `src/TargCC.WebAPI/Services/IGenerationHistoryService.cs` (~25 lines)
- `src/TargCC.WebAPI/Services/GenerationHistoryService.cs` (~150 lines)

**Files to Modify:**
- `src/TargCC.WebAPI/Program.cs` - Add 4 endpoints (~80 lines)
- `src/TargCC.WebAPI/Services/SchemaService.cs` - Add status logic (~50 lines)

#### Frontend Tasks
- [ ] Create `generationApi.ts`
  - fetchGenerationHistory()
  - fetchTableStatus()
  - getGenerationDetails()

- [ ] Create `useGenerationHistory` hook
  - Load history on mount
  - Filter by table
  - Sort by date
  - Auto-refresh capability

- [ ] Update Tables page to show real status
  - Replace mock "Not Generated" with actual data
  - Show last generated date from history
  - Color code status chips properly

**Files to Create:**
- `src/api/generationApi.ts` (~80 lines)
- `src/hooks/useGenerationHistory.ts` (~100 lines)

**Files to Modify:**
- `src/pages/Tables.tsx` - Use real status data (~50 lines changed)

**Tests to Write:**
- Backend: `GenerationHistoryServiceTests.cs` (~200 lines, 10-12 tests)
- Frontend: `useGenerationHistory.test.ts` (~120 lines, 6-8 tests)

**Estimated Time:** 4-5 hours

---

### 3. Basic Code Generation Integration 🔨 MEDIUM PRIORITY
**Goal:** Wire up Generate button to actual code generation

**Tasks:**
- [ ] Create generation endpoint
  ```
  POST /api/generation/generate
  Body: {
    tableName: string,
    schemaName: string,
    options: GenerationOptions
  }
  ```

- [ ] Implement generation service integration
  - Call existing generator services
  - Capture generated files
  - Track errors/warnings
  - Record to history

- [ ] Update Tables page Generate button
  - Show loading state during generation
  - Success/error notifications
  - Auto-refresh table list after generation
  - Open generated files location (optional)

- [ ] Add generation options dialog
  - Checkboxes for entity/repo/service/controller/tests
  - Overwrite existing toggle
  - Preview mode (show what will be generated)

**Files to Create:**
- `src/TargCC.WebAPI/Services/ICodeGenerationService.cs` (~30 lines)
- `src/TargCC.WebAPI/Services/CodeGenerationService.cs` (~200 lines)
- `src/components/generation/GenerationOptionsDialog.tsx` (~150 lines)

**Files to Modify:**
- `src/TargCC.WebAPI/Program.cs` - Add generation endpoint
- `src/pages/Tables.tsx` - Wire up Generate button

**Tests to Write:**
- Backend: `CodeGenerationServiceTests.cs` (~150 lines, 8-10 tests)
- Frontend: `GenerationOptionsDialog.test.ts` (~100 lines, 6-8 tests)

**Estimated Time:** 3-4 hours

---

## 📋 Secondary Objectives (If Time Permits)

### 4. View Details Dialog 👁️ LOW PRIORITY
- [ ] Create TableDetailsDialog component
- [ ] Show table columns with data types
- [ ] Show foreign key relationships
- [ ] Show indexes
- [ ] Show sample data (using preview endpoint)

**Estimated Time:** 2 hours

### 5. Table Search Improvements 🔍 LOW PRIORITY
- [ ] Add debounced search (300ms delay)
- [ ] Search in columns as well (not just table name)
- [ ] Highlight search matches
- [ ] Add recent searches dropdown

**Estimated Time:** 1-2 hours

---

## ✅ Success Criteria

### Must Have (Day 35 Complete)
- [x] Users can add database connections through UI
- [x] Users can edit existing connections
- [x] Connection form validates input
- [x] Generation history is tracked in backend
- [x] Tables page shows real generation status
- [x] Last generated date is accurate
- [x] Generate button triggers actual code generation

### Nice to Have
- [ ] Generation options dialog
- [ ] View Details dialog
- [ ] Improved search functionality

---

## 🧪 Testing Requirements

### Backend Tests
- [ ] 10-12 new tests for GenerationHistoryService
- [ ] 8-10 new tests for CodeGenerationService
- [ ] All tests passing with >95% coverage

### Frontend Tests
- [ ] 8-10 new tests for ConnectionForm
- [ ] 6-8 new tests for useGenerationHistory
- [ ] 6-8 new tests for GenerationOptionsDialog

**Target:** Add ~35 tests total

---

## 📊 Metrics

### Code to Write
- **Backend:** ~600 lines
- **Frontend:** ~700 lines
- **Tests:** ~800 lines
- **Total:** ~2,100 lines

### Expected Test Count
- **Backend:** 724 + 20 = 744 tests
- **Frontend:** 230 + 15 = 245 tests
- **Total:** 989 tests

---

## 🚧 Potential Blockers

### Technical Challenges
1. **Generator Integration** - Need to verify existing generators work correctly
2. **File Path Handling** - Cross-platform path issues
3. **Thread Safety** - Multiple generations running simultaneously
4. **History Storage** - File size management for large projects

### Dependencies
1. Existing generator services must be tested
2. Connection string parsing logic
3. File system permissions for generation output

---

## 📝 Documentation to Update

- [ ] CHANGELOG.md - Add Day 35 entry
- [ ] README.md - Update Phase 3C to 65%
- [ ] API.md - Document new endpoints
- [ ] ARCHITECTURE.md - Document generation flow

---

## 💡 Implementation Notes

### Connection Form Best Practices
- Use Formik or react-hook-form for form management
- Add real-time validation feedback
- Show connection string preview as user types
- Test button should disable form while testing
- Success state should close dialog automatically

### Generation History Design
- Keep last 100 entries per table (prevent unlimited growth)
- Store in JSON for simplicity (can migrate to DB later)
- Include timestamps in UTC
- Hash file contents for change detection

### Code Generation Service
- Run generation in background task
- Return operation ID immediately
- Allow polling for status
- Implement cancellation support

---

## 🎯 End of Day 35 Target State

### Backend
✅ Connection CRUD working with UI  
✅ Generation history tracking functional  
✅ Real generation status in schema API  
✅ Basic generation endpoint working  

### Frontend
✅ Connection form creating/editing connections  
✅ Tables page showing accurate status  
✅ Generate button triggering code generation  
✅ Success/error notifications working  

### Tests
✅ 989+ tests passing  
✅ 95%+ code coverage maintained  
✅ All critical paths covered  

---

**Prepared by:** Claude (Sonnet 4.5)  
**Date:** November 30, 2025  
**Status:** Ready for Day 35 kickoff
