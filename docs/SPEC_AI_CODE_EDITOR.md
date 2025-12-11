# Phase 3F: AI-Powered Interactive Code Editor - Specification

**Version:** 1.1
**Date:** December 10, 2025
**Status:** ✅ COMPLETE - Fully Implemented
**Phase:** 3F - AI Interactive Code Editor
**Implementation Date:** December 2-10, 2025

---

## ✅ Implementation Status

**Phase 3F is now COMPLETE and PRODUCTION READY!**

All planned features have been implemented:
- ✅ AICodeEditorService (Backend) - 434 lines
- ✅ AI Chat Panel (Frontend) - 8.3KB
- ✅ Monaco Editor Integration - 12KB
- ✅ Code Validation System
- ✅ Diff Generation Engine
- ✅ API Endpoints (3 endpoints)
- ✅ Demo Page & Components

**See:** `PHASE_3F_AI_CODE_EDITOR_COMPLETE.md` for full implementation details.

---

## 🎯 Overview

**Phase 3F** introduces an AI-powered interactive code editor that allows users to modify generated code through natural language commands. The system leverages Claude AI to understand user intent and apply modifications to generated React components, maintaining code quality and consistency.

### Vision

Enable users to refine auto-generated code without manual editing:

```
User generates CustomerForm.tsx (auto-generated)
  ↓
Opens in AI Code Editor
  ↓
User: "Move Email field to the left, make Save button blue"
  ↓
AI understands context + applies changes
  ↓
Live preview shows result
  ↓
User approves → Code saved
```

---

## 🌟 Key Features

### 1. **Context-Aware AI Editing**
- AI receives full context: schema, relationships, conventions
- Understands 12 prefix types (eno_, ent_, lkp_, etc.)
- Maintains Material-UI + Formik patterns
- Preserves validation rules

### 2. **Live Preview**
- Real-time rendering of modifications
- Side-by-side comparison (before/after)
- Interactive component preview
- Error highlighting

### 3. **Version Control**
- Undo/Redo functionality
- Track all modifications
- Revert to any version
- Diff viewer

### 4. **Safe Modifications**
- Validation before applying changes
- TypeScript compilation check
- Linting verification
- Breaking change detection

### 5. **Multi-File Awareness**
- Modify related files (types, API, hooks)
- Update imports automatically
- Maintain consistency across files

---

## 🏗️ Architecture

### Backend Components

#### 1. AICodeEditorService
**Location:** `src/TargCC.Core.Services/AI/AICodeEditorService.cs`

**Responsibilities:**
- Coordinate code modification workflow
- Build comprehensive context for AI
- Validate and apply code changes
- Manage conversation history

**Key Methods:**
```csharp
public class AICodeEditorService
{
    // Main modification method
    Task<CodeModificationResult> ModifyCodeAsync(
        string filePath,
        string userRequest,
        ModificationContext context,
        CancellationToken cancellationToken);

    // Build context for AI
    CodeContext BuildCodeContext(
        string filePath,
        Table table,
        List<Table> relatedTables);

    // Validate changes
    Task<ValidationResult> ValidateModificationAsync(
        string originalCode,
        string modifiedCode,
        string fileType);

    // Generate diff
    CodeDiff GenerateDiff(
        string originalCode,
        string modifiedCode);
}
```

#### 2. AICodeEditorPromptBuilder
**Location:** `src/TargCC.AI/Prompts/AICodeEditorPromptBuilder.cs`

**Responsibilities:**
- Build specialized prompts for code editing
- Include schema context
- Add coding conventions
- Specify output format

**Prompt Structure:**
```
System Message:
- You are an expert React/TypeScript developer
- Maintain Material-UI patterns
- Preserve Formik validation
- Follow project conventions

User Message:
- Current code: [file content]
- Schema context: [table definition]
- Relationships: [foreign keys]
- User request: [natural language command]
- Output: Complete modified file
```

#### 3. CodeModificationParser
**Location:** `src/TargCC.AI/Parsers/CodeModificationParser.cs`

**Responsibilities:**
- Parse AI response
- Extract modified code
- Identify changes made
- Generate change summary

#### 4. API Endpoints
**Location:** `src/TargCC.WebAPI/Program.cs`

