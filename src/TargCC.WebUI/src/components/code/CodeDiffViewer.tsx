/**
 * CodeDiffViewer Component
 *
 * Displays a side-by-side or unified diff view of code changes.
 * Shows line-by-line changes with color coding.
 *
 * Features:
 * - Side-by-side comparison
 * - Color-coded changes (additions, deletions, modifications)
 * - Line number display
 * - Change statistics
 * - Monaco Editor-based diff viewer
 */

import { Box, Paper, Typography, Chip } from '@mui/material';
import { DiffEditor } from '@monaco-editor/react';
import AddIcon from '@mui/icons-material/Add';
import RemoveIcon from '@mui/icons-material/Remove';
import EditIcon from '@mui/icons-material/Edit';
import type { CodeChange } from '../../types/aiCodeEditor';

interface CodeDiffViewerProps {
  originalCode: string;
  modifiedCode: string;
  changes: CodeChange[];
  height?: string;
  language?: string;
}

const CodeDiffViewer = ({
  originalCode,
  modifiedCode,
  changes,
  height = '600px',
  language = 'typescript',
}: CodeDiffViewerProps) => {
  // Calculate change statistics
  const additions = changes.filter((c) => c.type === 'Addition').length;
  const deletions = changes.filter((c) => c.type === 'Deletion').length;
  const modifications = changes.filter((c) => c.type === 'Modification').length;

  return (
    <Box>
      {/* Statistics */}
      <Box sx={{ display: 'flex', gap: 1, mb: 2 }}>
        <Chip
          icon={<AddIcon />}
          label={`${additions} added`}
          size="small"
          color="success"
          variant="outlined"
        />
        <Chip
          icon={<RemoveIcon />}
          label={`${deletions} removed`}
          size="small"
          color="error"
          variant="outlined"
        />
        <Chip
          icon={<EditIcon />}
          label={`${modifications} modified`}
          size="small"
          color="info"
          variant="outlined"
        />
      </Box>

      {/* Diff Editor */}
      <Paper sx={{ overflow: 'hidden' }}>
        <DiffEditor
          height={height}
          language={language}
          original={originalCode}
          modified={modifiedCode}
          theme="vs-dark"
          options={{
            readOnly: true,
            renderSideBySide: true,
            minimap: { enabled: false },
            scrollBeyondLastLine: false,
            fontSize: 14,
            lineNumbers: 'on',
            folding: true,
            automaticLayout: true,
            wordWrap: 'on',
            enableSplitViewResizing: true,
            renderOverviewRuler: true,
          }}
        />
      </Paper>

      {/* Change Details */}
      {changes.length > 0 && (
        <Paper sx={{ mt: 2, p: 2 }}>
          <Typography variant="subtitle2" gutterBottom>
            Change Details
          </Typography>
          <Box sx={{ maxHeight: '200px', overflowY: 'auto' }}>
            {changes.slice(0, 20).map((change, index) => (
              <Box
                key={index}
                sx={{
                  py: 0.5,
                  display: 'flex',
                  gap: 1,
                  alignItems: 'flex-start',
                  borderBottom: index < changes.length - 1 ? '1px solid' : 'none',
                  borderColor: 'divider',
                }}
              >
                <Chip
                  label={`L${change.lineNumber}`}
                  size="small"
                  sx={{ minWidth: 50 }}
                />
                <Chip
                  label={change.type}
                  size="small"
                  color={
                    change.type === 'Addition'
                      ? 'success'
                      : change.type === 'Deletion'
                      ? 'error'
                      : 'info'
                  }
                  sx={{ minWidth: 100 }}
                />
                <Typography variant="body2" sx={{ flex: 1 }}>
                  {change.description}
                </Typography>
              </Box>
            ))}
            {changes.length > 20 && (
              <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
                Showing first 20 of {changes.length} changes
              </Typography>
            )}
          </Box>
        </Paper>
      )}
    </Box>
  );
};

export default CodeDiffViewer;
