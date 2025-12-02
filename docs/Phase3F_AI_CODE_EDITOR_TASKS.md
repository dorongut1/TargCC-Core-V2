# Phase 3F: AI-Powered Code Editor - Task Breakdown

**Phase:** 3F - AI Interactive Code Editor
**Status:** Planning Complete, Ready to Implement
**Estimated Time:** 12-16 hours
**Start Date:** TBD
**Target Completion:** TBD

---

## 📊 Task Overview

| Category | Tasks | Estimated Time | Status |
|----------|-------|----------------|--------|
| **Backend** | 5 tasks | 5.5 hours | ⏸️ Not Started |
| **Frontend** | 6 tasks | 9 hours | ⏸️ Not Started |
| **Testing** | 2 tasks | 4 hours | ⏸️ Not Started |
| **Documentation** | 1 task | 1 hour | ⏸️ Not Started |
| **Total** | **14 tasks** | **19.5 hours** | **0% Complete** |

---

## 🔧 Backend Tasks (5.5 hours)

### Task 1: Create AICodeEditorService ⏱️ 2h
**Priority:** Critical
**Dependencies:** None
**Status:** ⏸️ Not Started

**Deliverables:**
- [ ] `src/TargCC.Core.Services/AI/AICodeEditorService.cs` created
- [ ] `ModifyCodeAsync()` method implemented
- [ ] `BuildCodeContext()` method implemented
- [ ] `ValidateModificationAsync()` method implemented
- [ ] `GenerateDiff()` method implemented
- [ ] XML documentation complete
- [ ] Logging configured

**Acceptance Criteria:**
- Service compiles without errors
- All methods have proper error handling
- Logging uses LoggerMessage pattern
- Context includes schema + relationships + conventions

**Files to Create:**
```
src/TargCC.Core.Services/AI/
├── AICodeEditorService.cs
└── CodeContext.cs (helper model)
```

---

### Task 2: Create Prompt Builder ⏱️ 1h
**Priority:** Critical
**Dependencies:** Task 1
**Status:** ⏸️ Not Started

**Deliverables:**
- [ ] `src/TargCC.AI/Prompts/AICodeEditorPromptBuilder.cs` created
- [ ] System message template defined
- [ ] User message with context builder
- [ ] Output format specification
- [ ] Example prompts documented

**Acceptance Criteria:**
- Prompts include full schema context
- Conventions clearly specified
- Output format is JSON-parseable
- Examples provided for common scenarios

**Files to Create:**
```
src/TargCC.AI/Prompts/
└── AICodeEditorPromptBuilder.cs
```

---

### Task 3: Create Response Parser ⏱️ 1h
**Priority:** Critical
**Dependencies:** Task 2
**Status:** ⏸️ Not Started

**Deliverables:**
- [ ] `src/TargCC.AI/Parsers/CodeModificationParser.cs` created
- [ ] Extract modified code from AI response
- [ ] Parse change summary
- [ ] Error handling for malformed responses
- [ ] Validation of parsed code

**Acceptance Criteria:**
- Handles various AI response formats
- Validates extracted code syntax
- Returns structured CodeChange objects
- Error messages are descriptive

**Files to Create:**
```
src/TargCC.AI/Parsers/
└── CodeModificationParser.cs
```

---

### Task 4: Create API Endpoints ⏱️ 1h
**Priority:** High
**Dependencies:** Tasks 1-3
**Status:** ⏸️ Not Started

**Deliverables:**
- [ ] POST `/api/ai/modify-code` endpoint
- [ ] POST `/api/ai/chat` endpoint
- [ ] GET `/api/ai/conversation/{id}` endpoint
- [ ] GET `/api/ai/versions/{fileId}` endpoint
- [ ] POST `/api/ai/revert/{versionId}` endpoint
- [ ] Swagger documentation
- [ ] CORS configured

**Acceptance Criteria:**
- All endpoints return proper HTTP status codes
- Request/response models are validated
- Errors return structured error messages
- Swagger UI shows all endpoints

**Files to Modify:**
```
src/TargCC.WebAPI/Program.cs (add endpoints)
```

---

### Task 5: Create Data Models ⏱️ 30min
**Priority:** High
**Dependencies:** None
**Status:** ⏸️ Not Started

**Deliverables:**
- [ ] `ModifyCodeRequest.cs`
- [ ] `ModifyCodeResponse.cs` (or `CodeModificationResult`)
- [ ] `CodeChange.cs`
- [ ] `ValidationResult.cs`
- [ ] `CodeVersion.cs`
- [ ] `ModificationContext.cs`

**Acceptance Criteria:**
- All properties have XML docs
- Data annotations for validation
- JSON serialization tested
- Nullable reference types used correctly