```csharp
// Modify generated code
app.MapPost("/api/ai/modify-code", async (
    ModifyCodeRequest request,
    AICodeEditorService service) => { });

// Chat with AI about code
app.MapPost("/api/ai/chat", async (
    ChatRequest request,
    ClaudeAIService service) => { });

// Get conversation history
app.MapGet("/api/ai/conversation/{id}", async (
    string id,
    ConversationService service) => { });

// Get code versions
app.MapGet("/api/ai/versions/{fileId}", async (
    string fileId,
    CodeVersionService service) => { });

// Revert to version
app.MapPost("/api/ai/revert/{versionId}", async (
    string versionId,
    CodeVersionService service) => { });
```

---

### Frontend Components

#### 1. AICodeEditor Component
**Location:** `src/TargCC.WebUI/src/components/ai/AICodeEditor.tsx`

**Features:**
- Monaco Editor integration
- Split view (code + preview)
- AI chat panel
- Toolbar with actions

**UI Layout:**
```
┌─────────────────────────────────────────────┐
│ Toolbar: [File] [Undo] [Redo] [Save] [Close]│
├─────────────────────┬───────────────────────┤
│  Monaco Editor      │   Live Preview        │
│  (Code)             │   (Rendered Component)│
│                     │                       │
│  [AI Chat Panel]    │   [Diff Viewer]       │
│  User: "Make blue"  │   (When needed)       │
│  AI: "Applied..."   │                       │
└─────────────────────┴───────────────────────┘
```

#### 2. Monaco Editor Integration
**Package:** `@monaco-editor/react`

**Configuration:**
```typescript
<MonacoEditor
  language="typescript"
  theme="vs-dark"
  value={code}
  onChange={handleCodeChange}
  options={{
    minimap: { enabled: true },
    formatOnPaste: true,
    formatOnType: true,
    autoIndent: 'full',
  }}
/>
```

#### 3. AI Chat Panel
**Location:** `src/TargCC.WebUI/src/components/ai/AIChatPanel.tsx`

**Features:**
- Conversation history
- Message input
- Loading states
- Error handling
- Suggested prompts

**Message Types:**
```typescript
interface ChatMessage {
  id: string;
  role: 'user' | 'assistant' | 'system';
  content: string;
  timestamp: Date;
  codeChanges?: CodeChange[];
}
```

#### 4. Live Preview Component
**Location:** `src/TargCC.WebUI/src/components/ai/LivePreview.tsx`

**Approaches:**

**Option A: iframe Sandbox**
```typescript
<iframe
  srcDoc={generatePreviewHtml(code)}
  sandbox="allow-scripts"
  style={{ width: '100%', height: '100%' }}
/>
```

**Option B: Dynamic Component (Preferred)**
```typescript
// Use React.lazy + dynamic import
const DynamicComponent = useMemo(() => {
  return compileTSXToComponent(code);
}, [code]);

return <DynamicComponent />;
```

#### 5. Diff Viewer
**Package:** `react-diff-viewer-continued`

```typescript
<DiffViewer
  oldValue={originalCode}
  newValue={modifiedCode}
  splitView={true}
  showDiffOnly={false}
  useDarkTheme={true}
/>
```

---

## 📦 Data Models

### Backend Models

#### ModifyCodeRequest
```csharp
public class ModifyCodeRequest
{
    public string FilePath { get; set; }
    public string UserRequest { get; set; }
    public string? ConversationId { get; set; }
    public ModificationContext Context { get; set; }
}

public class ModificationContext
{
    public string TableName { get; set; }
    public string Schema { get; set; }
    public List<string> RelatedTables { get; set; }
    public Dictionary<string, string> UserPreferences { get; set; }
}
```

#### CodeModificationResult
```csharp
public class CodeModificationResult
{
    public bool Success { get; set; }
    public string ModifiedCode { get; set; }
    public string OriginalCode { get; set; }
    public List<CodeChange> Changes { get; set; }
    public ValidationResult Validation { get; set; }
    public string? ErrorMessage { get; set; }
    public string ConversationId { get; set; }
}

public class CodeChange
{
    public int LineNumber { get; set; }
    public string Type { get; set; } // "addition", "deletion", "modification"
    public string Description { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
}
```

#### ValidationResult
```csharp
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; }
    public List<ValidationWarning> Warnings { get; set; }
    public bool HasBreakingChanges { get; set; }
}
```

#### CodeVersion
```csharp
public class CodeVersion
{
    public string Id { get; set; }
    public string FileId { get; set; }
    public string Code { get; set; }
    public DateTime Timestamp { get; set; }
    public string UserRequest { get; set; }
    public List<CodeChange> Changes { get; set; }
}
```

### Frontend Models

