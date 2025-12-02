# Build Verification Checklist

Use this checklist to verify the Phase 3F implementation is ready for testing.

## ✅ File Structure Check

Run these commands to verify all files exist:

```bash
# Backend files
ls src/TargCC.Core.Services/AI/AICodeEditorService.cs
ls src/TargCC.Core.Services/AI/IAICodeEditorService.cs
ls src/TargCC.Core.Services/AI/Models/CodeModificationResult.cs
ls src/TargCC.Core.Services/AI/Models/ValidationResult.cs
ls src/TargCC.Core.Services/AI/Models/CodeChange.cs
ls src/TargCC.Core.Services/AI/Models/ModificationContext.cs

# API files
ls src/TargCC.WebAPI/Models/Requests/CodeModificationRequest.cs
ls src/TargCC.WebAPI/Models/Responses/CodeModificationResponse.cs
ls src/TargCC.WebAPI/Models/Requests/CodeValidationRequest.cs
ls src/TargCC.WebAPI/Models/Requests/CodeDiffRequest.cs

# Frontend files
ls src/TargCC.WebUI/src/components/code/AICodeEditor.tsx
ls src/TargCC.WebUI/src/components/code/AIChatPanel.tsx
ls src/TargCC.WebUI/src/components/code/CodeDiffViewer.tsx
ls src/TargCC.WebUI/src/types/aiCodeEditor.ts
ls src/TargCC.WebUI/src/api/aiCodeEditorApi.ts
ls src/TargCC.WebUI/src/pages/AICodeEditorDemo.tsx
ls src/TargCC.WebUI/src/utils/mockReactCode.ts
```

Expected: All files should exist (no "No such file" errors)

## ✅ Git Status Check

```bash
git status
git log --oneline -5
```

Expected:
- Branch: `claude/audit-project-cleanup-01ERcm9g3T2u5Bz6DwcDQKYH`
- Clean working tree (all committed)
- Recent commits include Phase 3F implementation

## ✅ Backend Build Check

```bash
cd src/TargCC.WebAPI
dotnet restore
dotnet build --no-restore
```

Expected:
- No compilation errors
- Build succeeded message
- 0 Error(s), 0 Warning(s)

Common issues:
- Missing using statements → Check imports in AICodeEditorService.cs
- Missing package references → Run `dotnet restore`
- Missing dependencies → Check .csproj files

## ✅ Frontend Build Check

```bash
cd src/TargCC.WebUI
npm install
npm run build
```

Expected:
- No TypeScript errors
- Build completed successfully
- dist/ folder created

Common issues:
- Type errors → Check imports in .tsx files
- Missing dependencies → Run `npm install`
- Module not found → Check file paths

## ✅ TypeScript Check

```bash
cd src/TargCC.WebUI
npx tsc --noEmit
```

Expected:
- No errors
- Process exits with code 0

## ✅ Linting Check

```bash
cd src/TargCC.WebUI
npm run lint
```

Expected:
- No critical errors
- Warnings are acceptable

## ✅ API Endpoint Check

After starting the backend:

```bash
curl http://localhost:5000/api/health
```

Expected:
```json
{
  "status": "healthy",
  "timestamp": "2025-12-02T...",
  "version": "2.0.0-beta.1"
}
```

## ✅ Frontend Routing Check

After starting the frontend, visit these URLs:

1. http://localhost:5173/ - Dashboard (should load)
2. http://localhost:5173/ai-code-editor - AI Code Editor Demo (should load)

Expected:
- No 404 errors
- Pages render correctly
- No console errors

## ✅ Component Rendering Check

On the AI Code Editor page, verify:

- [ ] Monaco Editor loads (shows code)
- [ ] Chat panel appears on the right
- [ ] Tabs show "Editor" and "Diff (0)"
- [ ] Example selector shows "Simple Form" and "Complex Form"
- [ ] Example prompts display as chips
- [ ] Undo/Redo buttons are visible
- [ ] No React errors in console

## ✅ Mock Data Check

Verify mock data loads:

```bash
# Check mock code utility
cat src/TargCC.WebUI/src/utils/mockReactCode.ts | grep "export"
```

Expected:
- generateMockReactComponent function exists
- generateSimpleReactComponent function exists

## ✅ API Integration Check (Optional - Requires API Key)

If you have a Claude API key configured:

```bash
curl -X POST http://localhost:5000/api/ai/code/modify \
  -H "Content-Type: application/json" \
  -d '{
    "originalCode": "const x = 1;",
    "instruction": "change x to 2",
    "tableName": "Test",
    "schema": "dbo"
  }'
```

Expected:
- HTTP 200 OK
- JSON response with success: true
- modifiedCode field present

## ✅ File Permissions Check

```bash
# Check files are readable
ls -la src/TargCC.Core.Services/AI/
ls -la src/TargCC.WebUI/src/components/code/
```

Expected:
- All files have read permissions
- No permission denied errors

## 🚀 Ready to Test Criteria

The implementation is ready for testing when:

- ✅ All files exist in correct locations
- ✅ Backend builds without errors
- ✅ Frontend builds without errors
- ✅ No TypeScript errors
- ✅ Backend starts and health check passes
- ✅ Frontend starts and pages load
- ✅ Monaco Editor renders correctly
- ✅ Chat panel is functional
- ✅ No console errors in browser

## 🔧 Quick Fixes for Common Issues

### Issue: "Cannot find module"

```bash
cd src/TargCC.WebUI
rm -rf node_modules package-lock.json
npm install
```

### Issue: "dotnet command not found"

Install .NET 9 SDK from https://dot.net

### Issue: "Port 5000 already in use"

```bash
# Kill process on port 5000
lsof -ti:5000 | xargs kill -9

# Or change port in launchSettings.json
```

### Issue: TypeScript errors

```bash
cd src/TargCC.WebUI
npm install --save-dev @types/react @types/node
```

### Issue: Build cache problems

```bash
# Backend
cd src/TargCC.WebAPI
dotnet clean
dotnet build

# Frontend
cd src/TargCC.WebUI
rm -rf node_modules dist .vite
npm install
```

## 📝 Pre-Merge Checklist

Before merging to main:

- [ ] All checks above pass
- [ ] Code is committed and pushed
- [ ] Documentation is complete
- [ ] No TODO comments in production code
- [ ] No hardcoded API keys
- [ ] No console.log in production code
- [ ] Git history is clean
- [ ] Branch is up to date with main

## 📊 Expected Metrics

- **Backend Lines**: ~1,300 lines added
- **Frontend Lines**: ~900 lines added
- **Total Files**: 16 new files
- **Build Time**: 30-60 seconds
- **First Paint**: < 2 seconds
- **Monaco Load**: < 3 seconds

## ✅ Verification Complete

If all checks pass, you're ready to:

1. Start testing with the Quick Start Guide
2. Test AI modifications with your API key
3. Integrate with existing workflows
4. Deploy to staging environment

Congratulations! The Phase 3F implementation is ready! 🎉
