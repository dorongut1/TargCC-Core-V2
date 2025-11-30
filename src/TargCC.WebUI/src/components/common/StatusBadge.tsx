import { Chip, ChipProps } from '@mui/material';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import ErrorIcon from '@mui/icons-material/Error';
import PendingIcon from '@mui/icons-material/Pending';
import HourglassEmptyIcon from '@mui/icons-material/HourglassEmpty';

export type StatusType = 'success' | 'error' | 'pending' | 'processing';

export interface StatusBadgeProps {
  status: StatusType;
  label?: string;
  size?: ChipProps['size'];
  variant?: ChipProps['variant'];
}

/**
 * StatusBadge component displays a status indicator with icon and label
 * Supports success, error, pending, and processing states
 */
const StatusBadge = ({ 
  status, 
  label, 
  size = 'small',
  variant = 'outlined'
}: StatusBadgeProps) => {
  const config = {
    success: { 
      color: 'success' as const, 
      icon: <CheckCircleIcon fontSize="small" />, 
      defaultLabel: 'Complete' 
    },
    error: { 
      color: 'error' as const, 
      icon: <ErrorIcon fontSize="small" />, 
      defaultLabel: 'Error' 
    },
    pending: { 
      color: 'default' as const, 
      icon: <HourglassEmptyIcon fontSize="small" />, 
      defaultLabel: 'Pending' 
    },
    processing: { 
      color: 'primary' as const, 
      icon: <PendingIcon fontSize="small" />, 
      defaultLabel: 'Processing' 
    },
  };

  const { color, icon, defaultLabel } = config[status];

  return (
    <Chip
      icon={icon}
      label={label || defaultLabel}
      color={color}
      size={size}
      variant={variant}
    />
  );
};

export default StatusBadge;
