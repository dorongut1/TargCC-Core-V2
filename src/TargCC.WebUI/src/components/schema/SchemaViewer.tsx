import { useState } from 'react';
import { 
  Box, 
  TextField, 
  Typography,
  InputAdornment,
  Chip,
  Paper
} from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
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

  // Filter tables based on search term
  const filteredTables = schema.tables.filter(table =>
    table.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    table.columns.some(col => 
      col.name.toLowerCase().includes(searchTerm.toLowerCase())
    )
  );

  const targccCount = schema.tables.filter(t => t.hasTargCCColumns).length;

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
            )
          }}
        />
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
