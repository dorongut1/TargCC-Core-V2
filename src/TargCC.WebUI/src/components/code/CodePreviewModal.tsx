import { useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  IconButton,
  Box,
  Tabs,
  Tab,
  Typography,
  Chip,
  Tooltip,
  CircularProgress,
} from '@mui/material';
import {
  Close as CloseIcon,
  Download as DownloadIcon,
  ContentCopy as CopyIcon,
} from '@mui/icons-material';
import CodePreview from './CodePreview';

export interface GeneratedFile {
  fileName: string;
  content: string;
  language: string;
  path: string;
}

interface CodePreviewModalProps {
  open: boolean;
  onClose: () => void;
  files: GeneratedFile[];
  tableName: string;
  loading?: boolean;
}

/**
 * CodePreviewModal Component
 *
 * Displays generated code files in a modal dialog with tabs for multiple files.
 *
 * Features:
 * - Multi-file support with tabs
 * - Monaco Editor for code preview
 * - Copy to clipboard
 * - Download individual files
 * - Fullscreen mode
 * - Loading state
 */
const CodePreviewModal = ({
  open,
  onClose,
  files,
  tableName,
  loading = false
}: CodePreviewModalProps) => {
  const [selectedTab, setSelectedTab] = useState(0);
  const [copySuccess, setCopySuccess] = useState(false);

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setSelectedTab(newValue);
  };

  const handleCopy = async () => {
    if (files.length > 0 && files[selectedTab]) {
      try {
        await navigator.clipboard.writeText(files[selectedTab].content);
        setCopySuccess(true);
        setTimeout(() => setCopySuccess(false), 2000);
      } catch (err) {
        console.error('Failed to copy:', err);
      }
    }
  };

  const handleDownload = () => {
    if (files.length > 0 && files[selectedTab]) {
      const file = files[selectedTab];
      const blob = new Blob([file.content], { type: 'text/plain' });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = file.fileName;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
    }
  };

  const currentFile = files.length > 0 && files[selectedTab] ? files[selectedTab] : null;

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="lg"
      fullWidth
      PaperProps={{
        sx: {
          height: '90vh',
          maxHeight: '90vh',
        },
      }}
    >
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', pb: 1 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Typography variant="h6">
            Generated Code: {tableName}
          </Typography>
          <Chip
            label={`${files.length} file${files.length !== 1 ? 's' : ''}`}
            size="small"
            color="primary"
            variant="outlined"
          />
        </Box>
        <IconButton edge="end" color="inherit" onClick={onClose} aria-label="close">
          <CloseIcon />
        </IconButton>
      </DialogTitle>

      <DialogContent dividers sx={{ p: 0, display: 'flex', flexDirection: 'column' }}>
        {loading ? (
          <Box
            sx={{
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              justifyContent: 'center',
              height: '100%',
              gap: 2,
            }}
          >
            <CircularProgress size={60} />
            <Typography variant="h6" color="text.secondary">
              Generating code...
            </Typography>
            <Typography variant="body2" color="text.secondary">
              This may take a few moments
            </Typography>
          </Box>
        ) : files.length === 0 ? (
          <Box
            sx={{
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              justifyContent: 'center',
              height: '100%',
            }}
          >
            <Typography variant="h6" color="text.secondary">
              No files generated
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Please generate code first
            </Typography>
          </Box>
        ) : (
          <>
            <Tabs
              value={selectedTab}
              onChange={handleTabChange}
              variant="scrollable"
              scrollButtons="auto"
              sx={{
                borderBottom: 1,
                borderColor: 'divider',
                bgcolor: 'background.paper',
                px: 2,
              }}
            >
              {files.map((file, index) => (
                <Tab
                  key={index}
                  label={file.fileName}
                  sx={{
                    textTransform: 'none',
                    fontFamily: 'monospace',
                    minHeight: 48,
                  }}
                />
              ))}
            </Tabs>

            <Box sx={{ flex: 1, overflow: 'hidden', p: 2 }}>
              {currentFile && (
                <>
                  <Box sx={{ mb: 1, display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Typography variant="caption" color="text.secondary" sx={{ fontFamily: 'monospace' }}>
                      {currentFile.path}
                    </Typography>
                    <Chip label={currentFile.language.toUpperCase()} size="small" variant="outlined" />
                  </Box>
                  <CodePreview
                    code={currentFile.content}
                    language={currentFile.language}
                    height="calc(90vh - 220px)"
                    readOnly
                    title=""
                  />
                </>
              )}
            </Box>
          </>
        )}
      </DialogContent>

      <DialogActions sx={{ px: 3, py: 2, justifyContent: 'space-between' }}>
        <Box sx={{ display: 'flex', gap: 1 }}>
          {currentFile && (
            <>
              <Tooltip title={copySuccess ? 'Copied!' : 'Copy to clipboard'}>
                <Button
                  startIcon={<CopyIcon />}
                  onClick={handleCopy}
                  variant={copySuccess ? 'contained' : 'outlined'}
                  color={copySuccess ? 'success' : 'primary'}
                  disabled={loading}
                >
                  {copySuccess ? 'Copied!' : 'Copy'}
                </Button>
              </Tooltip>
              <Tooltip title="Download this file">
                <Button
                  startIcon={<DownloadIcon />}
                  onClick={handleDownload}
                  variant="outlined"
                  disabled={loading}
                >
                  Download
                </Button>
              </Tooltip>
            </>
          )}
        </Box>
        <Button onClick={onClose} variant="contained">
          Close
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default CodePreviewModal;
