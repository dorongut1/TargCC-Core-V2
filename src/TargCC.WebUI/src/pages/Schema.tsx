import { Container, Stack, Paper, Typography } from '@mui/material';
import SchemaViewer from '../components/schema/SchemaViewer';
import SchemaStats from '../components/schema/SchemaStats';
import RelationshipGraph from '../components/schema/RelationshipGraph';
import ExportMenu from '../components/schema/ExportMenu';
import { mockSchema } from '../utils/mockSchema';

/**
 * Schema Page
 * 
 * Displays comprehensive database schema visualization including:
 * - Statistics dashboard
 * - Relationship diagram
 * - Table and column details
 * - Export functionality
 * 
 * Route: /schema
 */
const Schema = () => {
  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Stack spacing={3}>
        {/* Page Header */}
        <Paper sx={{ p: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Typography variant="h4">Database Schema</Typography>
          <ExportMenu schema={mockSchema} />
        </Paper>

        {/* Statistics */}
        <SchemaStats schema={mockSchema} />

        {/* Relationship Graph */}
        <RelationshipGraph schema={mockSchema} />

        {/* Schema Viewer */}
        <SchemaViewer schema={mockSchema} />
      </Stack>
    </Container>
  );
};

export default Schema;
