import { Box, Typography, Chip } from '@mui/material';
import KeyIcon from '@mui/icons-material/Key';
import LinkIcon from '@mui/icons-material/Link';
import type { Column } from '../../types/schema';

/**
 * Props for ColumnList component
 */
interface ColumnListProps {
  /** Array of columns to display */
  columns: Column[];
}

/**
 * ColumnList Component
 * 
 * Displays a list of database columns with their properties.
 * Shows primary key and foreign key indicators, data types, and nullable status.
 * 
 * @example
 * ```tsx
 * <ColumnList columns={table.columns} />
 * ```
 */
const ColumnList = ({ columns }: ColumnListProps) => {
  return (
    <Box>
      {columns.map((column) => (
        <Box
          key={column.name}
          sx={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            p: 1,
            borderBottom: '1px solid',
            borderColor: 'divider',
            '&:hover': {
              bgcolor: 'action.hover'
            }
          }}
        >
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flex: 1 }}>
            {column.isPrimaryKey && (
              <KeyIcon 
                fontSize="small" 
                color="primary"
                titleAccess="Primary Key"
              />
            )}
            {column.isForeignKey && (
              <LinkIcon 
                fontSize="small" 
                color="secondary"
                titleAccess={`Foreign Key → ${column.foreignKeyTable}.${column.foreignKeyColumn}`}
              />
            )}
            
            <Typography 
              variant="body2" 
              fontWeight={column.isPrimaryKey ? 'bold' : 'normal'}
              sx={{ fontFamily: 'monospace' }}
            >
              {column.name}
            </Typography>
          </Box>

          <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
            <Chip 
              label={column.maxLength ? `${column.type}(${column.maxLength})` : column.type}
              size="small" 
              variant="outlined"
              sx={{ fontFamily: 'monospace', minWidth: '80px' }}
            />
            {!column.nullable && (
              <Chip 
                label="NOT NULL" 
                size="small" 
                color="error"
                variant="outlined"
              />
            )}
            {column.defaultValue && (
              <Chip 
                label={`= ${column.defaultValue}`}
                size="small" 
                color="info"
                variant="outlined"
                sx={{ fontFamily: 'monospace' }}
              />
            )}
          </Box>
        </Box>
      ))}
    </Box>
  );
};

export default ColumnList;
