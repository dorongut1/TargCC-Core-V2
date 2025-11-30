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

/**
 * CodeViewer Component
 * 
 * Multi-file code viewer with tabs and copy functionality.
 * 
 * Features:
 * - Multiple file tabs
 * - File switching
 * - Copy to clipboard
 * - Visual feedback on copy
 * - Monaco Editor integration
 */
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
