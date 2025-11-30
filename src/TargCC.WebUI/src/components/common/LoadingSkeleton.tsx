import { Box, Skeleton, Paper } from '@mui/material';

export type SkeletonType = 'table' | 'card' | 'list';

export interface LoadingSkeletonProps {
  type?: SkeletonType;
  count?: number;
}

/**
 * LoadingSkeleton component provides loading placeholders for different content types
 * Supports table, card, and list skeletons with customizable count
 */
const LoadingSkeleton = ({ type = 'card', count = 3 }: LoadingSkeletonProps) => {
  if (type === 'table') {
    return (
      <Paper sx={{ p: 2 }}>
        <Skeleton variant="rectangular" height={40} sx={{ mb: 2, borderRadius: 1 }} />
        {Array.from({ length: count }).map((_, i) => (
          <Skeleton 
            key={i} 
            variant="rectangular" 
            height={60} 
            sx={{ mb: 1, borderRadius: 1 }} 
          />
        ))}
      </Paper>
    );
  }
  if (type === 'list') {
    return (
      <Box>
        {Array.from({ length: count }).map((_, i) => (
          <Box 
            key={i} 
            sx={{ 
              display: 'flex', 
              gap: 2, 
              mb: 2,
              alignItems: 'center'
            }}
          >
            <Skeleton variant="circular" width={40} height={40} />
            <Box sx={{ flex: 1 }}>
              <Skeleton variant="text" width="60%" height={24} />
              <Skeleton variant="text" width="40%" height={20} />
            </Box>
          </Box>
        ))}
      </Box>
    );
  }

  // Default: card layout
  return (
    <Box 
      sx={{ 
        display: 'grid', 
        gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', 
        gap: 2 
      }}
    >
      {Array.from({ length: count }).map((_, i) => (
        <Paper key={i} sx={{ p: 2 }}>
          <Skeleton 
            variant="rectangular" 
            height={140} 
            sx={{ mb: 2, borderRadius: 1 }} 
          />
          <Skeleton variant="text" height={24} sx={{ mb: 1 }} />
          <Skeleton variant="text" width="60%" height={20} />
        </Paper>
      ))}
    </Box>
  );
};

export default LoadingSkeleton;
