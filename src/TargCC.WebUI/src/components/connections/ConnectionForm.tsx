/**
 * ConnectionForm Component
 * Form for creating and editing database connections
 */

import { useState, useEffect } from 'react';
import {
  Box,
  TextField,
  FormControlLabel,
  Checkbox,
  Button,
  Alert,
  CircularProgress,
  Typography,
} from '@mui/material';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import ErrorIcon from '@mui/icons-material/Error';

export interface ConnectionFormData {
  id?: string;
  name: string;
  server: string;
  database: string;
  useIntegratedSecurity: boolean;
  username?: string;
  password?: string;
}

interface ConnectionFormProps {
  initialData?: ConnectionFormData;
  onSubmit: (data: ConnectionFormData) => Promise<void>;
  onCancel: () => void;
  onTestConnection: (connectionString: string) => Promise<boolean>;
}

function ConnectionForm({
  initialData,
  onSubmit,
  onCancel,
  onTestConnection,
}: ConnectionFormProps) {
  const [formData, setFormData] = useState<ConnectionFormData>({
    name: '',
    server: '',
    database: '',
    useIntegratedSecurity: true,
    username: '',
    password: '',
    ...initialData,
  });

  const [errors, setErrors] = useState<Partial<Record<keyof ConnectionFormData, string>>>({});
  const [testing, setTesting] = useState(false);
  const [testResult, setTestResult] = useState<'success' | 'error' | null>(null);
  const [submitting, setSubmitting] = useState(false);

  // Reset test result when form data changes
  useEffect(() => {
    setTestResult(null);
  }, [formData.server, formData.database, formData.useIntegratedSecurity, formData.username, formData.password]);

  const buildConnectionString = (): string => {
    const parts = [
      `Server=${formData.server}`,
      `Database=${formData.database}`,
    ];

    if (formData.useIntegratedSecurity) {
      parts.push('Trusted_Connection=True');
    } else {
      if (formData.username) {
        parts.push(`User Id=${formData.username}`);
      }
      if (formData.password) {
        parts.push(`Password=${formData.password}`);
      }
    }

    parts.push('TrustServerCertificate=True');

    return parts.join(';') + ';';
  };

  const validate = (): boolean => {
    const newErrors: Partial<Record<keyof ConnectionFormData, string>> = {};

    if (!formData.name.trim()) {
      newErrors.name = 'Connection name is required';
    }

    if (!formData.server.trim()) {
      newErrors.server = 'Server address is required';
    }

    if (!formData.database.trim()) {
      newErrors.database = 'Database name is required';
    }

    if (!formData.useIntegratedSecurity) {
      if (!formData.username?.trim()) {
        newErrors.username = 'Username is required for SQL Authentication';
      }
      if (!formData.password?.trim()) {
        newErrors.password = 'Password is required for SQL Authentication';
      }
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleTest = async () => {
    if (!validate()) {
      return;
    }

    setTesting(true);
    setTestResult(null);

    try {
      const connectionString = buildConnectionString();
      const isValid = await onTestConnection(connectionString);
      setTestResult(isValid ? 'success' : 'error');
    } catch (error) {
      setTestResult('error');
    } finally {
      setTesting(false);
    }
  };

  const handleSubmit = async () => {
    if (!validate()) {
      return;
    }

    setSubmitting(true);

    try {
      await onSubmit(formData);
    } catch (error) {
      console.error('Failed to submit form:', error);
    } finally {
      setSubmitting(false);
    }
  };

  const handleChange = (field: keyof ConnectionFormData) => (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    const value = event.target.type === 'checkbox' ? event.target.checked : event.target.value;
    setFormData((prev) => ({ ...prev, [field]: value }));
    // Clear error for this field
    if (errors[field]) {
      setErrors((prev) => {
        const newErrors = { ...prev };
        delete newErrors[field];
        return newErrors;
      });
    }
  };

  return (
    <Box>
      <TextField
        fullWidth
        label="Connection Name"
        value={formData.name}
        onChange={handleChange('name')}
        error={!!errors.name}
        helperText={errors.name || 'A friendly name for this connection'}
        margin="normal"
        autoFocus
      />

      <TextField
        fullWidth
        label="Server"
        value={formData.server}
        onChange={handleChange('server')}
        error={!!errors.server}
        helperText={errors.server || 'e.g., localhost or 192.168.1.100'}
        margin="normal"
      />

      <TextField
        fullWidth
        label="Database"
        value={formData.database}
        onChange={handleChange('database')}
        error={!!errors.database}
        helperText={errors.database || 'The name of the database to connect to'}
        margin="normal"
      />

      <FormControlLabel
        control={
          <Checkbox
            checked={formData.useIntegratedSecurity}
            onChange={handleChange('useIntegratedSecurity')}
          />
        }
        label="Use Windows Authentication (Integrated Security)"
        sx={{ mt: 2, mb: 1 }}
      />

      {!formData.useIntegratedSecurity && (
        <Box sx={{ pl: 2, mt: 1 }}>
          <TextField
            fullWidth
            label="Username"
            value={formData.username || ''}
            onChange={handleChange('username')}
            error={!!errors.username}
            helperText={errors.username}
            margin="normal"
          />
          <TextField
            fullWidth
            label="Password"
            type="password"
            value={formData.password || ''}
            onChange={handleChange('password')}
            error={!!errors.password}
            helperText={errors.password}
            margin="normal"
          />
        </Box>
      )}

      {/* Connection String Preview */}
      <Box sx={{ mt: 3, mb: 2, p: 2, bgcolor: 'grey.100', borderRadius: 1 }}>
        <Typography variant="caption" color="text.secondary" gutterBottom>
          Connection String Preview:
        </Typography>
        <Typography
          variant="body2"
          sx={{
            fontFamily: 'monospace',
            wordBreak: 'break-all',
            mt: 1,
          }}
        >
          {buildConnectionString()}
        </Typography>
      </Box>

      {/* Test Result */}
      {testResult && (
        <Alert
          severity={testResult === 'success' ? 'success' : 'error'}
          icon={testResult === 'success' ? <CheckCircleIcon /> : <ErrorIcon />}
          sx={{ mb: 2 }}
        >
          {testResult === 'success'
            ? 'Connection test successful! ✅'
            : 'Connection test failed. Please check your settings.'}
        </Alert>
      )}

      {/* Action Buttons */}
      <Box sx={{ display: 'flex', gap: 2, mt: 3 }}>
        <Button
          variant="outlined"
          onClick={handleTest}
          disabled={testing || submitting}
          startIcon={testing ? <CircularProgress size={16} /> : <CheckCircleIcon />}
        >
          {testing ? 'Testing...' : 'Test Connection'}
        </Button>
        <Box sx={{ flex: 1 }} />
        <Button onClick={onCancel} disabled={submitting}>
          Cancel
        </Button>
        <Button
          variant="contained"
          onClick={handleSubmit}
          disabled={submitting || testing}
        >
          {submitting ? <CircularProgress size={24} /> : initialData?.id ? 'Save' : 'Add'}
        </Button>
      </Box>
    </Box>
  );
}

export default ConnectionForm;
