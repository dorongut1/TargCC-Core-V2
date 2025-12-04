import { useState } from 'react';
import {
  Container,
  Typography,
  Box,
  Paper,
  Tabs,
  Tab,
  Alert,
  Button,
  Chip,
} from '@mui/material';
import SmartToyIcon from '@mui/icons-material/SmartToy';
import CodeIcon from '@mui/icons-material/Code';
import AICodeEditor from '../components/code/AICodeEditor';
import { generateMockReactComponent, generateSimpleReactComponent } from '../utils/mockReactCode';

/**
 * AI Code Editor Demo Page
 *
 * Demonstrates the AI-powered code modification capabilities
 */
const AICodeEditorDemo = () => {
  const [selectedExample, setSelectedExample] = useState<'simple' | 'complex'>('simple');
  const [currentCode, setCurrentCode] = useState(generateSimpleReactComponent());

  const handleExampleChange = (_: React.SyntheticEvent, value: 'simple' | 'complex') => {
    setSelectedExample(value);
    if (value === 'simple') {
      setCurrentCode(generateSimpleReactComponent());
    } else {
      setCurrentCode(generateMockReactComponent('Customer'));
    }
  };

  const handleCodeChange = (newCode: string) => {
    setCurrentCode(newCode);
  };

  const handleSave = (code: string) => {
    console.log('Saving code:', code);
    // TODO: Implement actual save functionality
  };

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      {/* Header */}
      <Box sx={{ mb: 4 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
          <SmartToyIcon sx={{ fontSize: 40, color: 'primary.main' }} />
          <Typography variant="h4">
            AI Code Editor Demo
          </Typography>
          <Chip label="Phase 3F" color="primary" />
        </Box>

        <Typography variant="body1" color="text.secondary" paragraph>
          Modify generated React code using natural language instructions.
          Try commands like "Make the button blue" or "Add validation to email field".
        </Typography>

        <Alert severity="info" sx={{ mb: 2 }}>
          <Typography variant="body2">
            <strong>Note:</strong> This demo requires a Claude AI API key configured in the backend
            (appsettings.json). Without it, you can still explore the UI but modifications won't work.
          </Typography>
        </Alert>
      </Box>

      {/* Example Selector */}
      <Paper sx={{ mb: 3, p: 2 }}>
        <Typography variant="h6" gutterBottom>
          Choose an Example
        </Typography>
        <Tabs value={selectedExample} onChange={handleExampleChange}>
          <Tab
            label="Simple Form"
            value="simple"
            icon={<CodeIcon />}
            iconPosition="start"
          />
          <Tab
            label="Complex Form (Customer)"
            value="complex"
            icon={<CodeIcon />}
            iconPosition="start"
          />
        </Tabs>
      </Paper>

      {/* Example Prompts */}
      <Paper sx={{ mb: 3, p: 2 }}>
        <Typography variant="subtitle2" gutterBottom>
          Try These Commands:
        </Typography>
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
          <Chip label="Make the button blue" size="small" variant="outlined" />
          <Chip label="Change button to left side" size="small" variant="outlined" />
          <Chip label="Add email validation" size="small" variant="outlined" />
          <Chip label="Change grid to 2 columns" size="small" variant="outlined" />
          <Chip label="Add a loading spinner" size="small" variant="outlined" />
          <Chip label="Make text fields smaller" size="small" variant="outlined" />
          <Chip label="Add phone field after email" size="small" variant="outlined" />
        </Box>
      </Paper>

      {/* AI Code Editor */}
      <AICodeEditor
        initialCode={currentCode}
        tableName={selectedExample === 'simple' ? 'ContactForm' : 'Customer'}
        schema="dbo"
        relatedTables={selectedExample === 'complex' ? ['Orders', 'Addresses'] : []}
        language="typescript"
        height="600px"
        onCodeChange={handleCodeChange}
        onSave={handleSave}
      />

      {/* Instructions */}
      <Paper sx={{ mt: 3, p: 2, bgcolor: 'grey.50' }}>
        <Typography variant="h6" gutterBottom>
          How to Use
        </Typography>
        <Box component="ol" sx={{ pl: 2 }}>
          <Typography component="li" variant="body2" paragraph>
            <strong>Choose an example</strong> from the tabs above (Simple or Complex)
          </Typography>
          <Typography component="li" variant="body2" paragraph>
            <strong>Type instructions</strong> in the chat panel on the right
            (e.g., "Make the save button blue and move it to the left")
          </Typography>
          <Typography component="li" variant="body2" paragraph>
            <strong>Press Enter or click Send</strong> to submit your instruction
          </Typography>
          <Typography component="li" variant="body2" paragraph>
            <strong>Watch the AI modify</strong> your code in real-time
          </Typography>
          <Typography component="li" variant="body2" paragraph>
            <strong>Review changes</strong> in the Diff tab to see exactly what changed
          </Typography>
          <Typography component="li" variant="body2" paragraph>
            <strong>Use Undo/Redo</strong> buttons to navigate through your modifications
          </Typography>
        </Box>
      </Paper>

      {/* Technical Info */}
      <Paper sx={{ mt: 2, p: 2, bgcolor: 'info.light', color: 'info.contrastText' }}>
        <Typography variant="subtitle2" gutterBottom>
          Technical Details
        </Typography>
        <Typography variant="body2">
          • Backend: AICodeEditorService (.NET 9) with Claude AI integration
          <br />
          • Frontend: React 19 with Monaco Editor and Material-UI
          <br />
          • Features: Natural language processing, real-time validation, change tracking, undo/redo
          <br />
          • API Endpoints: /api/ai/code/modify, /validate, /diff
        </Typography>
      </Paper>
    </Container>
  );
};

export default AICodeEditorDemo;
