/**
 * Main App Component
 * Root component with routing and layout
 */

import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ThemeProvider, createTheme, CssBaseline } from '@mui/material';
import { Layout } from './components/Layout';
import { Dashboard } from './pages/Dashboard';
import { Tables } from './pages/Tables';
import GenerationWizard from './components/wizard/GenerationWizard';

// Create a Material-UI theme
const theme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
  },
});

// Create a React Query client
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <Router>
          <Layout>
            <Routes>
              <Route path="/" element={<Dashboard />} />
              <Route path="/tables" element={<Tables />} />
              <Route path="/generate" element={<GenerationWizard />} />
              <Route path="/suggestions" element={<div>AI Suggestions - Coming Soon</div>} />
              <Route path="/security" element={<div>Security Page - Coming Soon</div>} />
              <Route path="/chat" element={<div>AI Chat - Coming Soon</div>} />
            </Routes>
          </Layout>
        </Router>
      </ThemeProvider>
    </QueryClientProvider>
  );
}

export default App;
