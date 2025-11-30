import { useState } from 'react';
import { 
  Box, 
  TextField, 
  Typography,
  InputAdornment,
  Chip,
  Paper,
  Stack
} from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import ClearIcon from '@mui/icons-material/Clear';
import type { DatabaseSchema } from '../../types/schema';
import TableCard from './TableCard';

/**
 * Props for SchemaViewer component
 */
interface SchemaViewerProps {
  /** Database schema to display */
  schema: DatabaseSchema;
}

/**
 * SchemaViewer Component
 * 
 * Main schema visualization component that displays all database tables
 * with search and filter capabilities.
 * 
 * Features:
 * - Responsive grid layout
 * - Real-time search filtering
 * - Table and column search
 * - TargCC column highlighting
 * 
 * @example
 * ```tsx
 * <SchemaViewer schema={mockSchema} />
 * ```
 */
const SchemaViewer = ({ schema }: SchemaViewerProps) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [filters, setFilters] = useState({
    targccOnly: false,
    withRelationships: false,
  });

  // Filter tables based on search term and filters
  const filteredTables = schema.tables.filter((table) => {
    // Search filter
    const matchesSearch =
      table.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      table.columns.some((col) =>
        col.name.toLowerCase().includes(searchTerm.toLowerCase())
      );

    // TargCC filter
    const matchesTargCC = !filters.targccOnly || table.hasTargCCColumns;

    // Relationships filter
    const hasRelationship = schema.relationships.some(
      (rel) => rel.fromTable === table.name || rel.toTable === table.name
    );
    const matchesRelationships = !filters.withRelationships || hasRelationship;

    return matchesSearch && matchesTargCC && matchesRelationships;
  });

  const targccCount = schema.tables.filter((t) => t.hasTargCCColumns).length;
  const hasActiveFilters = filters.targccOnly || filters.withRelationships;

  /**
   * Toggle TargCC filter
   */
  const toggleTargCCFilter = () => {
    setFilters((prev) => ({ ...prev, targccOnly: !prev.targccOnly }));
  };

  /**
   * Toggle relationships filter
   */
  const toggleRelationshipsFilter = () => {
    setFilters((prev) => ({ ...prev, withRelationships: !prev.withRelationships }));
  };

  /**
   * Clear all filters
   */
  const clearFilters = () => {
    setFilters({ targccOnly: false, withRelationships: false });
  };

  return (
    <Box>
      {/* Header */}
      <Paper sx={{ p: 3, mb: 3 }} elevation={1}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <Typography variant="h4">
            Database Schema
          </Typography>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Chip 
              label={`${schema.tables.length} table${schema.tables.length !== 1 ? 's' : ''}`}
              color="primary"
            />
            {targccCount > 0 && (
              <Chip 
                label={`${targccCount} TargCC`}
                color="secondary"
              />
            )}
          </Box>
        </Box>

        {/* Search */}
        <TextField
          fullWidth
          placeholder="Search tables and columns..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon />
              </InputAdornment>
            ),
          }}
          sx={{ mb: 2 }}
        />

        {/* Filters */}
        <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
          <Chip
            label="TargCC Only"
            onClick={toggleTargCCFilter}
            color={filters.targccOnly ? 'primary' : 'default'}
            variant={filters.targccOnly ? 'filled' : 'outlined'}
          />
          <Chip
            label="With Relationships"
            onClick={toggleRelationshipsFilter}
            color={filters.withRelationships ? 'primary' : 'default'}
            variant={filters.withRelationships ? 'filled' : 'outlined'}
          />
          {hasActiveFilters && (
            <Chip
              label="Clear Filters"
              onClick={clearFilters}
              onDelete={clearFilters}
              deleteIcon={<ClearIcon />}
              variant="outlined"
              color="secondary"
            />
          )}
        </Stack>
      </Paper>

      {/* Table Grid */}
      {filteredTables.length > 0 ? (
        <Box sx={{ 
          display: 'grid', 
          gridTemplateColumns: {
            xs: '1fr',
            md: 'repeat(2, 1fr)',
            lg: 'repeat(auto-fill, minmax(450px, 1fr))'
          },
          gap: 2 
        }}>
          {filteredTables.map((table, index) => (
            <TableCard 
              key={table.name} 
              table={table}
              defaultExpanded={index === 0}
            />
          ))}
        </Box>
      ) : (
        <Paper sx={{ p: 4, textAlign: 'center' }} elevation={1}>
          <Typography variant="h6" color="text.secondary">
            No tables found
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
            No tables match your search: "{searchTerm}"
          </Typography>
        </Paper>
      )}
    </Box>
  );
};

export default SchemaViewer;
