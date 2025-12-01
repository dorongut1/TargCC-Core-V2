# Day 36 Plan - Phase 3D Begins: Connection Integration & Options

**Date:** December 2, 2025 (Planned)
**Phase:** 3D - Migration & Polish (Day 36-45)
**Target:** Day 36/45 (80%)

---

## 🎯 Primary Objectives

### 1. Connection Integration with Generation ⚡ HIGH PRIORITY
**Goal:** Use selected connection for generation instead of hardcoded connection string

**Tasks:**
- [ ] Add connection context/state management
  - Global state for selected connection
  - Store selected connection ID in localStorage
  - Restore selected connection on page load

- [ ] Update Tables page
  - Add connection selector dropdown at top of page
  - Show selected connection name
  - Disable generate button if no connection selected
  - Get connection string from selected connection

- [ ] Update generate function
  - Remove hardcoded connection string
  - Use connection string from selected connection
  - Show error if no connection selected
  - Add connection validation before generation

**Files to Create:**
- `src/TargCC.WebUI/src/contexts/ConnectionContext.tsx` (~80 lines)
- `src/TargCC.WebUI/src/hooks/useConnection.ts` (~50 lines)

**Files to Modify:**
- `src/TargCC.WebUI/src/pages/Tables.tsx` - Add connection selector, use selected connection (~60 lines changed)
- `src/TargCC.WebUI/src/App.tsx` - Wrap with ConnectionProvider

**Tests to Write:**
- `src/__tests__/contexts/ConnectionContext.test.ts` (~100 lines, 6-8 tests)
- `src/__tests__/hooks/useConnection.test.ts` (~80 lines, 5-6 tests)

**Estimated Time:** 2-3 hours

---

### 2. Generation Options Dialog 🎛️ HIGH PRIORITY
**Goal:** Allow users to customize what gets generated

**Tasks:**
- [ ] Create GenerationOptionsDialog component
  - Checkboxes for:
    - Generate Entity
    - Generate Repository
    - Generate Service
    - Generate Controller
    - Generate Tests
  - Overwrite existing files toggle
  - Namespace customization
  - Output path selection (optional)
  - Preview button (show what files will be generated)
  - Generate button
  - Cancel button

- [ ] Integrate with Tables page
  - Show dialog when generate button clicked
  - Single table generation
  - Bulk generation for selected tables
  - Remember last used options (localStorage)

- [ ] Add generation options API
  - GET /api/generation/options/preview
  - POST /api/generation/validate

**Files to Create:**
- `src/TargCC.WebUI/src/components/generation/GenerationOptionsDialog.tsx` (~200 lines)
- `src/TargCC.WebUI/src/components/generation/FilePreview.tsx` (~100 lines)

**Files to Modify:**
- `src/TargCC.WebUI/src/pages/Tables.tsx` - Wire up dialog (~50 lines)
- `src/TargCC.WebAPI/Program.cs` - Add preview endpoint (~40 lines)

**Tests to Write:**
- `src/__tests__/components/GenerationOptionsDialog.test.ts` (~150 lines, 8-10 tests)
- Backend: Add tests to existing generation tests (~50 lines, 3-4 tests)

**Estimated Time:** 3-4 hours

---

### 3. Bulk Generation Implementation 🔨 MEDIUM PRIORITY
**Goal:** Generate code for multiple tables at once

**Tasks:**
- [ ] Implement handleBulkGenerate function
  - Show GenerationOptionsDialog for bulk
  - Display table list being generated
  - Progress tracking for each table
  - Handle partial failures
  - Show summary at the end

- [ ] Add progress tracking UI
  - Progress bar for overall completion
  - Individual table status (pending/in-progress/success/error)
  - Cancel operation button
  - Real-time updates

- [ ] Backend support
  - Handle multiple table generation
  - Return progress updates
  - Error handling per table
  - Transaction/rollback for batch operations

**Files to Create:**
- `src/TargCC.WebUI/src/components/generation/BulkGenerationProgress.tsx` (~150 lines)

**Files to Modify:**
- `src/TargCC.WebUI/src/pages/Tables.tsx` - Implement handleBulkGenerate (~100 lines)
- `src/TargCC.WebAPI/Program.cs` - Bulk generation endpoint (~60 lines)
- `src/TargCC.WebAPI/Services/CodeGenerationService.cs` - Add bulk support (~80 lines)

**Tests to Write:**
- `src/__tests__/components/BulkGenerationProgress.test.ts` (~120 lines, 7-8 tests)
- Backend: Bulk generation tests (~150 lines, 8-10 tests)

**Estimated Time:** 3-4 hours

---

## 📋 Secondary Objectives (If Time Permits)

### 4. View Table Details Dialog 👁️ MEDIUM PRIORITY
**Goal:** Show comprehensive table information

