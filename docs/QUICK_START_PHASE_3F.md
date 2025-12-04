# Quick Start Guide - Phase 3F AI Code Editor

This guide will help you quickly set up and test the new AI Code Editor feature.

## Prerequisites

- .NET 9 SDK
- Node.js 18+ and npm
- Claude AI API Key from Anthropic (https://console.anthropic.com)
- SQL Server (for full functionality)

## Step 1: Clone and Checkout Branch

```bash
git clone <repository-url>
cd TargCC-Core-V2
git checkout claude/audit-project-cleanup-01ERcm9g3T2u5Bz6DwcDQKYH
```

## Step 2: Configure Claude AI API Key

### Option A: Using appsettings.json (Recommended for Development)

Edit `src/TargCC.WebAPI/appsettings.json`:

```json
{
  "AI": {
    "ApiKey": "sk-ant-api03-YOUR_API_KEY_HERE",
    "Model": "claude-sonnet-4-20250514",
    "MaxTokens": 4000,
    "CacheEnabled": true,
    "CacheDirectory": "./cache",
    "RateLimitPerMinute": 60
  }
}
```

### Option B: Using User Secrets (Recommended for Production)

```bash
cd src/TargCC.WebAPI
dotnet user-secrets init
dotnet user-secrets set "AI:ApiKey" "sk-ant-api03-YOUR_API_KEY_HERE"
```

## Step 3: Start Backend

```bash
cd src/TargCC.WebAPI
dotnet restore
dotnet build
dotnet run
```

The API will start on `http://localhost:5000`

**Verify Backend:**
- Open `http://localhost:5000/swagger` in your browser
- You should see the Swagger UI with API endpoints
- Look for the "AI Code Editor" section with 3 endpoints

## Step 4: Start Frontend

Open a new terminal:

```bash
cd src/TargCC.WebUI
npm install
npm run dev
```

The UI will start on `http://localhost:5173`

**Verify Frontend:**
- Open `http://localhost:5173` in your browser
- You should see the TargCC dashboard
- Click "AI Code Editor" in the sidebar

## Step 5: Test AI Code Editor

### Basic Test (Without API Key)

If you haven't configured an API key yet, you can still explore the UI:

1. Navigate to **AI Code Editor** from the sidebar
2. Choose "Simple Form" or "Complex Form"
3. View the code in Monaco Editor
4. See the chat panel and diff viewer interface
5. Try typing instructions (they won't work without API key, but you can see the UI)

### Full Test (With API Key)

1. **Navigate** to AI Code Editor page
2. **Select** "Simple Form" example
3. **Type** in chat: `Make the button blue`
4. **Press Enter** and wait 3-10 seconds
5. **Watch** the code update automatically
6. **Click** the "Diff" tab to see changes
7. **Try Undo/Redo** buttons

### Example Instructions to Try

Easy modifications:
- `Make the button blue`
- `Change button text to "Submit"`
- `Make text fields smaller`
- `Add margin to the form`

Medium complexity:
- `Change grid to 2 columns`
- `Add a phone field after email`
- `Make the button on the left side`
- `Add email validation`

Complex modifications:
- `Add a loading spinner when submitting`
- `Add error message display`
- `Change the layout to vertical`
- `Add a date picker field`

## Step 6: Verify Everything Works

### Check Backend Logs

In the backend terminal, you should see logs like:

```
info: TargCC.Core.Services.AI.AICodeEditorService[0]
      Starting code modification for table ContactForm with instruction: Make the button blue
info: TargCC.Core.Services.AI.AICodeEditorService[0]
      Code modification completed successfully for table ContactForm: 3 changes
```

### Check Frontend

1. **Code Editor**: Should show modified code
2. **Diff Viewer**: Should show green/red/blue changes
3. **Chat Panel**: Should show conversation history
4. **Statistics**: Should show "X changes" chip
5. **Undo/Redo**: Should work correctly

## Troubleshooting

### Backend Issues

**Error: "Failed to modify code: Unauthorized"**
- Check your API key is correct
- Verify it starts with `sk-ant-api03-`
- Check you have credits in your Anthropic account

**Error: "dotnet command not found"**
- Install .NET 9 SDK from https://dot.net

**Error: "Connection refused"**
- Make sure SQL Server is running (only needed for full features)
- Update connection string in appsettings.json

### Frontend Issues

**Error: "npm: command not found"**
- Install Node.js from https://nodejs.org

**Blank page or errors in console**
- Check backend is running on port 5000
- Check CORS is enabled (it should be by default)
- Check browser console for errors

**TypeScript errors during npm install**
- Run `npm install --legacy-peer-deps`
- Or delete `node_modules` and `package-lock.json` then run `npm install` again

**Monaco Editor not loading**
- Check internet connection (Monaco loads from CDN)
- Wait a few seconds for it to initialize
- Check browser console for errors

## API Endpoints

### POST /api/ai/code/modify

Modify code using natural language:

```bash
curl -X POST http://localhost:5000/api/ai/code/modify \
  -H "Content-Type: application/json" \
  -d '{
    "originalCode": "export const MyComponent = () => { return <Button>Save</Button>; };",
    "instruction": "Make the button blue",
    "tableName": "TestTable",
    "schema": "dbo"
  }'
```

### POST /api/ai/code/validate

Validate modified code:

```bash
curl -X POST http://localhost:5000/api/ai/code/validate \
  -H "Content-Type: application/json" \
  -d '{
    "originalCode": "const x = 1;",
    "modifiedCode": "const x = 2"
  }'
```

### POST /api/ai/code/diff

Generate diff:

```bash
curl -X POST http://localhost:5000/api/ai/code/diff \
  -H "Content-Type: application/json" \
  -d '{
    "originalCode": "const x = 1;",
    "modifiedCode": "const x = 2;"
  }'
```

## Project Structure

```
TargCC-Core-V2/
├── src/
│   ├── TargCC.Core.Services/
│   │   └── AI/
│   │       ├── AICodeEditorService.cs      (Main service)
│   │       ├── IAICodeEditorService.cs     (Interface)
│   │       └── Models/                      (Data models)
│   ├── TargCC.WebAPI/
│   │   ├── Program.cs                       (API endpoints)
│   │   ├── Models/Requests/                 (Request DTOs)
│   │   └── Models/Responses/                (Response DTOs)
│   └── TargCC.WebUI/
│       └── src/
│           ├── components/code/
│           │   ├── AICodeEditor.tsx        (Main component)
│           │   ├── AIChatPanel.tsx         (Chat interface)
│           │   └── CodeDiffViewer.tsx      (Diff viewer)
│           ├── pages/
│           │   └── AICodeEditorDemo.tsx    (Demo page)
│           ├── api/
│           │   └── aiCodeEditorApi.ts      (API client)
│           └── types/
│               └── aiCodeEditor.ts          (TypeScript types)
```

## Performance Tips

1. **Rate Limits**: Default is 60 requests/minute. Adjust in appsettings.json if needed.
2. **Caching**: Enable AI response caching for faster repeated operations.
3. **Timeouts**: Default is 30 seconds. Increase for complex modifications.
4. **Token Limits**: Default is 4000 tokens. Increase for larger code files.

## Cost Estimation

Claude AI costs (as of Dec 2024):
- Input: $3 per million tokens
- Output: $15 per million tokens

Typical costs per modification:
- Simple (50 lines): ~$0.001
- Medium (200 lines): ~$0.005
- Complex (500 lines): ~$0.015

Monthly estimates (100 modifications/day):
- Simple: ~$3/month
- Medium: ~$15/month
- Complex: ~$45/month

## Next Steps

1. **Test with your own code**: Replace mock code with your generated components
2. **Customize prompts**: Modify project conventions in AICodeEditorService.cs
3. **Add more examples**: Create new mock components in mockReactCode.ts
4. **Integrate with generation**: Connect AI editor to code generation workflow
5. **Add persistence**: Save conversation history to database
6. **Add tests**: Write unit and integration tests

## Support

- **Documentation**: See `docs/Phase3F_IMPLEMENTATION_SUMMARY.md`
- **Specification**: See `docs/SPEC_AI_CODE_EDITOR.md`
- **Tasks**: See `docs/Phase3F_AI_CODE_EDITOR_TASKS.md`
- **GitHub Issues**: Report bugs and feature requests

## Known Limitations

1. **Single File**: Currently only modifies one file at a time
2. **No Persistence**: Conversation history is lost on page refresh
3. **No Multi-user**: One session per user
4. **English Only**: AI works best with English instructions
5. **Token Limit**: Large files (>1000 lines) may exceed token limits

## Security Notes

⚠️ **Important Security Considerations:**

1. **API Key**: Never commit API keys to git
2. **User Input**: All user instructions are sent to Claude AI
3. **Code Privacy**: Generated code is sent to Anthropic servers
4. **Rate Limiting**: Implement additional rate limiting for production
5. **Authentication**: Add authentication before deploying to production

## Success Criteria

You know it's working when:
- ✅ Backend starts without errors
- ✅ Frontend loads without errors
- ✅ You can navigate to AI Code Editor page
- ✅ Monaco Editor displays code
- ✅ Chat panel accepts input
- ✅ AI modifies code based on instructions
- ✅ Diff viewer shows changes
- ✅ Undo/Redo works
- ✅ Validation shows errors/warnings

Congratulations! You now have a working AI-powered code editor! 🎉