**Files to Create:**
```
src/TargCC.Core.Services/AI/Models/
├── ModifyCodeRequest.cs
├── CodeModificationResult.cs
├── CodeChange.cs
├── ValidationResult.cs
├── CodeVersion.cs
└── ModificationContext.cs
```

---

## 🎨 Frontend Tasks (9 hours)

### Task 6: Create AICodeEditor Component ⏱️ 2h
**Priority:** Critical
**Dependencies:** Backend Tasks 1-4
**Status:** ⏸️ Not Started

**Deliverables:**
- [ ] `src/TargCC.WebUI/src/components/ai/AICodeEditor.tsx` created
- [ ] Layout with split view (code + preview)
- [ ] Toolbar with actions (Save, Undo, Redo, Close)
- [ ] State management (code, versions, conversation)
- [ ] Responsive design

**Acceptance Criteria:**
- Component renders without errors
- Layout adapts to screen size
- Toolbar actions are functional
- State updates correctly

**Files to Create:**
```
src/TargCC.WebUI/src/components/ai/
└── AICodeEditor.tsx
```

---

### Task 7: Integrate Monaco Editor ⏱️ 1h
**Priority:** Critical
**Dependencies:** Task 6
**Status:** ⏸️ Not Started

**Deliverables:**
- [ ] Install `@monaco-editor/react` package
- [ ] Configure Monaco Editor
- [ ] TypeScript/TSX syntax highlighting
- [ ] Auto-formatting enabled
- [ ] Theme configuration (dark/light)

**Acceptance Criteria:**
- Editor loads and displays code
- Syntax highlighting works for TSX
- Auto-complete enabled
- Formatting on save works

**Dependencies:**
```bash
npm install @monaco-editor/react
```

---

### Task 8: Create AI Chat Panel ⏱️ 2h
**Priority:** High
**Dependencies:** Task 6
**Status:** ⏸️ Not Started

**Deliverables:**
- [ ] `src/TargCC.WebUI/src/components/ai/AIChatPanel.tsx` created
- [ ] Message list UI
- [ ] Input field with send button
- [ ] Loading states (typing indicator)
- [ ] Error handling
- [ ] Suggested prompts (helpful examples)

**Acceptance Criteria:**
- Messages display in conversation format
- User can send messages
- Loading indicator shows during AI processing
- Errors are displayed gracefully
- Suggested prompts are clickable

**Files to Create:**
```
src/TargCC.WebUI/src/components/ai/
├── AIChatPanel.tsx
└── ChatMessage.tsx (sub-component)
```

---

### Task 9: Create Live Preview Component ⏱️ 2h
**Priority:** High
**Dependencies:** Task 6
**Status:** ⏸️ Not Started

**Deliverables:**
- [ ] `src/TargCC.WebUI/src/components/ai/LivePreview.tsx` created
- [ ] Component renderer (compile TSX to component)
- [ ] Error boundary for runtime errors
- [ ] Refresh on code change
- [ ] Loading state

**Acceptance Criteria:**
- Renders React components from code string
- Updates when code changes
- Shows compilation errors gracefully
- Handles runtime errors without crashing

**Files to Create:**
```
src/TargCC.WebUI/src/components/ai/
├── LivePreview.tsx
└── PreviewErrorBoundary.tsx
```

**Note:** This is complex - may need iframe approach as fallback.

---

### Task 10: Add Diff Viewer ⏱️ 1h
**Priority:** Medium
**Dependencies:** Task 6
**Status:** ⏸️ Not Started

**Deliverables:**
- [ ] Install `react-diff-viewer-continued` package
- [ ] `src/TargCC.WebUI/src/components/ai/DiffViewer.tsx` created
- [ ] Side-by-side view
- [ ] Syntax highlighting
- [ ] Toggle visibility

**Acceptance Criteria:**
- Shows before/after code comparison
- Highlights changes clearly
- Can toggle between unified/split view
- Works with dark theme

**Dependencies:**
```bash
npm install react-diff-viewer-continued
```

**Files to Create:**
```
src/TargCC.WebUI/src/components/ai/
└── DiffViewer.tsx
```

---

### Task 11: Create Version History UI ⏱️ 1h
**Priority:** Low
**Dependencies:** Task 6
**Status:** ⏸️ Not Started

**Deliverables:**
- [ ] `src/TargCC.WebUI/src/components/ai/VersionHistory.tsx` created
- [ ] List of versions with timestamps
- [ ] Revert functionality
- [ ] Undo/Redo buttons
- [ ] Diff between versions

**Acceptance Criteria:**
- Shows all saved versions
- User can revert to any version
- Undo/Redo buttons work correctly
- Can view diff between any two versions

**Files to Create:**
```
src/TargCC.WebUI/src/components/ai/
└── VersionHistory.tsx
```