#### TypeScript Interfaces
```typescript
interface ModifyCodeRequest {
  filePath: string;
  userRequest: string;
  conversationId?: string;
  context: ModificationContext;
}

interface ModificationContext {
  tableName: string;
  schema: string;
  relatedTables: string[];
  userPreferences: Record<string, string>;
}

interface CodeModificationResult {
  success: boolean;
  modifiedCode: string;
  originalCode: string;
  changes: CodeChange[];
  validation: ValidationResult;
  errorMessage?: string;
  conversationId: string;
}

interface CodeChange {
  lineNumber: number;
  type: 'addition' | 'deletion' | 'modification';
  description: string;
  oldValue: string;
  newValue: string;
}
```

---

## 🔄 Workflow

### User Interaction Flow

```
1. User generates code (e.g., CustomerForm.tsx)
   ↓
2. Clicks "Open in AI Editor" button
   ↓
3. AICodeEditor loads:
   - Monaco Editor shows code
   - Live Preview renders component
   - AI Chat Panel ready
   ↓
4. User types request: "Move Email field to left column"
   ↓
5. Frontend sends request to backend
   ↓
6. Backend:
   a. Builds code context (schema, relations)
   b. Creates AI prompt with context
   c. Calls Claude AI API
   d. Parses AI response
   e. Validates modifications
   f. Returns result
   ↓
7. Frontend:
   a. Updates Monaco Editor with new code
   b. Refreshes Live Preview
   c. Shows Diff Viewer (optional)
   d. Adds to version history
   ↓
8. User reviews:
   - Approves → Save
   - Requests changes → Repeat from step 4
   - Reverts → Undo to previous version
```

### Technical Flow

```
┌─────────────┐
│   User      │
└──────┬──────┘
       │ "Make button blue"
       ↓
┌─────────────────────┐
│  AICodeEditor.tsx   │
│  - Capture request  │
│  - Show loading     │
└──────┬──────────────┘
       │ POST /api/ai/modify-code
       ↓
┌──────────────────────────┐
│  AICodeEditorService.cs  │
│  1. BuildCodeContext()   │
│  2. Create AI prompt     │
│  3. Call ClaudeAI        │
│  4. Parse response       │
│  5. Validate             │
└──────┬───────────────────┘
       │ CodeModificationResult
       ↓
┌─────────────────────┐
│  AICodeEditor.tsx   │
│  - Update editor    │
│  - Refresh preview  │
│  - Show diff        │
│  - Save version     │
└─────────────────────┘
```

---

## 🎨 User Experience

### Example Interactions

#### Example 1: Layout Changes
```
User: "Put the Email field in the left column next to First Name"

AI Response:
✓ Moved Email field to left FormControl
✓ Updated grid layout (6 cols each)
✓ Preserved validation rules

Changes Applied:
- Line 45: Added Email to left column
- Line 62: Removed Email from right column
- Line 23: Updated Grid xs={12} → xs={6}
```

#### Example 2: Styling Changes
```
User: "Make the Save button blue and larger"

AI Response:
✓ Changed button color to primary (blue)
✓ Increased button size to large
✓ Maintained Material-UI props

Changes Applied:
- Line 123: color="inherit" → color="primary"
- Line 124: size="medium" → size="large"
```

#### Example 3: Field Modifications
```
User: "Add a phone number field after Email with validation"

AI Response:
✓ Added ent_Phone field
✓ Created Yup validation (phone format)
✓ Added to Formik initialValues
✓ Inserted TextField component

Changes Applied:
- Line 15: Added phone validation schema
- Line 32: Added phone to initialValues
- Line 87: Inserted Phone TextField
```

#### Example 4: Complex Changes
```
User: "Split full name into first and last name fields"

AI Response:
✓ Replaced ent_FullName with ent_FirstName + ent_LastName
✓ Updated validation (both required)
✓ Updated type definitions
✓ Modified grid layout (6 cols each)

Changes Applied:
- CustomerForm.tsx: 8 changes
- Customer.types.ts: 3 changes (auto-suggested)

⚠️ Warning: This affects the type definition.
   Open Customer.types.ts to review?
```

---

## 🔐 Safety & Validation

### Pre-Modification Checks

1. **File Lock Check**
   - Ensure file not modified externally
   - Prevent concurrent edits

2. **Syntax Validation**
   - TypeScript compilation
   - ESLint validation
   - Import verification

3. **Breaking Change Detection**
   - Interface changes
   - Prop modifications
   - Type mismatches

4. **Dependency Check**
   - Related files impact
   - Import chain verification

### Post-Modification Validation

1. **Code Quality**
   - Run ESLint
   - Check formatting (Prettier)
   - Verify TypeScript types

