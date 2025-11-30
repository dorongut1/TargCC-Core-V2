# Next Session: Day 28 - Monaco Editor Integration (Part 1)

**Date:** Next Session  
**Phase:** 3C - Local Web UI  
**Day:** 28 of 45  
**Duration:** ~3-4 hours  
**Status:** Ready to Start

---

## 🎯 Day 28 Objectives

### Primary Goal
Integrate Monaco Editor (the VS Code editor) into the React app for code preview functionality. This will allow users to see generated code with syntax highlighting, line numbers, and a professional editor experience.

### Specific Deliverables

1. **Monaco Editor Setup**
   - Install @monaco-editor/react package
   - Install TypeScript types
   - Verify installation works

2. **CodePreview Component**
   - Create reusable editor component
   - Configure for C# syntax highlighting
   - Add loading state
   - Apply dark theme
   - Make read-only

3. **CodeViewer Component**
   - Multi-file tabs
   - File switching
   - Copy to clipboard button
   - Mock generated code display

4. **Testing**
   - 8-10 new tests
   - Monaco loading tests
   - Code display tests
   - File tab switching tests

---

## 📋 Detailed Implementation Plan

### Part 1: Package Installation (15 minutes)

#### 1.1 Install Packages
```bash
cd C:\Disk1\TargCC-Core-V2\src\TargCC.WebUI

npm install @monaco-editor/react
npm install @types/monaco-editor --save-dev
```

#### 1.2 Verify Installation
```bash
npm list @monaco-editor/react
# Should show version ~4.6.0 or higher
```

---

### Part 2: CodePreview Component (60 minutes)

#### 2.1 Create CodePreview.tsx

```typescript
// src/components/code/CodePreview.tsx
import { useState } from 'react';
import Editor from '@monaco-editor/react';
import { Box, Paper, Typography, CircularProgress } from '@mui/material';

interface CodePreviewProps {
  code: string;
  language?: string;
  height?: string;
  readOnly?: boolean;
  title?: string;
}

const CodePreview = ({ 
  code, 
  language = 'csharp', 
  height = '400px',
  readOnly = true,
  title = 'Code Preview'
}: CodePreviewProps) => {
  const [isLoading, setIsLoading] = useState(true);

  return (
    <Paper sx={{ p: 2 }} elevation={2}>
      <Typography variant="h6" gutterBottom>
        {title}
      </Typography>
      
      {isLoading && (
        <Box 
          sx={{ 
            display: 'flex', 
            justifyContent: 'center', 
            alignItems: 'center',
            height,
            bgcolor: 'grey.900'
          }}
        >
          <CircularProgress />
        </Box>
      )}
      
      <Box sx={{ display: isLoading ? 'none' : 'block' }}>
        <Editor
          height={height}
          language={language}
          value={code}
          theme="vs-dark"
          options={{
            readOnly,
            minimap: { enabled: false },
            scrollBeyondLastLine: false,
            fontSize: 14,
            lineNumbers: 'on',
            folding: true,
            automaticLayout: true,
            wordWrap: 'on'
          }}
          onMount={() => setIsLoading(false)}
        />
      </Box>
    </Paper>
  );
};

export default CodePreview;
```

---

### Part 3: CodeViewer with Tabs (60 minutes)

#### 3.1 Create CodeViewer.tsx

```typescript
// src/components/code/CodeViewer.tsx
import { useState } from 'react';
import {
  Box,
  Tabs,
  Tab,
  IconButton,
  Tooltip,
  Alert
} from '@mui/material';
import ContentCopyIcon from '@mui/icons-material/ContentCopy';
import CheckIcon from '@mui/icons-material/Check';
import CodePreview from './CodePreview';

export interface CodeFile {
  name: string;
  code: string;
  language: string;
}

interface CodeViewerProps {
  files: CodeFile[];
}

const CodeViewer = ({ files }: CodeViewerProps) => {
  const [activeTab, setActiveTab] = useState(0);
  const [copied, setCopied] = useState(false);

  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(files[activeTab].code);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch (err) {
      console.error('Failed to copy:', err);
    }
  };

  if (files.length === 0) {
    return (
      <Alert severity="info">
        No code files to display
      </Alert>
    );
  }

  return (
    <Box>
      <Box 
        sx={{ 
          borderBottom: 1, 
          borderColor: 'divider', 
          display: 'flex', 
          justifyContent: 'space-between',
          alignItems: 'center'
        }}
      >
        <Tabs 
          value={activeTab} 
          onChange={(_, val) => setActiveTab(val)}
          variant="scrollable"
          scrollButtons="auto"
        >
          {files.map((file, index) => (
            <Tab key={index} label={file.name} />
          ))}
        </Tabs>
        
        <Tooltip title={copied ? 'Copied!' : 'Copy to clipboard'}>
          <IconButton onClick={handleCopy} color={copied ? 'success' : 'default'}>
            {copied ? <CheckIcon /> : <ContentCopyIcon />}
          </IconButton>
        </Tooltip>
      </Box>

      <Box sx={{ mt: 2 }}>
        <CodePreview
          code={files[activeTab].code}
          language={files[activeTab].language}
          title={`${files[activeTab].name}`}
          height="500px"
        />
      </Box>
    </Box>
  );
};

export default CodeViewer;
```

