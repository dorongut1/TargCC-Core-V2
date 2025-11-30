import { Container, Typography, Box } from '@mui/material';
import CodeViewer from '../components/code/CodeViewer';
import { mockCodeFiles } from '../utils/mockCode';

/**
 * Code Demo Page
 * 
 * Temporary page to test Monaco Editor integration
 */
const CodeDemo = () => {
  const files = mockCodeFiles('Customer');

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Typography variant="h4" gutterBottom>
        Monaco Editor Demo
      </Typography>
      
      <Typography variant="body1" color="text.secondary" paragraph>
        Testing CodeViewer component with mock generated code
      </Typography>

      <Box sx={{ mt: 4 }}>
        <CodeViewer files={files} />
      </Box>
    </Container>
  );
};

export default CodeDemo;
