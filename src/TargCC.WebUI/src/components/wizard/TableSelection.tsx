import { useState } from 'react';
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
  Button
} from '@mui/material';
import { TableChart as TableIcon } from '@mui/icons-material';
import type { WizardData } from './GenerationWizard';

interface TableSelectionProps {
  data: WizardData;
  onChange: (data: WizardData) => void;
}

const TableSelection = ({ data, onChange }: TableSelectionProps) => {
  const [searchTerm, setSearchTerm] = useState('');

  // Mock table data - in real app, this would come from API
  const allTables = [
    'Customer',
    'Order',
    'Product',
    'Employee',
    'Invoice',
    'Category',
    'Supplier',
    'Inventory'
  ];

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

  return (
    <Box>
      <Typography variant="h6" gutterBottom>
        Select Tables to Generate
      </Typography>

      <Typography variant="body2" color="text.secondary" paragraph>
        Choose one or more tables for code generation
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
