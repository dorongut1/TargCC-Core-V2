import { useState } from 'react';
import {
  Box,
  Tabs,
  Tab,
  IconButton,
  Tooltip,
  Alert,
  Select,
  MenuItem,
  FormControl,
  InputLabel
} from '@mui/material';
import ContentCopyIcon from '@mui/icons-material/ContentCopy';
import CheckIcon from '@mui/icons-material/Check';
import DownloadIcon from '@mui/icons-material/Download';
import FolderZipIcon from '@mui/icons-material/FolderZip';
import CodePreview from './CodePreview';
import { downloadFile, downloadAllAsZip } from '../../utils/downloadCode';

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
 * Multi-file code viewer with tabs, language selection, and download functionality.
 * 
 * Features:
 * - Multiple file tabs
 * - File switching
 * - Language selector (C#, TypeScript, JavaScript, SQL, JSON)
 * - Copy to clipboard
 * - Download single file
 * - Download all files as ZIP
 * - Visual feedback on actions
 * - Monaco Editor integration
 */
const CodeViewer = ({ files }: CodeViewerProps) => {
  const [activeTab, setActiveTab] = useState(0);
  const [copied, setCopied] = useState(false);
  const [downloading, setDownloading] = useState(false);
  const [language, setLanguage] = useState(files[0]?.language || 'csharp');

  const languages = [
    { value: 'csharp', label: 'C#' },
    { value: 'typescript', label: 'TypeScript' },
    { value: 'javascript', label: 'JavaScript' },
    { value: 'sql', label: 'SQL' },
    { value: 'json', label: 'JSON' },
  ];

  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(files[activeTab].code);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch (err) {
      console.error('Failed to copy:', err);
    }
  };

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
          alignItems: 'center',
          gap: 2,
          flexWrap: 'wrap'
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
        
        <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
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

          <Tooltip title={copied ? 'Copied!' : 'Copy to clipboard'}>
            <IconButton onClick={handleCopy} color={copied ? 'success' : 'default'}>
              {copied ? <CheckIcon /> : <ContentCopyIcon />}
            </IconButton>
          </Tooltip>

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

      <Box sx={{ mt: 2 }}>
        <CodePreview
          code={files[activeTab].code}
          language={language}
          title={`${files[activeTab].name}`}
          height="500px"
        />
      </Box>
    </Box>
  );
};

export default CodeViewer;
