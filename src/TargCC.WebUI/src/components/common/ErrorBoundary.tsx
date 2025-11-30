import { Component, ReactNode } from 'react';
import { Box, Paper, Typography, Button, Alert } from '@mui/material';
import ErrorIcon from '@mui/icons-material/Error';
import RefreshIcon from '@mui/icons-material/Refresh';

export interface ErrorBoundaryProps {
  children: ReactNode;
  fallback?: ReactNode;
  onReset?: () => void;
}

interface ErrorBoundaryState {
  hasError: boolean;
  error?: Error;
  errorInfo?: string;
}

/**
 * ErrorBoundary component catches JavaScript errors anywhere in the child component tree
 * Provides a fallback UI with retry functionality
 */
class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = { 
      hasError: false,
      error: undefined,
      errorInfo: undefined
    };
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { 
      hasError: true, 
      error,
      errorInfo: error.stack
    };
  }
  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    // Log error to console for debugging
    console.error('ErrorBoundary caught an error:', error, errorInfo);
  }

  handleReset = () => {
    const { onReset } = this.props;
    
    // Call custom reset handler if provided
    if (onReset) {
      onReset();
    }

    // Reset error state
    this.setState({ 
      hasError: false, 
      error: undefined,
      errorInfo: undefined
    });
  };

  render() {
    const { hasError, error, errorInfo } = this.state;
    const { children, fallback } = this.props;

    if (hasError) {
      // Use custom fallback if provided
      if (fallback) {
        return fallback;
      }

      // Default error UI
      return (
        <Box
          sx={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            minHeight: '400px',
            p: 3
          }}
        >
          <Paper 
            sx={{ 
              p: 4, 
              textAlign: 'center', 
              maxWidth: 600, 
              width: '100%'
            }}
            elevation={3}
          >
            <ErrorIcon 
              color="error" 
              sx={{ fontSize: 64, mb: 2 }} 
            />
            
            <Typography variant="h5" gutterBottom>
              Something went wrong
            </Typography>
            
            <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
              An unexpected error occurred. Please try again.
            </Typography>
            {error && (
              <Alert 
                severity="error" 
                sx={{ my: 2, textAlign: 'left' }}
              >
                <Typography variant="body2" component="div">
                  <strong>Error:</strong> {error.message}
                </Typography>
                {errorInfo && (
                  <Typography 
                    variant="caption" 
                    component="pre" 
                    sx={{ 
                      mt: 1, 
                      overflow: 'auto', 
                      maxHeight: 200,
                      fontSize: '0.7rem'
                    }}
                  >
                    {errorInfo}
                  </Typography>
                )}
              </Alert>
            )}

            <Button
              variant="contained"
              startIcon={<RefreshIcon />}
              onClick={this.handleReset}
              size="large"
            >
              Try Again
            </Button>
          </Paper>
        </Box>
      );
    }

    return children;
  }
}

export default ErrorBoundary;