**Tasks:**
- [ ] Create TableDetailsDialog component
  - Tabs for: Overview, Columns, Relationships, Indexes, Data
  - Overview: Table stats (row count, size, created date)
  - Columns: List with data types, nullability, defaults
  - Relationships: Foreign keys, referenced tables
  - Indexes: Primary key, indexes, constraints
  - Data: Sample rows (first 10-20 rows)

- [ ] Add table details API
  - GET /api/schema/table/{schema}/{name}/details
  - GET /api/schema/table/{schema}/{name}/sample

**Files to Create:**
- `src/TargCC.WebUI/src/components/tables/TableDetailsDialog.tsx` (~250 lines)
- `src/TargCC.WebUI/src/components/tables/ColumnList.tsx` (reuse existing or enhance)
- `src/TargCC.WebUI/src/components/tables/SampleDataGrid.tsx` (~100 lines)

**Files to Modify:**
- `src/TargCC.WebUI/src/pages/Tables.tsx` - Wire up View Details button
- `src/TargCC.WebAPI/Services/SchemaService.cs` - Add sample data query (~40 lines)

**Tests to Write:**
- `src/__tests__/components/TableDetailsDialog.test.ts` (~120 lines, 6-8 tests)

**Estimated Time:** 2-3 hours

---

### 5. Connection Selector Improvements 🔗 LOW PRIORITY
**Goal:** Better UX for connection management

**Tasks:**
- [ ] Add connection status indicator in selector
  - Show last successful connection test
  - Auto-test connection on selection
  - Warning for stale connections

- [ ] Quick connection actions
  - Test connection from dropdown
  - Edit connection from dropdown
  - Delete connection from dropdown

- [ ] Connection groups/favorites
  - Mark connections as favorite
  - Group connections by server/environment

**Estimated Time:** 1-2 hours

---

## ✅ Success Criteria

### Must Have (Day 36 Complete)
- [ ] Selected connection used for generation (no hardcoded connection string)
- [ ] Connection selector in Tables page working
- [ ] GenerationOptionsDialog functional
- [ ] Users can customize what gets generated
- [ ] Bulk generation working for multiple tables
- [ ] Progress tracking for bulk operations
- [ ] All new features tested

### Nice to Have
- [ ] View Table Details dialog
- [ ] Connection status indicators
- [ ] Quick connection actions

---

## 🧪 Testing Requirements

### Frontend Tests
- [ ] 6-8 tests for ConnectionContext
- [ ] 5-6 tests for useConnection hook
- [ ] 8-10 tests for GenerationOptionsDialog
- [ ] 7-8 tests for BulkGenerationProgress
- [ ] 6-8 tests for TableDetailsDialog (if implemented)

### Backend Tests
- [ ] 3-4 tests for preview endpoint
- [ ] 8-10 tests for bulk generation
- [ ] All tests passing with >95% coverage

**Target:** Add ~35 tests total

---

## 📊 Metrics

### Code to Write
- **Frontend:** ~1,100 lines (components + hooks + context)
- **Backend:** ~300 lines (endpoints + services)
- **Tests:** ~900 lines
- **Total:** ~2,300 lines

### Expected Test Count
- **Backend:** 727 + 12 = 739 tests
- **Frontend:** 527 + 35 = 562 tests
- **Total:** 1,301 tests

---

## 🚧 Potential Blockers

### Technical Challenges
1. **Connection State Management** - Need global state across components
2. **Bulk Generation Progress** - Real-time updates without WebSocket
3. **File Preview** - Show what will be generated without actually generating
4. **Error Handling** - Partial failures in bulk operations

### Dependencies
1. Existing connection management must work
2. Generation endpoints must be stable
3. Schema service must provide all needed data

---

## 📝 Implementation Notes

### Connection Context Best Practices
- Use React Context for global state
- Persist selected connection to localStorage
- Auto-restore on app load
- Provide easy-to-use hook (useConnection)
- Handle connection changes gracefully

### Generation Options Dialog
- Default to same options as last generation
- Validate options before allowing generation
- Show preview of files to be generated
- Clear error messages for invalid options

### Bulk Generation
- Process tables sequentially (avoid resource exhaustion)
- Allow cancellation mid-process
- Show progress in real-time
- Provide detailed summary at end
- Handle errors per table (don't fail entire batch)

---

## 🎯 End of Day 36 Target State

### Frontend
✅ Connection selector working in Tables page
✅ Selected connection used for generation
✅ GenerationOptionsDialog functional
✅ Bulk generation with progress tracking
✅ No hardcoded connection strings

### Backend
✅ Preview endpoint for generation options
✅ Bulk generation endpoint
✅ Proper error handling per table

### Tests
✅ 1,301+ tests passing
✅ 95%+ code coverage maintained
✅ All critical paths covered

---

## 🔜 Day 37 Preview

After Day 36, we'll focus on:
- Migration script generation
- Version tracking system
- Table details enhancements
- Performance optimizations
- UI/UX polish

---

**Prepared by:** Claude (Sonnet 4.5)
**Date:** December 1, 2025
**Status:** Ready for Day 36 kickoff
