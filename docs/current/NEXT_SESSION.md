# Next Session: Day 29 - Monaco Advanced Features

**Date:** Next Session  
**Phase:** 3C - Local Web UI  
**Day:** 29 of 45  
**Duration:** ~3-4 hours  
**Status:** Ready to Start

---

## 🎯 Day 29 Objectives

### Primary Goal
Add advanced features to Monaco Editor: theme toggle, language selector, download functionality, and integrate with the Generation Wizard.

### Specific Deliverables

1. **Theme Toggle** (60 min)
   - Dark/light theme switcher
   - Persist preference in localStorage
   - Smooth transitions
   - IconButton with mode icons

2. **Language Selector** (45 min)
   - Dropdown for language selection
   - Support: C#, TypeScript, JavaScript, SQL, JSON
   - Dynamic syntax highlighting
   - Current language indicator

3. **Download Functionality** (45 min)
   - Download single file button
   - Download all files as ZIP
   - Proper file extensions
   - Progress feedback

4. **Wizard Integration** (60 min)
   - Add code preview to GenerationWizard
   - Show in Step 4 (after generation)
   - Use actual selected tables
   - Modal or inline display

5. **Testing** (30 min)
   - 8-10 new tests
   - Theme switching tests
   - Download tests
   - Language selector tests

---

## 📋 Detailed Implementation Plan

### Part 1: Theme Toggle (60 minutes)

#### 1.1 Update CodePreview Component

```typescript
// src/components/code/CodePreview.tsx

import { useState, useEffect } from 'react';
import { IconButton, Tooltip } from '@mui/material';
import LightModeIcon from '@mui/icons-material/LightMode';
import DarkModeIcon from '@mui/icons-material/DarkMode';

interface CodePreviewProps {
  // ... existing props
  theme?: 'vs-dark' | 'light';
  onThemeChange?: (theme: 'vs-dark' | 'light') => void;
}

const CodePreview = ({ ... }: CodePreviewProps) => {
  const [editorTheme, setEditorTheme] = useState<'vs-dark' | 'light'>(
    () => (localStorage.getItem('monacoTheme') as 'vs-dark' | 'light') || 'vs-dark'
  );

  const toggleTheme = () => {
    const newTheme = editorTheme === 'vs-dark' ? 'light' : 'vs-dark';
    setEditorTheme(newTheme);
    localStorage.setItem('monacoTheme', newTheme);
    onThemeChange?.(newTheme);
  };

  return (
    <Paper>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
        <Typography variant="h6">{title}</Typography>
        
        <Tooltip title={`Switch to ${editorTheme === 'vs-dark' ? 'light' : 'dark'} theme`}>
          <IconButton onClick={toggleTheme} size="small">
            {editorTheme === 'vs-dark' ? <LightModeIcon /> : <DarkModeIcon />}
          </IconButton>
        </Tooltip>
      </Box>

      <Editor
        theme={editorTheme}
        // ... other props
      />
    </Paper>
  );
};
```

---

### Part 2: Language Selector (45 minutes)

#### 2.1 Add Language Selector to CodeViewer

```typescript
// src/components/code/CodeViewer.tsx

import { Select, MenuItem, FormControl, InputLabel } from '@mui/material';

const CodeViewer = ({ files }: CodeViewerProps) => {
  const [language, setLanguage] = useState('csharp');

  const languages = [
    { value: 'csharp', label: 'C#' },
    { value: 'typescript', label: 'TypeScript' },
    { value: 'javascript', label: 'JavaScript' },
    { value: 'sql', label: 'SQL' },
    { value: 'json', label: 'JSON' },
  ];

  return (
    <Box>
      <Box sx={{ display: 'flex', gap: 2, mb: 2, alignItems: 'center' }}>
        <Tabs value={activeTab} onChange={(_, val) => setActiveTab(val)}>
          {/* ... tabs */}
        </Tabs>

        <FormControl size="small" sx={{ minWidth: 120 }}>
          <InputLabel>Language</InputLabel>
          <Select
            value={language}
            label="Language"
            onChange={(e) => setLanguage(e.target.value)}
          >
            {languages.map((lang) => (
              <MenuItem key={lang.value} value={lang.value}>
                {lang.label}
              </MenuItem>
            ))}
          </Select>
        </FormControl>

        <Tooltip title="Copy">
          <IconButton onClick={handleCopy}>
            {/* ... */}
          </IconButton>
        </Tooltip>
      </Box>

      <CodePreview
        code={files[activeTab].code}
        language={language}
        // ... other props
      />
    </Box>
  );
};
```

---

### Part 3: Download Functionality (45 minutes)

#### 3.1 Install JSZip

```bash
npm install jszip
npm install @types/jszip --save-dev
```

#### 3.2 Add Download Functions

```typescript
// src/utils/downloadCode.ts

import JSZip from 'jszip';

export const downloadFile = (filename: string, content: string) => {
  const blob = new Blob([content], { type: 'text/plain;charset=utf-8' });
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
};

export const downloadAllAsZip = async (
  files: Array<{ name: string; code: string }>,
  zipName: string = 'generated-code.zip'
) => {
  const zip = new JSZip();
  
  files.forEach(file => {
    zip.file(file.name, file.code);
  });

  const blob = await zip.generateAsync({ type: 'blob' });
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = zipName;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
};
```

#### 3.3 Add Download Buttons to CodeViewer