---

### Part 4: Mock Generated Code (30 minutes)

#### 4.1 Create mockCode.ts

```typescript
// src/utils/mockCode.ts

export const generateMockCode = (tableName: string) => ({
  entity: `namespace TargCC.Domain.Entities
{
    /// <summary>
    /// ${tableName} entity
    /// Generated by TargCC Core V2
    /// </summary>
    public class ${tableName}
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ${tableName} name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}`,

  repository: `namespace TargCC.Infrastructure.Repositories
{
    /// <summary>
    /// Repository interface for ${tableName}
    /// </summary>
    public interface I${tableName}Repository
    {
        /// <summary>
        /// Get ${tableName} by ID
        /// </summary>
        Task<${tableName}?> GetByIdAsync(int id);

        /// <summary>
        /// Get all ${tableName}s
        /// </summary>
        Task<IEnumerable<${tableName}>> GetAllAsync();

        /// <summary>
        /// Add new ${tableName}
        /// </summary>
        Task<int> AddAsync(${tableName} entity);

        /// <summary>
        /// Update existing ${tableName}
        /// </summary>
        Task UpdateAsync(${tableName} entity);

        /// <summary>
        /// Delete ${tableName} by ID
        /// </summary>
        Task DeleteAsync(int id);
    }
}`,

  handler: `namespace TargCC.Application.${tableName}s.Queries
{
    /// <summary>
    /// Query to get ${tableName} by ID
    /// </summary>
    public record Get${tableName}Query(int Id) : IRequest<${tableName}>;

    /// <summary>
    /// Handler for Get${tableName}Query
    /// </summary>
    public class Get${tableName}QueryHandler 
        : IRequestHandler<Get${tableName}Query, ${tableName}>
    {
        private readonly I${tableName}Repository _repository;

        public Get${tableName}QueryHandler(I${tableName}Repository repository)
        {
            _repository = repository;
        }

        public async Task<${tableName}> Handle(
            Get${tableName}Query request, 
            CancellationToken cancellationToken)
        {
            return await _repository.GetByIdAsync(request.Id);
        }
    }
}`,

  controller: `namespace TargCC.API.Controllers
{
    /// <summary>
    /// ${tableName} API controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ${tableName}Controller : ControllerBase
    {
        private readonly IMediator _mediator;

        public ${tableName}Controller(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get ${tableName} by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<${tableName}>> Get(int id)
        {
            var query = new Get${tableName}Query(id);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get all ${tableName}s
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<${tableName}>>> GetAll()
        {
            var query = new GetAll${tableName}sQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}`
});

export const mockCodeFiles = (tableName: string) => {
  const code = generateMockCode(tableName);
  
  return [
    { name: `${tableName}.cs`, code: code.entity, language: 'csharp' },
    { name: `I${tableName}Repository.cs`, code: code.repository, language: 'csharp' },
    { name: `Get${tableName}QueryHandler.cs`, code: code.handler, language: 'csharp' },
    { name: `${tableName}Controller.cs`, code: code.controller, language: 'csharp' }
  ];
};
```

---

### Part 5: Testing CodePreview (30 minutes)

#### 5.1 Create CodePreview.test.tsx

```typescript
// src/__tests__/code/CodePreview.test.tsx
import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import CodePreview from '../../components/code/CodePreview';

describe('CodePreview', () => {
  const sampleCode = 'public class Customer { }';

  it('renders with title', () => {
    render(<CodePreview code={sampleCode} title="Test Code" />);
    expect(screen.getByText('Test Code')).toBeInTheDocument();
  });

  it('shows loading spinner initially', () => {
    render(<CodePreview code={sampleCode} />);
    expect(screen.getByRole('progressbar')).toBeInTheDocument();
  });

  it('uses csharp language by default', () => {
    render(<CodePreview code={sampleCode} />);
    // Monaco editor should be configured for csharp
  });

  it('applies custom height', () => {
    render(<CodePreview code={sampleCode} height="600px" />);
    // Editor should use 600px height
  });
});
```

---

### Part 6: Testing CodeViewer (30 minutes)

#### 6.1 Create CodeViewer.test.tsx

```typescript
// src/__tests__/code/CodeViewer.test.tsx
import { describe, it, expect } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import CodeViewer from '../../components/code/CodeViewer';

describe('CodeViewer', () => {
  const mockFiles = [
    { name: 'Entity.cs', code: 'public class Entity {}', language: 'csharp' },
    { name: 'Repository.cs', code: 'public class Repo {}', language: 'csharp' }
  ];

  it('renders file tabs', () => {
    render(<CodeViewer files={mockFiles} />);
    expect(screen.getByText('Entity.cs')).toBeInTheDocument();
    expect(screen.getByText('Repository.cs')).toBeInTheDocument();
  });

  it('shows first file by default', () => {
    render(<CodeViewer files={mockFiles} />);
    expect(screen.getByText('Entity.cs')).toBeInTheDocument();
  });

  it('switches between files when tab clicked', () => {
    render(<CodeViewer files={mockFiles} />);
    const repoTab = screen.getByText('Repository.cs');
    fireEvent.click(repoTab);
    // Should now show Repository.cs content
  });

  it('shows copy button', () => {
    render(<CodeViewer files={mockFiles} />);
    expect(screen.getByLabelText(/copy to clipboard/i)).toBeInTheDocument();
  });

  it('shows info alert when no files', () => {
    render(<CodeViewer files={[]} />);
    expect(screen.getByText('No code files to display')).toBeInTheDocument();
  });
});
```

---

## 📁 Files to Create

### New Component Files
```
src/components/code/
├── CodePreview.tsx          (80 lines)
└── CodeViewer.tsx           (100 lines)
```

### New Utility Files
```
src/utils/
└── mockCode.ts              (150 lines)
```

### New Test Files
```
src/__tests__/code/
├── CodePreview.test.tsx     (60 lines)
└── CodeViewer.test.tsx      (80 lines)
```

---

## ✅ Success Criteria

### Functionality
- [ ] Monaco Editor package installed
- [ ] CodePreview component renders
- [ ] Loading spinner shows during Monaco init
- [ ] C# syntax highlighting works
- [ ] Dark theme applied
- [ ] CodeViewer tabs work
- [ ] File switching functional
- [ ] Copy to clipboard works

### Testing
- [ ] 8-10 new tests written
- [ ] All tests have correct logic
- [ ] Monaco loading tested
- [ ] File switching tested

### Code Quality
- [ ] TypeScript strict mode compliant
- [ ] No build errors or warnings
- [ ] Components under 150 lines each
- [ ] Proper prop types and interfaces
- [ ] Clean, readable code

### Documentation
- [ ] Updated Phase3_Checklist.md
- [ ] Updated STATUS.md
- [ ] Updated HANDOFF.md for Day 29
- [ ] Code comments where needed

---

## 🚀 Getting Started

### 1. Environment Setup (5 min)
```bash
cd C:\Disk1\TargCC-Core-V2\src\TargCC.WebUI

# Verify app runs
npm run dev
# Open http://localhost:5174
```

### 2. Install Packages (5 min)
```bash
npm install @monaco-editor/react
npm install @types/monaco-editor --save-dev

# Verify installation
npm list @monaco-editor/react
```

### 3. Create Directory Structure (2 min)
```bash
# Create code directory
mkdir src\components\code
mkdir src\__tests__\code
mkdir src\utils

# Verify structure
dir src\components
dir src\__tests__
```

### 4. Implementation Order
1. Install Monaco packages
2. Create CodePreview component
3. Test CodePreview in browser
4. Create mockCode utility
5. Create CodeViewer component
6. Test CodeViewer in browser
7. Write tests
8. Update documentation

---

## 💡 Tips for Success

### Monaco Editor Best Practices
1. **Loading State:** Always show loading spinner
2. **Performance:** Monaco takes 1-2 seconds to load
3. **Theme:** Use 'vs-dark' for consistency
4. **Options:** Disable minimap for cleaner look
5. **Height:** Use fixed height (not 100%)

### Common Pitfalls to Avoid
- Don't forget loading state
- Test Monaco initialization
- Handle empty code gracefully
- Use appropriate language names
- Test copy functionality

### Testing Considerations
- Monaco may need mocking in tests
- Test component structure, not Monaco internals
- Focus on user interactions
- Test error states

---

## 📞 Quick Commands

```bash
# Development
npm run dev              # Start dev server
npm test                 # Run tests
npm run build            # Build for production

# Check Monaco
npm list @monaco-editor/react
```

---

**Ready to Start:** ✅  
**Estimated Duration:** 3-4 hours  
**Expected Output:** Working code preview with Monaco Editor  
**Next Day:** Day 29 - Monaco Editor Advanced Features

---

**Created:** 30/11/2025  
**Status:** Ready for Day 28! 🚀
