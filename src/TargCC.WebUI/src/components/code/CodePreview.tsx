import { useState, useEffect } from 'react';
import Editor from '@monaco-editor/react';
import { Box, Paper, Typography, CircularProgress, IconButton, Tooltip } from '@mui/material';
import LightModeIcon from '@mui/icons-material/LightMode';
import DarkModeIcon from '@mui/icons-material/DarkMode';

interface CodePreviewProps {
  code: string;
  language?: string;
  height?: string;
  readOnly?: boolean;
  title?: string;
  theme?: 'vs-dark' | 'light';
  onThemeChange?: (theme: 'vs-dark' | 'light') => void;
}

/**
 * CodePreview Component
 * 
 * Displays code using Monaco Editor (VS Code editor)
 * with syntax highlighting and professional styling.
 * 
 * Features:
 * - Syntax highlighting
 * - Loading state with spinner
 * - Theme toggle (dark/light)
 * - Theme persistence in localStorage
 * - Line numbers
 * - Code folding
 * - Read-only mode
 */
const CodePreview = ({ 
  code, 
  language = 'csharp', 
  height = '400px',
  readOnly = true,
  title = 'Code Preview',
  theme: externalTheme,
  onThemeChange
}: CodePreviewProps) => {
  const [isLoading, setIsLoading] = useState(true);
  
  // Theme management with localStorage persistence
  const [editorTheme, setEditorTheme] = useState<'vs-dark' | 'light'>(() => {
    if (externalTheme) return externalTheme;
    const saved = localStorage.getItem('monacoTheme');
    return (saved as 'vs-dark' | 'light') || 'vs-dark';
  });

  // Update theme when external theme changes
  useEffect(() => {
    if (externalTheme && externalTheme !== editorTheme) {
      setEditorTheme(externalTheme);
    }
  }, [externalTheme]);

  const toggleTheme = () => {
    const newTheme = editorTheme === 'vs-dark' ? 'light' : 'vs-dark';
    setEditorTheme(newTheme);
    localStorage.setItem('monacoTheme', newTheme);
    onThemeChange?.(newTheme);
  };

  return (
    <Paper sx={{ p: 2 }} elevation={2}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
        {title && (
          <Typography variant="h6">
            {title}
          </Typography>
        )}
        
        <Tooltip title={`Switch to ${editorTheme === 'vs-dark' ? 'light' : 'dark'} theme`}>
          <IconButton onClick={toggleTheme} size="small">
            {editorTheme === 'vs-dark' ? <LightModeIcon /> : <DarkModeIcon />}
          </IconButton>
        </Tooltip>
      </Box>
      
      {isLoading && (
        <Box 
          sx={{ 
            display: 'flex', 
            justifyContent: 'center', 
            alignItems: 'center',
            height,
            bgcolor: 'grey.900',
            borderRadius: 1
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
          theme={editorTheme}
          options={{
            readOnly,
            minimap: { enabled: false },
            scrollBeyondLastLine: false,
            fontSize: 14,
            lineNumbers: 'on',
            folding: true,
            automaticLayout: true,
            wordWrap: 'on',
            renderLineHighlight: 'all',
            cursorStyle: 'line',
            scrollbar: {
              vertical: 'visible',
              horizontal: 'visible'
            }
          }}
          onMount={() => setIsLoading(false)}
        />
      </Box>
    </Paper>
  );
};

export default CodePreview;
