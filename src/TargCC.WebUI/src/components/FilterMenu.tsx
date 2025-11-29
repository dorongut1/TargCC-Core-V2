import { useState } from 'react';
import {
  Box,
  Button,
  Popover,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Chip,
  IconButton,
  Typography,
  Divider,
  Stack
} from '@mui/material';
import { FilterList, Add, Clear } from '@mui/icons-material';

export interface FilterCriteria {
  id: string;
  field: string;
  operator: 'equals' | 'contains' | 'gt' | 'lt' | 'gte' | 'lte';
  value: string | number;
}

interface FilterMenuProps {
  availableFields: { value: string; label: string }[];
  filters: FilterCriteria[];
  onFiltersChange: (filters: FilterCriteria[]) => void;
}

const FilterMenu = ({ availableFields, filters, onFiltersChange }: FilterMenuProps) => {
  const [anchorEl, setAnchorEl] = useState<HTMLButtonElement | null>(null);
  const [newFilter, setNewFilter] = useState<Partial<FilterCriteria>>({
    field: '',
    operator: 'equals',
    value: ''
  });

  const operators = [
    { value: 'equals', label: 'Equals' },
    { value: 'contains', label: 'Contains' },
    { value: 'gt', label: 'Greater than' },
    { value: 'lt', label: 'Less than' },
    { value: 'gte', label: 'Greater or equal' },
    { value: 'lte', label: 'Less or equal' }
  ];

  const handleOpenMenu = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleCloseMenu = () => {
    setAnchorEl(null);
  };

  const handleAddFilter = () => {
    if (newFilter.field && newFilter.operator && newFilter.value !== '') {
      const filter: FilterCriteria = {
        id: `filter-${Date.now()}`,
        field: newFilter.field,
        operator: newFilter.operator as FilterCriteria['operator'],
        value: newFilter.value
      };
      
      onFiltersChange([...filters, filter]);
      
      // Reset form
      setNewFilter({
        field: '',
        operator: 'equals',
        value: ''
      });
    }
  };

  const handleRemoveFilter = (filterId: string) => {
    onFiltersChange(filters.filter(f => f.id !== filterId));
  };

  const handleClearAllFilters = () => {
    onFiltersChange([]);
  };

  const getFieldLabel = (fieldValue: string) => {
    return availableFields.find(f => f.value === fieldValue)?.label || fieldValue;
  };

  const getOperatorLabel = (operatorValue: string) => {
    return operators.find(o => o.value === operatorValue)?.label || operatorValue;
  };

  const formatFilterLabel = (filter: FilterCriteria) => {
    return `${getFieldLabel(filter.field)} ${getOperatorLabel(filter.operator)} "${filter.value}"`;
  };

  const open = Boolean(anchorEl);

  return (
    <Box>
      {/* Filter Button */}
      <Box display="flex" alignItems="center" gap={1} mb={2}>
        <Button
          variant="outlined"
          startIcon={<FilterList />}
          onClick={handleOpenMenu}
          size="small"
        >
          Filters {filters.length > 0 && `(${filters.length})`}
        </Button>

        {filters.length > 0 && (
          <Button
            variant="text"
            startIcon={<Clear />}
            onClick={handleClearAllFilters}
            size="small"
            color="error"
          >
            Clear All
          </Button>
        )}
      </Box>

      {/* Active Filters Chips */}
      {filters.length > 0 && (
        <Box display="flex" flexWrap="wrap" gap={1} mb={2}>
          {filters.map((filter) => (
            <Chip
              key={filter.id}
              label={formatFilterLabel(filter)}
              onDelete={() => handleRemoveFilter(filter.id)}
              size="small"
              color="primary"
              variant="outlined"
            />
          ))}
        </Box>
      )}

      {/* Filter Popover */}
      <Popover
        open={open}
        anchorEl={anchorEl}
        onClose={handleCloseMenu}
        anchorOrigin={{
          vertical: 'bottom',
          horizontal: 'left',
        }}
        transformOrigin={{
          vertical: 'top',
          horizontal: 'left',
        }}
      >
        <Box p={3} width={400}>
          <Typography variant="h6" gutterBottom>
            Add Filter
          </Typography>
          
          <Divider sx={{ mb: 2 }} />

          <Stack spacing={2}>
            {/* Field Selector */}
            <FormControl fullWidth size="small">
              <InputLabel>Field</InputLabel>
              <Select
                value={newFilter.field || ''}
                onChange={(e) => setNewFilter({ ...newFilter, field: e.target.value })}
                label="Field"
              >
                {availableFields.map((field) => (
                  <MenuItem key={field.value} value={field.value}>
                    {field.label}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>

            {/* Operator Selector */}
            <FormControl fullWidth size="small">
              <InputLabel>Operator</InputLabel>
              <Select
                value={newFilter.operator || 'equals'}
                onChange={(e) => setNewFilter({ ...newFilter, operator: e.target.value as FilterCriteria['operator'] })}
                label="Operator"
              >
                {operators.map((op) => (
                  <MenuItem key={op.value} value={op.value}>
                    {op.label}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>

            {/* Value Input */}
            <TextField
              fullWidth
              size="small"
              label="Value"
              value={newFilter.value || ''}
              onChange={(e) => setNewFilter({ ...newFilter, value: e.target.value })}
              onKeyDown={(e) => {
                if (e.key === 'Enter') {
                  handleAddFilter();
                }
              }}
            />

            {/* Action Buttons */}
            <Box display="flex" gap={1} justifyContent="flex-end" pt={1}>
              <Button
                variant="outlined"
                onClick={handleCloseMenu}
                size="small"
              >
                Cancel
              </Button>
              <Button
                variant="contained"
                startIcon={<Add />}
                onClick={handleAddFilter}
                disabled={!newFilter.field || newFilter.value === ''}
                size="small"
              >
                Add Filter
              </Button>
            </Box>
          </Stack>
        </Box>
      </Popover>
    </Box>
  );
};

export default FilterMenu;
