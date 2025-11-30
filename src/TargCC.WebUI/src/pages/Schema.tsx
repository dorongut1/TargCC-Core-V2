import { useState } from 'react';
import {
  Container,
  Stack,
  Paper,
  Typography,
  Alert,
  CircularProgress,
  Box,
  IconButton,
  Tooltip,
  Chip,
  AlertTitle,
} from '@mui/material';
import RefreshIcon from '@mui/icons-material/Refresh';
import CloudOffIcon from '@mui/icons-material/CloudOff';
import CloudDoneIcon from '@mui/icons-material/CloudDone';
import SchemaViewer from '../components/schema/SchemaViewer';
import SchemaStats from '../components/schema/SchemaStats';
import RelationshipGraph from '../components/schema/RelationshipGraph';
import ExportMenu from '../components/schema/ExportMenu';
import { useSchema } from '../hooks/useSchema';
import { mockSchema } from '../utils/mockSchema';

/**
 * Schema Page
 * 
 * Displays comprehensive database schema visualization including:
 * - Live API connection with backend
 * - Statistics dashboard
 * - Relationship diagram
 * - Table and column details
 * - Export functionality
 * - Real-time refresh capability
 * 
 * Route: /schema
 */
const Schema = () => {
  // For now, hardcode schema name - later we'll add a selector
  const [selectedSchema] = useState<string>('dbo');
  
  // Load schema from API
  const { schema, loading, error, refresh, lastUpdated, isConnected } = useSchema(selectedSchema);

  // Determine which schema to display
  const enableMockFallback = import.meta.env.VITE_ENABLE_MOCK_FALLBACK === 'true';
  const displaySchema = schema || (enableMockFallback ? mockSchema : null);
  const usingMockData = !schema && enableMockFallback;

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Stack spacing={3}>
        {/* Page Header */}
        <Paper sx={{ p: 2 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Box>
              <Typography variant="h4">Database Schema</Typography>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 0.5 }}>
                {lastUpdated && (
                  <Typography variant="caption" color="text.secondary">
                    Last updated: {lastUpdated.toLocaleString()}
                  </Typography>
                )}
                <Chip
                  icon={isConnected ? <CloudDoneIcon /> : <CloudOffIcon />}
                  label={isConnected ? 'Connected' : 'Disconnected'}
                  color={isConnected ? 'success' : 'default'}
                  size="small"
                  sx={{ ml: 1 }}
                />
                {usingMockData && (
                  <Chip
                    label="Using Mock Data"
                    color="warning"
                    size="small"
                  />
                )}
              </Box>
            </Box>
            <Box sx={{ display: 'flex', gap: 1 }}>
              <Tooltip title="Refresh Schema from Database">
                <IconButton onClick={refresh} disabled={loading} color="primary">
                  <RefreshIcon />
                </IconButton>
              </Tooltip>
              {displaySchema && <ExportMenu schema={displaySchema} />}
            </Box>
          </Box>
        </Paper>

        {/* Loading State */}
        {loading && (
          <Paper sx={{ p: 3, textAlign: 'center' }}>
            <CircularProgress />
            <Typography sx={{ mt: 2 }} color="text.secondary">
              Loading schema from database...
            </Typography>
          </Paper>
        )}

        {/* Error State */}
        {error && !loading && (
          <Alert 
            severity={usingMockData ? 'warning' : 'error'}
            action={
              <IconButton color="inherit" size="small" onClick={refresh}>
                <RefreshIcon />
              </IconButton>
            }
          >
            <AlertTitle>{usingMockData ? 'API Connection Failed' : 'Error'}</AlertTitle>
            {error}
            {usingMockData && (
              <Typography variant="body2" sx={{ mt: 1 }}>
                Displaying mock data instead. Check that the WebAPI is running on {import.meta.env.VITE_API_URL}
              </Typography>
            )}
          </Alert>
        )}

        {/* No Data State */}
        {!loading && !displaySchema && (
          <Alert severity="info">
            <AlertTitle>No Schema Available</AlertTitle>
            Unable to load schema data. Please ensure the WebAPI is running and try refreshing.
          </Alert>
        )}

        {/* Content - Only show when we have data */}
        {!loading && displaySchema && (
          <>
            {/* Statistics */}
            <SchemaStats schema={displaySchema} />

            {/* Relationship Graph */}
            <RelationshipGraph schema={displaySchema} />

            {/* Schema Viewer */}
            <SchemaViewer schema={displaySchema} />
          </>
        )}
      </Stack>
    </Container>
  );
};

export default Schema;
