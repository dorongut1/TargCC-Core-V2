/**
 * ReactComponentPreview Component
 *
 * Provides live preview of generated React components using an iframe sandbox.
 * Useful for visualizing generated UI without running a dev server.
 */

import { useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Box,
  Typography,
  Alert,
  Tabs,
  Tab,
  Paper,
  IconButton,
  Tooltip,
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import CodeIcon from '@mui/icons-material/Code';
import VisibilityIcon from '@mui/icons-material/Visibility';
import RefreshIcon from '@mui/icons-material/Refresh';
import Editor from '@monaco-editor/react';

interface ReactComponentPreviewProps {
  code: string;
  componentName: string;
  open: boolean;
  onClose: () => void;
}

const ReactComponentPreview = ({
  code,
  componentName,
  open,
  onClose,
}: ReactComponentPreviewProps) => {
  const [activeTab, setActiveTab] = useState<'preview' | 'code'>('preview');
  const [iframeKey, setIframeKey] = useState(0);

  const handleRefresh = () => {
    setIframeKey((prev) => prev + 1);
  };

  // Create a complete HTML document with React and the component
  const createPreviewHTML = () => {
    return `
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>${componentName} Preview</title>

  <!-- React, ReactDOM, Material-UI via CDN -->
  <script crossorigin src="https://unpkg.com/react@18/umd/react.production.min.js"></script>
  <script crossorigin src="https://unpkg.com/react-dom@18/umd/react-dom.production.min.js"></script>
  <script src="https://unpkg.com/@babel/standalone/babel.min.js"></script>

  <!-- Material-UI CSS -->
  <link rel="stylesheet" href="https://fonts.googleapis.com/css?family=Roboto:300,400,500,700&display=swap" />
  <link rel="stylesheet" href="https://fonts.googleapis.com/icon?family=Material+Icons" />

  <style>
    body {
      margin: 0;
      padding: 20px;
      font-family: 'Roboto', sans-serif;
      background: #f5f5f5;
    }
    #root {
      background: white;
      padding: 24px;
      border-radius: 8px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    }
    .error {
      color: #d32f2f;
      padding: 16px;
      background: #ffebee;
      border-radius: 4px;
      border-left: 4px solid #d32f2f;
    }
  </style>
</head>
<body>
  <div id="root"></div>

  <script type="text/babel">
    const { useState, useEffect } = React;
    const { createRoot } = ReactDOM;

    // Mock hooks and API calls for preview
    const mockData = {
      data: null,
      isLoading: false,
      error: null,
      refetch: () => console.log('Refetch called'),
    };

    // Mock custom hooks
    window.useCustomer = (id) => mockData;
    window.useCustomers = (filters) => ({ ...mockData, data: [] });
    window.useCreateCustomer = () => ({
      mutate: (data) => console.log('Create:', data),
      isLoading: false,
    });
    window.useUpdateCustomer = () => ({
      mutate: (data) => console.log('Update:', data),
      isLoading: false,
    });

    // Mock React Router
    const MockRouter = ({ children }) => children;
    window.useNavigate = () => (path) => console.log('Navigate to:', path);
    window.useParams = () => ({ id: '1' });

    // Simple form library mocks
    window.useFormik = (config) => ({
      values: config.initialValues || {},
      errors: {},
      touched: {},
      handleChange: (e) => console.log('Change:', e.target.name, e.target.value),
      handleBlur: () => {},
      handleSubmit: (e) => { e.preventDefault(); config.onSubmit?.(config.initialValues); },
      setFieldValue: (field, value) => console.log('Set:', field, value),
    });

    try {
      ${code}

      // Try to find and render the component
      const componentMatch = code.match(/export\\s+(const|function)\\s+(\\w+)/);
      const ComponentName = componentMatch ? componentMatch[2] : '${componentName}';
      const Component = window[ComponentName];

      if (Component) {
        const root = createRoot(document.getElementById('root'));
        root.render(
          <MockRouter>
            <Component />
          </MockRouter>
        );
      } else {
        document.getElementById('root').innerHTML = \`
          <div class="error">
            <h3>⚠️ Component Not Found</h3>
            <p>Could not find exported component: <code>\${ComponentName}</code></p>
            <p>This is a preview environment with limited functionality.</p>
          </div>
        \`;
      }
    } catch (error) {
      document.getElementById('root').innerHTML = \`
        <div class="error">
          <h3>⚠️ Preview Error</h3>
          <p><strong>\${error.name}:</strong> \${error.message}</p>
          <pre style="margin-top: 16px; padding: 12px; background: #fff; border-radius: 4px; overflow: auto;">\${error.stack}</pre>
          <hr style="margin: 16px 0; border: none; border-top: 1px solid #ccc;">
          <p style="font-size: 0.875rem; color: #666;">
            <strong>Note:</strong> This is a sandboxed preview environment with limited libraries.
            Some features may not work as expected. For full functionality, run the code in your development environment.
          </p>
        </div>
      \`;
    }
  </script>
</body>
</html>
    `.trim();
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="lg" fullWidth>
      <DialogTitle>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Box>
            <Typography variant="h6" component="span">
              {componentName} - Preview
            </Typography>
            <Typography variant="caption" color="text.secondary" sx={{ ml: 2 }}>
              Sandboxed Preview Environment
            </Typography>
          </Box>
          <Box sx={{ display: 'flex', gap: 1 }}>
            {activeTab === 'preview' && (
              <Tooltip title="Refresh Preview">
                <IconButton onClick={handleRefresh} size="small">
                  <RefreshIcon />
                </IconButton>
              </Tooltip>
            )}
            <IconButton onClick={onClose} size="small">
              <CloseIcon />
            </IconButton>
          </Box>
        </Box>
      </DialogTitle>

      <Box sx={{ borderBottom: 1, borderColor: 'divider', px: 3 }}>
        <Tabs value={activeTab} onChange={(_, val) => setActiveTab(val)}>
          <Tab icon={<VisibilityIcon />} label="Preview" value="preview" />
          <Tab icon={<CodeIcon />} label="Source Code" value="code" />
        </Tabs>
      </Box>

      <DialogContent sx={{ p: 0, height: 600 }}>
        {activeTab === 'preview' ? (
          <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
            <Alert severity="info" sx={{ m: 2, mb: 0, borderRadius: 0 }}>
              <Typography variant="body2">
                <strong>Preview Mode:</strong> This is a sandboxed preview with mock data.
                Some features (API calls, routing, etc.) are simulated. For full functionality,
                run the component in your development environment.
              </Typography>
            </Alert>
            <Box sx={{ flex: 1, position: 'relative' }}>
              <iframe
                key={iframeKey}
                srcDoc={createPreviewHTML()}
                title={`${componentName} Preview`}
                style={{
                  width: '100%',
                  height: '100%',
                  border: 'none',
                  background: 'white',
                }}
                sandbox="allow-scripts"
              />
            </Box>
          </Box>
        ) : (
          <Box sx={{ height: '100%', p: 2 }}>
            <Paper sx={{ height: '100%', overflow: 'hidden' }}>
              <Editor
                height="100%"
                language="typescript"
                value={code}
                theme="vs-dark"
                options={{
                  readOnly: true,
                  minimap: { enabled: false },
                  scrollBeyondLastLine: false,
                  fontSize: 13,
                  lineNumbers: 'on',
                  folding: true,
                  wordWrap: 'on',
                }}
              />
            </Paper>
          </Box>
        )}
      </DialogContent>

      <DialogActions>
        <Button onClick={onClose}>Close</Button>
      </DialogActions>
    </Dialog>
  );
};

export default ReactComponentPreview;
