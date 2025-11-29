import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  IconButton,
  Chip,
  TextField,
  InputAdornment,
  Button,
  Tooltip,
  CircularProgress,
  Alert,
} from '@mui/material';
import {
  Search as SearchIcon,
  PlayArrow as GenerateIcon,
  Visibility as ViewIcon,
  Edit as EditIcon,
  Refresh as RefreshIcon,
} from '@mui/icons-material';
import { apiService } from '../services/api';
import type { Table as TableModel } from '../types/models';

/**
 * Tables page component - displays list of database tables
 */
export const Tables: React.FC = () => {
  const [tables, setTables] = useState<TableModel[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');

  // Load tables on mount
  useEffect(() => {
    loadTables();
  }, []);

  const loadTables = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await apiService.getTables();
      setTables(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load tables');
    } finally {
      setLoading(false);
    }
  };

  // Filter tables based on search
  const filteredTables = tables.filter(
    (table) =>
      table.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      table.schema.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleGenerate = async (tableName: string) => {
    try {
      await apiService.generateCode({
        tableName,
        options: {
          generateEntity: true,
          generateRepository: true,
          generateService: true,
          generateController: true,
          generateTests: true,
          overwriteExisting: false,
        },
      });
      await loadTables(); // Reload to update status
    } catch (err) {
      console.error('Generate failed:', err);
    }
  };

  const getStatusColor = (status: string): 'success' | 'warning' | 'error' | 'default' => {
    switch (status) {
      case 'Generated':
        return 'success';
      case 'Modified':
        return 'warning';
      case 'Error':
        return 'error';
      default:
        return 'default';
    }
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Box p={3}>
        <Alert severity="error" action={
          <Button color="inherit" size="small" onClick={loadTables}>
            Retry
          </Button>
        }>
          {error}
        </Alert>
      </Box>
    );
  }

  return (
    <Box>
      {/* Header */}
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4" component="h1">
          Database Tables
        </Typography>
        <Button
          variant="outlined"
          startIcon={<RefreshIcon />}
          onClick={loadTables}
        >
          Refresh
        </Button>
      </Box>

      {/* Search */}
      <Box mb={3}>
        <TextField
          fullWidth
          placeholder="Search tables..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon />
              </InputAdornment>
            ),
          }}
        />
      </Box>

      {/* Stats */}
      <Box mb={3} display="flex" gap={2}>
        <Chip
          label={`Total: ${tables.length}`}
          color="primary"
          variant="outlined"
        />
        <Chip
          label={`Generated: ${tables.filter(t => t.generationStatus === 'Generated').length}`}
          color="success"
          variant="outlined"
        />
        <Chip
          label={`Not Generated: ${tables.filter(t => t.generationStatus === 'Not Generated').length}`}
          color="default"
          variant="outlined"
        />
      </Box>

      {/* Table */}
      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Name</TableCell>
              <TableCell>Schema</TableCell>
              <TableCell>Rows</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Last Generated</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {filteredTables.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} align="center">
                  <Typography color="text.secondary" py={4}>
                    {searchTerm ? 'No tables found matching your search' : 'No tables available'}
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              filteredTables.map((table) => (
                <TableRow key={`${table.schema}.${table.name}`} hover>
                  <TableCell>
                    <Typography fontWeight="medium">{table.name}</Typography>
                  </TableCell>
                  <TableCell>{table.schema}</TableCell>
                  <TableCell>{table.rowCount?.toLocaleString() ?? 'N/A'}</TableCell>
                  <TableCell>
                    <Chip
                      label={table.generationStatus}
                      color={getStatusColor(table.generationStatus)}
                      size="small"
                    />
                  </TableCell>
                  <TableCell>
                    {table.lastGenerated
                      ? new Date(table.lastGenerated).toLocaleDateString()
                      : 'Never'}
                  </TableCell>
                  <TableCell align="right">
                    <Tooltip title="Generate Code">
                      <IconButton
                        size="small"
                        onClick={() => handleGenerate(table.name)}
                        color="primary"
                      >
                        <GenerateIcon />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="View Details">
                      <IconButton size="small" color="default">
                        <ViewIcon />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Edit Configuration">
                      <IconButton size="small" color="default">
                        <EditIcon />
                      </IconButton>
                    </Tooltip>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Footer */}
      <Box mt={2} display="flex" justifyContent="space-between" alignItems="center">
        <Typography variant="body2" color="text.secondary">
          Showing {filteredTables.length} of {tables.length} tables
        </Typography>
      </Box>
    </Box>
  );
};
