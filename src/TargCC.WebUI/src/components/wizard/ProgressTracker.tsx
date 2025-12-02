import { useState } from 'react';
import { Box, Paper, LinearProgress, Typography, Chip, IconButton, Tooltip } from '@mui/material';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import ErrorIcon from '@mui/icons-material/Error';
import PendingIcon from '@mui/icons-material/Pending';
import HourglassEmptyIcon from '@mui/icons-material/HourglassEmpty';
import VisibilityIcon from '@mui/icons-material/Visibility';
import { getFileTypeIcon, getFileTypeColor } from '../../utils/fileTypeIcons';
import ReactComponentPreview from '../code/ReactComponentPreview';

export interface ProgressItem {
  id: string;
  name: string;
  type: string;
  status: 'pending' | 'processing' | 'complete' | 'error';
  message?: string;
  code?: string; // Optional: the generated code for preview
}

export interface ProgressTrackerProps {
  items: ProgressItem[];
  currentProgress: number;
  estimatedTimeRemaining?: number;
  currentFile?: string;
}

const ProgressTracker = ({
  items,
  currentProgress,
  estimatedTimeRemaining,
  currentFile
}: ProgressTrackerProps) => {
  const [previewItem, setPreviewItem] = useState<ProgressItem | null>(null);

  const isReactComponent = (item: ProgressItem) => {
    return item.type === 'react' && item.name.endsWith('.tsx');
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'complete':
        return <CheckCircleIcon color="success" fontSize="small" />;
      case 'error':
        return <ErrorIcon color="error" fontSize="small" />;
      case 'processing':
        return <PendingIcon color="primary" fontSize="small" />;
      default:
        return <HourglassEmptyIcon color="disabled" fontSize="small" />;
    }
  };

  const getStatusColor = (status: string): 'default' | 'primary' | 'success' | 'error' => {
    switch (status) {
      case 'complete':
        return 'success';
      case 'error':
        return 'error';
      case 'processing':
        return 'primary';
      default:
        return 'default';
    }
  };

  const completedCount = items.filter(item => item.status === 'complete').length;
  const totalCount = items.length;

  const formatTime = (seconds: number): string => {
    if (seconds < 60) return `${seconds}s`;
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;
    return `${minutes}m ${remainingSeconds}s`;
  };

  return (
    <Paper sx={{ p: 3 }} elevation={2}>
      <Box sx={{ mb: 3 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
          <Typography variant="h6">
            Generation Progress
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {completedCount} / {totalCount} files
          </Typography>
        </Box>

        <LinearProgress 
          variant="determinate" 
          value={currentProgress} 
          sx={{ 
            mb: 1, 
            height: 8, 
            borderRadius: 4,
            bgcolor: 'action.hover'
          }}
        />

        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Typography variant="body2" color="text.secondary">
            {Math.round(currentProgress)}% complete
          </Typography>
          {estimatedTimeRemaining !== undefined && estimatedTimeRemaining > 0 && (
            <Typography variant="body2" color="text.secondary">
              ~{formatTime(estimatedTimeRemaining)} remaining
            </Typography>
          )}
        </Box>
      </Box>

      {currentFile && (
        <Box sx={{ mb: 2, p: 2, bgcolor: 'action.hover', borderRadius: 1 }}>
          <Typography variant="body2" color="primary" fontWeight="medium">
            Currently generating: {currentFile}
          </Typography>
        </Box>
      )}

      <Box sx={{ 
        display: 'flex', 
        flexDirection: 'column', 
        gap: 1,
        maxHeight: 400,
        overflowY: 'auto'
      }}>
        {items.map((item) => (
          <Box 
            key={item.id}
            sx={{ 
              display: 'flex', 
              alignItems: 'center',
              gap: 2,
              p: 1.5,
              borderRadius: 1,
              bgcolor: item.status === 'processing' ? 'action.selected' : 'transparent',
              transition: 'background-color 0.3s ease',
              border: '1px solid',
              borderColor: item.status === 'processing' ? 'primary.main' : 'transparent'
            }}
          >
            <Box sx={{ minWidth: 24, display: 'flex', alignItems: 'center' }}>
              {getStatusIcon(item.status)}
            </Box>

            <Box sx={{ minWidth: 24, display: 'flex', alignItems: 'center', color: `${getFileTypeColor(item.type)}.main` }}>
              {getFileTypeIcon(item.type)}
            </Box>

            <Box sx={{ flex: 1, minWidth: 0 }}>
              <Typography 
                variant="body2" 
                sx={{ 
                  fontWeight: item.status === 'processing' ? 'medium' : 'normal',
                  overflow: 'hidden',
                  textOverflow: 'ellipsis',
                  whiteSpace: 'nowrap'
                }}
              >
                {item.name}
              </Typography>
              {item.message && (
                <Typography 
                  variant="caption" 
                  color="text.secondary"
                  sx={{
                    display: 'block',
                    overflow: 'hidden',
                    textOverflow: 'ellipsis',
                    whiteSpace: 'nowrap'
                  }}
                >
                  {item.message}
                </Typography>
              )}
            </Box>

            <Chip
              label={item.status}
              color={getStatusColor(item.status)}
              size="small"
              variant="outlined"
            />

            {isReactComponent(item) && item.status === 'complete' && (
              <Tooltip title="Preview Component">
                <IconButton
                  size="small"
                  onClick={() => setPreviewItem(item)}
                  color="primary"
                >
                  <VisibilityIcon fontSize="small" />
                </IconButton>
              </Tooltip>
            )}
          </Box>
        ))}
      </Box>

      {/* Preview Dialog */}
      {previewItem && (
        <ReactComponentPreview
          code={previewItem.code || `// Code not available for preview\n// Component: ${previewItem.name}`}
          componentName={previewItem.name.replace('.tsx', '')}
          open={previewItem !== null}
          onClose={() => setPreviewItem(null)}
        />
      )}
    </Paper>
  );
};

export default ProgressTracker;