---

### Task 12: Create API Integration & Hooks ⏱️ 1h
**Priority:** Critical
**Dependencies:** Backend Task 4
**Status:** ⏸️ Not Started

**Deliverables:**
- [ ] `src/TargCC.WebUI/src/api/aiApi.ts` created
- [ ] `modifyCode()` API function
- [ ] `chat()` API function
- [ ] `getConversation()` API function
- [ ] `getVersions()` API function
- [ ] `revertToVersion()` API function
- [ ] React hooks: `useModifyCode`, `useChat`, `useVersions`
- [ ] Error handling
- [ ] Loading states

**Acceptance Criteria:**
- All API functions work correctly
- Hooks handle loading/error states
- Type-safe request/response models
- Retry logic for network errors

**Files to Create:**
```
src/TargCC.WebUI/src/api/
└── aiApi.ts

src/TargCC.WebUI/src/hooks/
└── useAIEditor.ts
```

---

## 🧪 Testing Tasks (4 hours)

### Task 13: Backend Tests ⏱️ 2h
**Priority:** High
**Dependencies:** Backend Tasks 1-4
**Status:** ⏸️ Not Started

**Deliverables:**
- [ ] AICodeEditorService tests (5 tests minimum)
  - [ ] ModifyCodeAsync with valid request returns success
  - [ ] ModifyCodeAsync with invalid code returns validation error
  - [ ] BuildCodeContext includes all schema info
  - [ ] ValidateModification detects breaking changes
  - [ ] GenerateDiff produces correct diff
- [ ] Prompt builder tests (3 tests minimum)
  - [ ] Prompt includes schema context
  - [ ] Prompt includes user preferences
  - [ ] Prompt formats correctly
- [ ] Parser tests (3 tests minimum)
  - [ ] Parses valid AI response
  - [ ] Handles malformed response
  - [ ] Extracts code changes correctly
- [ ] API endpoint tests (5 tests minimum)
  - [ ] POST /api/ai/modify-code returns 200 on success
  - [ ] POST /api/ai/modify-code returns 400 on bad request
  - [ ] POST /api/ai/chat works with context
  - [ ] GET /api/ai/versions returns all versions
  - [ ] POST /api/ai/revert reverts correctly

**Acceptance Criteria:**
- All tests pass
- Code coverage >90% for new code
- Tests use proper mocking
- Edge cases covered

**Files to Create:**
```
tests/TargCC.Core.Tests/Services/AI/
├── AICodeEditorServiceTests.cs
├── AICodeEditorPromptBuilderTests.cs
└── CodeModificationParserTests.cs

tests/TargCC.WebAPI.Tests/AI/
└── AIEndpointsTests.cs
```

**Target:** +16 tests minimum

---

### Task 14: Frontend Tests ⏱️ 2h
**Priority:** Medium
**Dependencies:** Frontend Tasks 6-12
**Status:** ⏸️ Not Started

**Deliverables:**
- [ ] AICodeEditor tests (5 tests minimum)
  - [ ] Renders Monaco Editor with code
  - [ ] Sends modification request on user input
  - [ ] Updates editor with AI response
  - [ ] Shows diff viewer on request
  - [ ] Handles undo/redo correctly
- [ ] Chat panel tests (3 tests minimum)
  - [ ] Displays messages correctly
  - [ ] Sends messages to API
  - [ ] Shows loading indicator
- [ ] Live preview tests (3 tests minimum)
  - [ ] Renders component from code
  - [ ] Updates on code change
  - [ ] Handles compilation errors gracefully
- [ ] Integration test (1 test)
  - [ ] End-to-end: modify code and preview

**Acceptance Criteria:**
- All tests pass
- React Testing Library best practices
- User interactions tested
- Edge cases covered

**Files to Create:**
```
src/TargCC.WebUI/src/__tests__/components/ai/
├── AICodeEditor.test.tsx
├── AIChatPanel.test.tsx
├── LivePreview.test.tsx
└── integration.test.tsx
```

**Target:** +12 tests minimum

---

## 📚 Documentation Task (1 hour)

### Task 15: User & Developer Documentation ⏱️ 1h (Optional for MVP)
**Priority:** Low
**Dependencies:** All tasks
**Status:** ⏸️ Not Started

**Deliverables:**
- [ ] User guide: How to use AI Code Editor
- [ ] Common prompts/examples
- [ ] Tips for best results
- [ ] Developer guide: Architecture overview
- [ ] API documentation
- [ ] Testing guide

**Files to Create:**
```
docs/
├── AI_CODE_EDITOR_USER_GUIDE.md
└── AI_CODE_EDITOR_DEV_GUIDE.md
```

---

## 🎯 Milestones

