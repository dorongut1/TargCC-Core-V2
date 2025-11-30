import { useState } from 'react';
import { 
  Card, 
  CardHeader, 
  CardContent, 
  IconButton, 
  Collapse,
  Typography,
  Chip,
  Box
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import TableChartIcon from '@mui/icons-material/TableChart';
import type { Table } from '../../types/schema';
import ColumnList from './ColumnList';

/**
 * Props for TableCard component
 */
interface TableCardProps {
  /** Table data to display */
  table: Table;
  /** Whether the card is expanded by default */
  defaultExpanded?: boolean;
}

/**
 * TableCard Component
 * 
 * Displays a database table with expandable column details.
 * Shows table name, column count, row count, and TargCC status.
 * 
 * @example
 * ```tsx
 * <TableCard table={table} defaultExpanded={true} />
 * ```
 */
const TableCard = ({ table, defaultExpanded = true }: TableCardProps) => {
  const [expanded, setExpanded] = useState(defaultExpanded);

  return (
    <Card elevation={2}>
      <CardHeader
        avatar={<TableChartIcon color="primary" />}
        action={
          <IconButton
            onClick={() => setExpanded(!expanded)}
            aria-expanded={expanded}
            aria-label="show more"
            sx={{
              transform: expanded ? 'rotate(180deg)' : 'rotate(0deg)',
              transition: 'transform 0.3s'
            }}
          >
            <ExpandMoreIcon />
          </IconButton>
        }
        title={
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
            <Typography variant="h6" component="span" sx={{ fontFamily: 'monospace' }}>
              {table.schema}.{table.name}
            </Typography>
            {table.hasTargCCColumns && (
              <Chip 
                label="TargCC" 
                size="small" 
                color="primary"
                sx={{ fontWeight: 'bold' }}
              />
            )}
          </Box>
        }
        subheader={
          <Box sx={{ display: 'flex', gap: 1, mt: 0.5, flexWrap: 'wrap' }}>
            <Chip 
              label={`${table.columns.length} column${table.columns.length !== 1 ? 's' : ''}`}
              size="small" 
              variant="outlined"
            />
            {table.rowCount !== undefined && (
              <Chip 
                label={`${table.rowCount.toLocaleString()} row${table.rowCount !== 1 ? 's' : ''}`}
                size="small" 
                variant="outlined"
              />
            )}
          </Box>
        }
      />
      
      <Collapse in={expanded} timeout="auto" unmountOnExit>
        <CardContent sx={{ pt: 0 }}>
          <ColumnList columns={table.columns} />
        </CardContent>
      </Collapse>
    </Card>
  );
};

export default TableCard;
