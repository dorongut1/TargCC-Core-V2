import { Box, Switch, FormControlLabel, Chip, IconButton, Tooltip } from '@mui/material';
import { Refresh as RefreshIcon } from '@mui/icons-material';

interface AutoRefreshControlProps {
  /** Whether auto-refresh is enabled */
  enabled: boolean;
  /** Callback when toggle is changed */
  onToggle: (enabled: boolean) => void;
  /** Timestamp of last refresh */
  lastRefresh: Date;
  /** Optional manual refresh callback */
  onManualRefresh?: () => void;
}

/**
 * Control component for auto-refresh functionality.
 * Displays toggle switch, last refresh time, and manual refresh button.
 */
const AutoRefreshControl = ({
  enabled,
  onToggle,
  lastRefresh,
  onManualRefresh
}: AutoRefreshControlProps) => {
  /**
   * Format timestamp as "time ago" string
   */
  const formatTimeAgo = (date: Date): string => {
    const seconds = Math.floor((Date.now() - date.getTime()) / 1000);
    
    if (seconds < 10) return 'just now';
    if (seconds < 60) return `${seconds}s ago`;
    
    const minutes = Math.floor(seconds / 60);
    if (minutes < 60) return `${minutes}m ago`;
    
    const hours = Math.floor(minutes / 60);
    if (hours < 24) return `${hours}h ago`;
    
    const days = Math.floor(hours / 24);
    return `${days}d ago`;
  };

  return (
    <Box display="flex" alignItems="center" gap={2}>
      <FormControlLabel
        control={
          <Switch
            checked={enabled}
            onChange={(e) => onToggle(e.target.checked)}
            color="primary"
          />
        }
        label="Auto-refresh"
      />
      
      {enabled && (
        <Chip
          icon={<RefreshIcon />}
          label={`Updated ${formatTimeAgo(lastRefresh)}`}
          size="small"
          variant="outlined"
          color="primary"
        />
      )}

      {onManualRefresh && (
        <Tooltip title="Refresh now">
          <IconButton
            size="small"
            onClick={onManualRefresh}
            color="primary"
          >
            <RefreshIcon />
          </IconButton>
        </Tooltip>
      )}
    </Box>
  );
};

export default AutoRefreshControl;