### Milestone 1: Backend Foundation (Day 1) - 5.5h
- ✅ All backend tasks complete (Tasks 1-5)
- ✅ API endpoints working
- ✅ Can modify code via API call

### Milestone 2: Frontend Core (Day 2) - 5h
- ✅ AICodeEditor component working (Tasks 6-7)
- ✅ Monaco Editor integrated
- ✅ Chat Panel functional (Task 8)

### Milestone 3: Preview & Polish (Day 3) - 4h
- ✅ Live Preview working (Task 9)
- ✅ Diff Viewer integrated (Task 10)
- ✅ Version History (Task 11)
- ✅ API integration complete (Task 12)

### Milestone 4: Testing & Launch (Day 4) - 4h
- ✅ Backend tests passing (Task 13)
- ✅ Frontend tests passing (Task 14)
- ✅ All acceptance criteria met
- ✅ Ready for production

---

## 📋 Dependencies

### External Packages to Install

**Frontend:**
```bash
npm install @monaco-editor/react
npm install react-diff-viewer-continued
```

**Backend:**
- ✅ All dependencies already in place (ClaudeAIService, etc.)

### Internal Dependencies

**Services:**
- ✅ `ClaudeAIService` (exists)
- ✅ `AIConfiguration` (exists)
- ✅ `ConversationContext` (exists)
- 🆕 `AICodeEditorService` (new)
- 🆕 `CodeVersionService` (new, optional for MVP)

---

## 🚀 Getting Started

### Pre-implementation Checklist
- [x] Specification reviewed and approved
- [x] Architecture validated
- [x] Tasks broken down and estimated
- [ ] Development environment ready
- [ ] API key configured (Claude API)
- [ ] Team assigned to tasks

### Ready to Start?
1. Review the spec: `docs/SPEC_AI_CODE_EDITOR.md`
2. Set up environment (see below)
3. Start with Task 1 (Backend foundation)
4. Follow milestone order

### Environment Setup
```bash
# Backend
cd src/TargCC.WebAPI
dotnet restore
dotnet run

# Frontend
cd src/TargCC.WebUI
npm install
npm install @monaco-editor/react react-diff-viewer-continued
npm run dev

# Configure AI
# Add Claude API key to appsettings.json or environment variables
```

---

## ⚠️ Risk Mitigation

### High Risk Items
1. **Live Preview Complexity**
   - **Risk:** Compiling TSX to React component is complex
   - **Mitigation:** Use iframe sandbox as fallback
   - **Task:** 9

2. **AI Response Quality**
   - **Risk:** AI may not understand complex requests
   - **Mitigation:** Detailed prompts + examples + context
   - **Task:** 2

3. **Performance**
   - **Risk:** AI calls may be slow (>3s)
   - **Mitigation:** Loading indicators + async processing
   - **Tasks:** All

### Medium Risk Items
1. **Monaco Editor Integration**
   - **Risk:** TypeScript config may conflict
   - **Mitigation:** Use isolated config
   - **Task:** 7

2. **Version Control**
   - **Risk:** Managing many versions may slow down
   - **Mitigation:** Limit to 10 recent versions
   - **Task:** 11

---

## 📊 Success Criteria

### Must Have (MVP)
- [x] Specification complete
- [ ] User can modify generated code via natural language
- [ ] AI understands schema context
- [ ] Live preview shows changes
- [ ] Code is validated before saving
- [ ] Basic undo/redo works

### Should Have
- [ ] Diff viewer shows changes
- [ ] Conversation history maintained
- [ ] Version control (10 versions)
- [ ] Suggested prompts

### Nice to Have (Post-MVP)
- [ ] Multi-file modifications
- [ ] AI explains changes
- [ ] Export conversation
- [ ] Custom prompt templates

---

## 📈 Progress Tracking

**Update this section as tasks complete:**

| Week | Tasks Complete | Hours Spent | Status |
|------|----------------|-------------|--------|
| Week 1 | 0/14 (0%) | 0h | 🔜 Not Started |
| Week 2 | TBD | TBD | ⏸️ Pending |

**Last Updated:** December 2, 2025
**Status:** Planning Complete, Ready to Begin Implementation

---

## 🔗 Related Documents

- **Specification:** [SPEC_AI_CODE_EDITOR.md](SPEC_AI_CODE_EDITOR.md)
- **Status:** [STATUS.md](current/STATUS.md)
- **Roadmap:** [PROJECT_CAPABILITIES_AND_ROADMAP.md](current/PROJECT_CAPABILITIES_AND_ROADMAP.md)
- **README:** [README.md](../README.md)

---

**Document Owner:** Development Team
**Reviewers:** Product, Engineering
**Approved:** Pending

---

*This task list is a living document. Update as you complete tasks and learn more.*
