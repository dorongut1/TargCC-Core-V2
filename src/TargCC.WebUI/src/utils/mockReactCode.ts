/**
 * Mock React Component Code Utility
 *
 * Generates realistic React/TypeScript component examples.
 * Used to demonstrate AI code modification capabilities.
 */

/**
 * Generate mock React component code for a given table
 */
export const generateMockReactComponent = (tableName: string) => {
  return `import { useState, useEffect } from 'react';
import {
  Box,
  Paper,
  TextField,
  Button,
  Typography,
  Grid,
  Alert,
  CircularProgress,
} from '@mui/material';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import { use${tableName}s } from '../hooks/use${tableName}s';

interface ${tableName}FormProps {
  initialData?: ${tableName};
  onSubmit: (data: ${tableName}) => Promise<void>;
  onCancel: () => void;
}

interface ${tableName} {
  id: number;
  name: string;
  email: string;
  phone: string;
  address: string;
  createdAt: Date;
}

const validationSchema = Yup.object({
  name: Yup.string().required('Name is required'),
  email: Yup.string().email('Invalid email').required('Email is required'),
  phone: Yup.string().required('Phone is required'),
  address: Yup.string(),
});

/**
 * ${tableName} Form Component
 *
 * Form for creating/editing ${tableName} records
 */
export const ${tableName}Form = ({ initialData, onSubmit, onCancel }: ${tableName}FormProps) => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const formik = useFormik({
    initialValues: {
      name: initialData?.name || '',
      email: initialData?.email || '',
      phone: initialData?.phone || '',
      address: initialData?.address || '',
    },
    validationSchema,
    onSubmit: async (values) => {
      setLoading(true);
      setError(null);

      try {
        await onSubmit(values as ${tableName});
      } catch (err) {
        setError(err instanceof Error ? err.message : 'An error occurred');
      } finally {
        setLoading(false);
      }
    },
  });

  return (
    <Paper sx={{ p: 3 }}>
      <Typography variant="h5" gutterBottom>
        {initialData ? 'Edit' : 'Create'} ${tableName}
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <Box component="form" onSubmit={formik.handleSubmit}>
        <Grid container spacing={2}>
          <Grid item xs={12} sm={6}>
            <TextField
              fullWidth
              id="name"
              name="name"
              label="Name"
              value={formik.values.name}
              onChange={formik.handleChange}
              onBlur={formik.handleBlur}
              error={formik.touched.name && Boolean(formik.errors.name)}
              helperText={formik.touched.name && formik.errors.name}
            />
          </Grid>

          <Grid item xs={12} sm={6}>
            <TextField
              fullWidth
              id="email"
              name="email"
              label="Email"
              type="email"
              value={formik.values.email}
              onChange={formik.handleChange}
              onBlur={formik.handleBlur}
              error={formik.touched.email && Boolean(formik.errors.email)}
              helperText={formik.touched.email && formik.errors.email}
            />
          </Grid>

          <Grid item xs={12} sm={6}>
            <TextField
              fullWidth
              id="phone"
              name="phone"
              label="Phone"
              value={formik.values.phone}
              onChange={formik.handleChange}
              onBlur={formik.handleBlur}
              error={formik.touched.phone && Boolean(formik.errors.phone)}
              helperText={formik.touched.phone && formik.errors.phone}
            />
          </Grid>

          <Grid item xs={12} sm={6}>
            <TextField
              fullWidth
              id="address"
              name="address"
              label="Address"
              value={formik.values.address}
              onChange={formik.handleChange}
              onBlur={formik.handleBlur}
              error={formik.touched.address && Boolean(formik.errors.address)}
              helperText={formik.touched.address && formik.errors.address}
            />
          </Grid>

          <Grid item xs={12}>
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
              <Button
                variant="outlined"
                onClick={onCancel}
                disabled={loading}
              >
                Cancel
              </Button>
              <Button
                type="submit"
                variant="contained"
                color="primary"
                disabled={loading || !formik.isValid}
              >
                {loading ? <CircularProgress size={24} /> : 'Save'}
              </Button>
            </Box>
          </Grid>
        </Grid>
      </Box>
    </Paper>
  );
};

export default ${tableName}Form;
`;
};

/**
 * Generate a simpler example for quick testing
 */
export const generateSimpleReactComponent = () => {
  return `import { Box, Button, TextField, Typography } from '@mui/material';

export const SimpleForm = () => {
  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h5" gutterBottom>
        Contact Form
      </Typography>

      <TextField
        fullWidth
        label="Email"
        margin="normal"
      />

      <TextField
        fullWidth
        label="Message"
        multiline
        rows={4}
        margin="normal"
      />

      <Button
        variant="contained"
        color="primary"
        sx={{ mt: 2 }}
      >
        Send Message
      </Button>
    </Box>
  );
};
`;
};
