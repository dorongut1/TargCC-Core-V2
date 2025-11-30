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

/**
 * CodePreview Component
 * 
 * Displays code using Monaco Editor (VS Code editor)
 * with syntax highlighting and professional styling.
 * 
 * Features:
 * - Syntax highlighting
 * - Loading state with spinner
 * - Dark theme
 * - Line numbers
 * - Code folding
 * - Read-only mode
 */
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
      {title && (
        <Typography variant="h6" gutterBottom>
          {title}
        </Typography>
      )}
      
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
          theme="vs-dark"
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
