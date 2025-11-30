import { Container } from '@mui/material';
import SchemaViewer from '../components/schema/SchemaViewer';
import { mockSchema } from '../utils/mockSchema';

/**
 * Schema Page
 * 
 * Displays database schema visualization with table and column details.
 * Uses mock data for development.
 * 
 * Route: /schema
 */
const Schema = () => {
  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <SchemaViewer schema={mockSchema} />
    </Container>
  );
};

export default Schema;