```typescript
import DownloadIcon from '@mui/icons-material/Download';
import FolderZipIcon from '@mui/icons-material/FolderZip';
import { downloadFile, downloadAllAsZip } from '../../utils/downloadCode';

const CodeViewer = ({ files }: CodeViewerProps) => {
  const [downloading, setDownloading] = useState(false);

  const handleDownloadCurrent = () => {
    downloadFile(files[activeTab].name, files[activeTab].code);
  };

  const handleDownloadAll = async () => {
    setDownloading(true);
    try {
      await downloadAllAsZip(files, 'generated-code.zip');
    } finally {
      setDownloading(false);
    }
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', gap: 1, mb: 2, alignItems: 'center' }}>
        {/* ... existing buttons */}

        <Tooltip title="Download current file">
          <IconButton onClick={handleDownloadCurrent}>
            <DownloadIcon />
          </IconButton>
        </Tooltip>

        <Tooltip title="Download all as ZIP">
          <IconButton onClick={handleDownloadAll} disabled={downloading}>
            <FolderZipIcon />
          </IconButton>
        </Tooltip>
      </Box>
    </Box>
  );
};
```

---

### Part 4: Wizard Integration (60 minutes)

#### 4.1 Update GenerationWizard

```typescript
// src/components/wizard/GenerationWizard.tsx

import CodeViewer from '../code/CodeViewer';
import { mockCodeFiles } from '../../utils/mockCode';

const GenerationWizard = () => {
  // ... existing state

  const generatedFiles = selectedTables.length > 0
    ? mockCodeFiles(selectedTables[0])
    : [];

  // In Step 4 (GenerationProgress)
  const renderGenerationProgress = () => (
    <Box>
      {/* ... existing progress UI */}

      {progress === 100 && (
        <Box sx={{ mt: 4 }}>
          <Typography variant="h6" gutterBottom>
            Generated Code Preview
          </Typography>
          <CodeViewer files={generatedFiles} />
        </Box>
      )}
    </Box>
  );
};
```

---

### Part 5: Testing (30 minutes)

#### 5.1 Theme Toggle Tests

```typescript
// src/__tests__/code/CodePreview.test.tsx

describe('Theme Toggle', () => {
  it('toggles between dark and light themes', () => {
    // ...
  });

  it('persists theme to localStorage', () => {
    // ...
  });

  it('loads saved theme on mount', () => {
    // ...
  });
});
```

#### 5.2 Language Selector Tests

```typescript
// src/__tests__/code/CodeViewer.test.tsx

describe('Language Selector', () => {
  it('renders language dropdown', () => {
    // ...
  });

  it('changes language when selected', () => {
    // ...
  });

  it('updates Monaco editor language', () => {
    // ...
  });
});
```

#### 5.3 Download Tests

```typescript
describe('Download Functionality', () => {
  it('downloads single file', () => {
    // ...
  });

  it('downloads all files as ZIP', async () => {
    // ...
  });

  it('shows loading state during ZIP', () => {
    // ...
  });
});
```

---

## 📝 Files to Create/Modify

### New Files
```
src/utils/
└── downloadCode.ts (50 lines)
```

### Modified Files
```
src/components/code/
├── CodePreview.tsx (+30 lines, theme toggle)
└── CodeViewer.tsx (+60 lines, language + downloads)

src/components/wizard/
└── GenerationWizard.tsx (+20 lines, code preview)
```

### New Test Files
```
src/__tests__/utils/
└── downloadCode.test.ts (40 lines)
```

---

## ✅ Success Criteria

### Functionality
- [ ] Theme toggle works (dark ↔ light)
- [ ] Theme persists in localStorage
- [ ] Language selector works
- [ ] Monaco updates language dynamically
- [ ] Download single file works
- [ ] Download all as ZIP works
- [ ] Wizard shows code preview
- [ ] Integration smooth

### Testing
- [ ] 8-10 new tests written
- [ ] Theme tests pass
- [ ] Download tests pass
- [ ] Language tests pass
- [ ] Build successful (dev)

### Code Quality
- [ ] TypeScript compliant
- [ ] Components clean
- [ ] Proper error handling
- [ ] No console warnings

### Documentation
- [ ] STATUS.md updated
- [ ] HANDOFF.md for Day 30
- [ ] Phase3_Checklist.md updated

---

## 🚀 Getting Started

### 1. Install Dependencies (5 min)
```bash
cd C:\Disk1\TargCC-Core-V2\src\TargCC.WebUI
npm install jszip
npm install @types/jszip --save-dev
```

### 2. Development Order
1. Theme toggle in CodePreview
2. Test theme switching
3. Language selector in CodeViewer
4. Download utilities
5. Download buttons
6. Wizard integration
7. Write tests
8. Update docs

---

## 💡 Tips for Success

### Theme Switching
- Monaco supports 3 built-in themes: 'vs-dark', 'light', 'hc-black'
- Transition is instant (no animation needed)
- Store preference in localStorage
- Default to 'vs-dark'

### Language Support
- Monaco supports 60+ languages
- Use exact IDs: 'csharp', 'typescript', 'javascript', 'sql', 'json'
- Language changes are instant
- Syntax highlighting updates automatically

### Downloads
- Use Blob API for single files
- JSZip for multiple files
- Always revoke object URLs
- Show loading state for ZIP (can be slow)

### Wizard Integration
- Keep code preview optional
- Show only after generation complete
- Use selected table names
- Consider modal for full-screen view

---

## 📞 Quick Commands

```bash
# Start dev server
npm run dev

# Run tests
npm test

# Check package
npm list jszip

# Demo page
# http://localhost:5173/code-demo
```

---

**Ready to Start:** ✅  
**Estimated Duration:** 3-4 hours  
**Expected Output:** Full-featured Monaco Editor  
**Next Day:** Day 30 - Progress Display & Polish

---

**Created:** 01/12/2025  
**Status:** Ready for Day 29! 🚀