2. **Component Testing**
   - Compile check
   - Runtime error detection
   - Preview rendering

3. **Diff Review**
   - Show all changes
   - Highlight critical changes
   - Allow selective apply

---

## 🧪 Testing Strategy

### Backend Tests

#### AICodeEditorService Tests
```csharp
[Fact]
public async Task ModifyCodeAsync_ValidRequest_ReturnsSuccess()

[Fact]
public async Task ModifyCodeAsync_InvalidCode_ReturnsValidationError()

[Fact]
public async Task BuildCodeContext_WithRelations_IncludesAllTables()

[Fact]
public async Task ValidateModification_BreakingChange_ReturnsWarning()
```

#### Prompt Builder Tests
```csharp
[Fact]
public void BuildPrompt_IncludesSchemaContext()

[Fact]
public void BuildPrompt_IncludesUserPreferences()

[Fact]
public void BuildPrompt_FormatsCorrectly()
```

### Frontend Tests

#### AICodeEditor Tests
```typescript
test('renders Monaco Editor with code', () => {});
test('sends modification request on user input', () => {});
test('updates editor with AI response', () => {});
test('shows diff viewer on request', () => {});
test('handles undo/redo correctly', () => {});
```

#### Live Preview Tests
```typescript
test('renders component from code', () => {});
test('updates on code change', () => {});
test('handles compilation errors gracefully', () => {});
```

### Integration Tests

```typescript
test('end-to-end: modify code and preview', async () => {
  // 1. Generate code
  // 2. Open in editor
  // 3. Send modification request
  // 4. Verify AI response
  // 5. Check preview update
  // 6. Save and verify file
});
```

---

## 📊 Success Metrics

### Functionality Metrics
- ✅ 90%+ successful modifications
- ✅ <2s response time (AI call)
- ✅ Zero breaking changes without warning
- ✅ 100% syntax-valid output

### User Experience Metrics
- ✅ <3 iterations to desired result
- ✅ Natural language understanding: 85%+
- ✅ User satisfaction: 4.5/5

### Code Quality Metrics
- ✅ Maintains StyleCop compliance
- ✅ Preserves Material-UI patterns
- ✅ TypeScript strict mode compliance
- ✅ ESLint: 0 errors, <5 warnings

---

## 🚀 Implementation Roadmap

### Phase 1: Foundation (3-4 hours)
- ✅ Create AICodeEditorService
- ✅ Build prompt builder
- ✅ Implement basic API endpoints
- ✅ Set up response parser

### Phase 2: Frontend Core (4-5 hours)
- ✅ Build AICodeEditor component
- ✅ Integrate Monaco Editor
- ✅ Create AI Chat Panel
- ✅ Basic request/response flow

### Phase 3: Preview & Diff (2-3 hours)
- ✅ Implement Live Preview
- ✅ Add Diff Viewer
- ✅ Version history UI

### Phase 4: Polish & Testing (3-4 hours)
- ✅ Validation logic
- ✅ Error handling
- ✅ Unit tests (backend)
- ✅ Component tests (frontend)
- ✅ Integration tests

**Total Estimated Time: 12-16 hours**

---

## 📋 Task Breakdown

### Backend Tasks

1. **Create AICodeEditorService** (2h)
   - [ ] Service class skeleton
   - [ ] ModifyCodeAsync method
   - [ ] BuildCodeContext method
   - [ ] ValidateModificationAsync method
   - [ ] GenerateDiff method

2. **Create Prompt Builder** (1h)
   - [ ] AICodeEditorPromptBuilder class
   - [ ] System message template
   - [ ] User message with context
   - [ ] Output format specification

3. **Create Parser** (1h)
   - [ ] CodeModificationParser class
   - [ ] Extract code from response
   - [ ] Parse change summary
   - [ ] Error handling

4. **API Endpoints** (1h)
   - [ ] POST /api/ai/modify-code
   - [ ] POST /api/ai/chat
   - [ ] GET /api/ai/conversation/{id}
   - [ ] GET /api/ai/versions/{fileId}
   - [ ] POST /api/ai/revert/{versionId}

5. **Models** (30min)
   - [ ] ModifyCodeRequest
   - [ ] CodeModificationResult
   - [ ] CodeChange
   - [ ] ValidationResult
   - [ ] CodeVersion

### Frontend Tasks

6. **AICodeEditor Component** (2h)
   - [ ] Component skeleton
   - [ ] Layout (split view)
   - [ ] State management
   - [ ] Toolbar

