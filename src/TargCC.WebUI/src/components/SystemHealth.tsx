/**
 * SystemHealth Component
 * Displays system health metrics and status
 */

import { Card, CardContent, Typography, Box, LinearProgress, Chip } from '@mui/material';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import WarningIcon from '@mui/icons-material/Warning';
import ErrorIcon from '@mui/icons-material/Error';

export interface SystemHealthProps {
  cpuUsage: number;
  memoryUsage: number;
  diskUsage: number;
  status: 'healthy' | 'warning' | 'critical';
}

export const SystemHealth: React.FC<SystemHealthProps> = ({
  cpuUsage,
  memoryUsage,
  diskUsage,
  status,
}) => {
  const getStatusIcon = () => {
    switch (status) {
      case 'healthy':
        return <CheckCircleIcon sx={{ color: 'success.main' }} />;
      case 'warning':
        return <WarningIcon sx={{ color: 'warning.main' }} />;
      case 'critical':
        return <ErrorIcon sx={{ color: 'error.main' }} />;
    }
  };

  const getStatusColor = () => {
    switch (status) {
      case 'healthy':
        return 'success';
      case 'warning':
        return 'warning';
      case 'critical':
        return 'error';
    }
  };

  const getProgressColor = (value: number): 'success' | 'warning' | 'error' => {
    if (value < 60) return 'success';
    if (value < 80) return 'warning';
    return 'error';
  };

  return (
    <Card>
      <CardContent>
        <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
          <Typography variant="h6">System Health</Typography>
          <Box display="flex" alignItems="center" gap={1}>
            {getStatusIcon()}
            <Chip
              label={status.toUpperCase()}
              color={getStatusColor()}
              size="small"
            />
          </Box>
        </Box>

        <Box mb={2}>
          <Box display="flex" justifyContent="space-between" mb={1}>
            <Typography variant="body2" color="text.secondary">
              CPU Usage
            </Typography>
            <Typography variant="body2" fontWeight="medium">
              {cpuUsage}%
            </Typography>
          </Box>
          <LinearProgress
            variant="determinate"
            value={cpuUsage}
            color={getProgressColor(cpuUsage)}
          />
        </Box>

        <Box mb={2}>
          <Box display="flex" justifyContent="space-between" mb={1}>
            <Typography variant="body2" color="text.secondary">
              Memory Usage
            </Typography>
            <Typography variant="body2" fontWeight="medium">
              {memoryUsage}%
            </Typography>
          </Box>
          <LinearProgress
            variant="determinate"
            value={memoryUsage}
            color={getProgressColor(memoryUsage)}
          />
        </Box>

        <Box>
          <Box display="flex" justifyContent="space-between" mb={1}>
            <Typography variant="body2" color="text.secondary">
              Disk Usage
            </Typography>
            <Typography variant="body2" fontWeight="medium">
              {diskUsage}%
            </Typography>
          </Box>
          <LinearProgress
            variant="determinate"
            value={diskUsage}
            color={getProgressColor(diskUsage)}
          />
        </Box>
      </CardContent>
    </Card>
  );
};
