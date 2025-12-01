import { useState, useEffect } from 'react';
import {
  Box,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Checkbox,
  TextField,
  Typography,
  Button,
  CircularProgress,
  Alert
} from '@mui/material';
import { TableChart as TableIcon } from '@mui/icons-material';
import type { WizardData } from './GenerationWizard';
import { useConnection } from '../../hooks/useConnection';
import { apiService } from '../../services/api';

interface TableSelectionProps {
  data: WizardData;
  onChange: (data: WizardData) => void;
}

const TableSelection = ({ data, onChange }: TableSelectionProps) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [allTables, setAllTables] = useState<string[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const { selectedConnection } = useConnection();

  // Fetch tables from API
  useEffect(() => {
    const fetchTables = async () => {
      if (!selectedConnection) {
        setError('No active connection. Please connect to a database first.');
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setError(null);
        const tables = await apiService.getTables(); // Uses default 'dbo' schema
        setAllTables(tables.map(t => t.name));
      } catch (err) {
        console.error('Error fetching tables:', err);
        setError(err instanceof Error ? err.message : 'Failed to fetch tables');
        // Fallback to empty array
        setAllTables([]);
      } finally {
        setLoading(false);
      }
    };

    fetchTables();
  }, [selectedConnection]);

  const filteredTables = allTables.filter((table) =>
    table.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleToggle = (table: string) => {
    const newSelection = data.selectedTables.includes(table)
      ? data.selectedTables.filter((t) => t !== table)
      : [...data.selectedTables, table];

    onChange({ ...data, selectedTables: newSelection });
  };

  const handleSelectAll = () => {
    onChange({ ...data, selectedTables: filteredTables });
  };

  const handleSelectNone = () => {
    onChange({ ...data, selectedTables: [] });
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: 200 }}>
        <CircularProgress />
        <Typography variant="body2" sx={{ ml: 2 }}>Loading tables...</Typography>
      </Box>
    );
  }

  if (error) {
    return (
      <Box>
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
        <Typography variant="body2" color="text.secondary">
          Please ensure you have an active database connection before using the Generation Wizard.
        </Typography>
      </Box>
    );
  }

  if (allTables.length === 0) {
    return (
      <Box>
        <Alert severity="warning" sx={{ mb: 2 }}>
          No tables found in the database.
        </Alert>
        <Typography variant="body2" color="text.secondary">
          The connected database appears to be empty or you don't have permissions to view tables.
        </Typography>
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h6" gutterBottom>
        Select Tables to Generate
      </Typography>

      <Typography variant="body2" color="text.secondary" paragraph>
        Choose one or more tables for code generation ({allTables.length} tables available)
      </Typography>

      <Box sx={{ mb: 2, display: 'flex', gap: 2 }}>
        <TextField
          fullWidth
          placeholder="Search tables..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          size="small"
        />
        <Button onClick={handleSelectAll} variant="outlined" size="small">
          Select All
        </Button>
        <Button onClick={handleSelectNone} variant="outlined" size="small">
          Select None
        </Button>
      </Box>

      <List sx={{ maxHeight: 300, overflow: 'auto', border: '1px solid #e0e0e0', borderRadius: 1 }}>
        {filteredTables.length === 0 ? (
          <ListItem>
            <ListItemText primary="No tables found" secondary="Try a different search term" />
          </ListItem>
        ) : (
          filteredTables.map((table) => (
            <ListItem key={table} disablePadding>
              <ListItemButton onClick={() => handleToggle(table)} dense>
                <ListItemIcon>
                  <Checkbox
                    checked={data.selectedTables.includes(table)}
                    tabIndex={-1}
                    disableRipple
                  />
                </ListItemIcon>
                <ListItemIcon>
                  <TableIcon />
                </ListItemIcon>
                <ListItemText primary={table} />
              </ListItemButton>
            </ListItem>
          ))
        )}
      </List>

      <Typography variant="caption" color="text.secondary" sx={{ mt: 2, display: 'block' }}>
        {data.selectedTables.length} table(s) selected
      </Typography>
    </Box>
  );
};

export default TableSelection;