7. **Monaco Editor Integration** (1h)
   - [ ] Install @monaco-editor/react
   - [ ] Configure editor
   - [ ] Syntax highlighting
   - [ ] Auto-formatting

8. **AI Chat Panel** (2h)
   - [ ] Chat UI
   - [ ] Message list
   - [ ] Input field
   - [ ] Send/receive messages
   - [ ] Loading states

9. **Live Preview** (2h)
   - [ ] Component renderer
   - [ ] TSX compilation
   - [ ] Error boundary
   - [ ] Refresh logic

10. **Diff Viewer** (1h)
    - [ ] Install react-diff-viewer-continued
    - [ ] Side-by-side view
    - [ ] Highlighting
    - [ ] Toggle visibility

11. **Version History** (1h)
    - [ ] Version list UI
    - [ ] Revert functionality
    - [ ] Undo/Redo buttons
    - [ ] Diff between versions

12. **API Integration** (1h)
    - [ ] API client functions
    - [ ] React hooks (useModifyCode, useChat)
    - [ ] Error handling
    - [ ] Loading states

### Testing Tasks

13. **Backend Tests** (2h)
    - [ ] AICodeEditorService tests (5 tests)
    - [ ] Prompt builder tests (3 tests)
    - [ ] Parser tests (3 tests)
    - [ ] API endpoint tests (5 tests)

14. **Frontend Tests** (2h)
    - [ ] AICodeEditor tests (5 tests)
    - [ ] Chat panel tests (3 tests)
    - [ ] Live preview tests (3 tests)
    - [ ] Integration test (1 test)

---

## 🔧 Dependencies

### Backend
- ✅ Existing: `ClaudeAIService`
- ✅ Existing: `AIConfiguration`
- ✅ Existing: `ConversationContext`
- 🆕 New: `AICodeEditorService`
- 🆕 New: `CodeVersionService`

### Frontend
- 🆕 `@monaco-editor/react` (^4.6.0)
- 🆕 `react-diff-viewer-continued` (^3.3.0)
- ✅ Existing: Material-UI
- ✅ Existing: React Query
- ✅ Existing: Axios

### AI
- ✅ Claude API (Anthropic)
- ✅ API Key configuration
- ✅ Model: claude-sonnet-4-5

---

## 🎯 Acceptance Criteria

### Must Have
- ✅ User can modify generated code via natural language
- ✅ AI understands schema context
- ✅ Live preview shows changes immediately
- ✅ Validation prevents breaking changes
- ✅ Undo/Redo functionality works
- ✅ Code saved maintains quality standards

### Should Have
- ✅ Diff viewer shows before/after
- ✅ Conversation history maintained
- ✅ Multi-file awareness (suggests related changes)
- ✅ Suggested prompts for common tasks

### Nice to Have
- ⚠️ Batch modifications (multiple files)
- ⚠️ AI explains changes in detail
- ⚠️ Export conversation history
- ⚠️ Custom prompt templates

---

## 📖 Documentation Requirements

1. **User Guide**
   - How to use AI Code Editor
   - Common prompts/examples
   - Tips for best results

2. **Developer Guide**
   - Architecture overview
   - Adding new AI capabilities
   - Extending prompt templates

3. **API Documentation**
   - Endpoint specifications
   - Request/response formats
   - Error codes

4. **Testing Guide**
   - How to test AI features
   - Mock AI responses
   - Test data setup

---

## 🔮 Future Enhancements (Post-MVP)

### Phase 3F.1: Advanced Features
- Multi-file batch editing
- AI-suggested improvements
- Code review mode
- Template library

### Phase 3F.2: Collaboration
- Share conversations
- Team prompt templates
- Code review comments
- Approval workflow

### Phase 3F.3: Intelligence
- Learn from user preferences
- Auto-suggest based on patterns
- Code quality scoring
- Best practice recommendations

---

## ⚠️ Known Limitations

1. **AI Limitations**
   - May not understand very complex requests
   - Requires clear, specific instructions
   - Limited to modifications (not full rewrites)

2. **Technical Limitations**
   - Monaco Editor: browser-only
   - Live Preview: React components only
   - File size: <10,000 lines recommended

3. **Scope Limitations**
   - Phase 3F focuses on UI components
   - Backend code modifications: future phase
   - SQL modifications: not included

---

**Document Status:** Draft
**Next Steps:** Review → Approve → Implement
**Owner:** Development Team
**Reviewers:** Product, Engineering, QA

---

*This specification is a living document and will be updated as requirements evolve.*
