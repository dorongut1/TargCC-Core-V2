import React, { useState, useEffect, useMemo } from 'react';
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
  Checkbox,
  Menu,
  MenuItem,
  TableSortLabel,
} from '@mui/material';
import {
  Search as SearchIcon,
  PlayArrow as GenerateIcon,
  Visibility as ViewIcon,
  Edit as EditIcon,
  Refresh as RefreshIcon,
  MoreVert as MoreIcon,
} from '@mui/icons-material';
import { apiService } from '../services/api';
import type { Table as TableModel } from '../types/models';
import Pagination from '../components/Pagination';
import FilterMenu from '../components/FilterMenu';
import type { FilterCriteria } from '../components/FilterMenu';

type SortField = 'name' | 'schema' | 'rowCount' | 'lastGenerated';
type SortDirection = 'asc' | 'desc';

interface SortConfig {
  field: SortField;
  direction: SortDirection;
}

/**
 * Tables page component - displays list of database tables
 */
export const Tables: React.FC = () => {
  const [tables, setTables] = useState<TableModel[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [sortConfig, setSortConfig] = useState<SortConfig>({ field: 'name', direction: 'asc' });
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(25);
  const [selectedRows, setSelectedRows] = useState<Set<string>>(new Set());
  const [bulkMenuAnchor, setBulkMenuAnchor] = useState<null | HTMLElement>(null);
  const [filters, setFilters] = useState<FilterCriteria[]>([]);

  // Available filter fields
  const filterFields = [
    { value: 'name', label: 'Table Name' },
    { value: 'schema', label: 'Schema' },
    { value: 'rowCount', label: 'Row Count' },
    { value: 'generationStatus', label: 'Status' }
  ];

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

  // Apply filters
  const applyFilters = (table: TableModel): boolean => {
    if (filters.length === 0) return true;

    return filters.every(filter => {
      const value = table[filter.field as keyof TableModel];
      const filterValue = filter.value;

      switch (filter.operator) {
        case 'equals':
          return String(value).toLowerCase() === String(filterValue).toLowerCase();
        case 'contains':
          return String(value).toLowerCase().includes(String(filterValue).toLowerCase());
        case 'gt':
          return Number(value) > Number(filterValue);
        case 'lt':
          return Number(value) < Number(filterValue);
        case 'gte':
          return Number(value) >= Number(filterValue);
        case 'lte':
          return Number(value) <= Number(filterValue);
        default:
          return true;
      }
    });
  };

  // Filter, sort and paginate tables
  const processedTables = useMemo(() => {
    // 1. Filter by search
    let result = tables.filter(
      (table) =>
        table.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        table.schema.toLowerCase().includes(searchTerm.toLowerCase())
    );

    // 2. Apply advanced filters
    result = result.filter(applyFilters);

    // 3. Sort
    result.sort((a, b) => {
      const aValue = a[sortConfig.field];
      const bValue = b[sortConfig.field];

      if (aValue === null || aValue === undefined) return 1;
      if (bValue === null || bValue === undefined) return -1;

      let comparison = 0;
      if (typeof aValue === 'string' && typeof bValue === 'string') {
        comparison = aValue.localeCompare(bValue);
      } else if (typeof aValue === 'number' && typeof bValue === 'number') {
        comparison = aValue - bValue;
      } else if (aValue instanceof Date && bValue instanceof Date) {
        comparison = aValue.getTime() - bValue.getTime();
      }

      return sortConfig.direction === 'asc' ? comparison : -comparison;
    });

    return result;
  }, [tables, searchTerm, sortConfig, filters]);

  // Paginated tables
  const paginatedTables = useMemo(() => {
    const startIndex = (page - 1) * pageSize;
    return processedTables.slice(startIndex, startIndex + pageSize);
  }, [processedTables, page, pageSize]);

  // Handle sort
  const handleSort = (field: SortField) => {
    setSortConfig(prev => ({
      field,
      direction: prev.field === field && prev.direction === 'asc' ? 'desc' : 'asc'
    }));
  };

  // Handle row selection
  const handleSelectRow = (tableKey: string) => {
    const newSelected = new Set(selectedRows);
    if (newSelected.has(tableKey)) {
      newSelected.delete(tableKey);
    } else {
      newSelected.add(tableKey);
    }
    setSelectedRows(newSelected);
  };

  const handleSelectAll = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (event.target.checked) {
      const allKeys = paginatedTables.map(t => `${t.schema}.${t.name}`);
      setSelectedRows(new Set(allKeys));
    } else {
      setSelectedRows(new Set());
    }
  };

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
      await loadTables();
    } catch (err) {
      console.error('Generate failed:', err);
    }
  };

  const handleBulkGenerate = async () => {
    // Bulk generate for selected tables
    setBulkMenuAnchor(null);
    console.log('Generating for:', selectedRows);
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

  const isAllSelected = paginatedTables.length > 0 && 
    paginatedTables.every(t => selectedRows.has(`${t.schema}.${t.name}`));
  const isSomeSelected = paginatedTables.some(t => selectedRows.has(`${t.schema}.${t.name}`));

  return (
    <Box>
      {/* Header */}
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4" component="h1">
          Database Tables
        </Typography>
        <Box display="flex" gap={2}>
          {selectedRows.size > 0 && (
            <Button
              variant="outlined"
              startIcon={<MoreIcon />}
              onClick={(e) => setBulkMenuAnchor(e.currentTarget)}
            >
              Bulk Actions ({selectedRows.size})
            </Button>
          )}
          <Button
            variant="outlined"
            startIcon={<RefreshIcon />}
            onClick={loadTables}
          >
            Refresh
          </Button>
        </Box>
      </Box>

      {/* Search and Filters */}
      <Box mb={3} display="flex" gap={2}>
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

      {/* Advanced Filters */}
      <FilterMenu
        availableFields={filterFields}
        filters={filters}
        onFiltersChange={setFilters}
      />

      {/* Stats */}
      <Box mb={3} display="flex" gap={2}>
        <Chip
          label={`Total: ${tables.length}`}
          color="primary"
          variant="outlined"
        />
        <Chip
          label={`Showing: ${processedTables.length}`}
          color="info"
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
              <TableCell padding="checkbox">
                <Checkbox
                  checked={isAllSelected}
                  indeterminate={isSomeSelected && !isAllSelected}
                  onChange={handleSelectAll}
                />
              </TableCell>
              <TableCell>
                <TableSortLabel
                  active={sortConfig.field === 'name'}
                  direction={sortConfig.field === 'name' ? sortConfig.direction : 'asc'}
                  onClick={() => handleSort('name')}
                >
                  Name
                </TableSortLabel>
              </TableCell>
              <TableCell>
                <TableSortLabel
                  active={sortConfig.field === 'schema'}
                  direction={sortConfig.field === 'schema' ? sortConfig.direction : 'asc'}
                  onClick={() => handleSort('schema')}
                >
                  Schema
                </TableSortLabel>
              </TableCell>
              <TableCell>
                <TableSortLabel
                  active={sortConfig.field === 'rowCount'}
                  direction={sortConfig.field === 'rowCount' ? sortConfig.direction : 'asc'}
                  onClick={() => handleSort('rowCount')}
                >
                  Rows
                </TableSortLabel>
              </TableCell>
              <TableCell>Status</TableCell>
              <TableCell>
                <TableSortLabel
                  active={sortConfig.field === 'lastGenerated'}
                  direction={sortConfig.field === 'lastGenerated' ? sortConfig.direction : 'asc'}
                  onClick={() => handleSort('lastGenerated')}
                >
                  Last Generated
                </TableSortLabel>
              </TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {paginatedTables.length === 0 ? (
              <TableRow>
                <TableCell colSpan={7} align="center">
                  <Typography color="text.secondary" py={4}>
                    {searchTerm || filters.length > 0 ? 'No tables found matching your criteria' : 'No tables available'}
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              paginatedTables.map((table) => {
                const tableKey = `${table.schema}.${table.name}`;
                const isSelected = selectedRows.has(tableKey);

                return (
                  <TableRow key={tableKey} hover selected={isSelected}>
                    <TableCell padding="checkbox">
                      <Checkbox
                        checked={isSelected}
                        onChange={() => handleSelectRow(tableKey)}
                      />
                    </TableCell>
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
                );
              })
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Pagination */}
      {processedTables.length > 0 && (
        <Pagination
          total={processedTables.length}
          page={page}
          pageSize={pageSize}
          onPageChange={setPage}
          onPageSizeChange={setPageSize}
        />
      )}

      {/* Bulk Actions Menu */}
      <Menu
        anchorEl={bulkMenuAnchor}
        open={Boolean(bulkMenuAnchor)}
        onClose={() => setBulkMenuAnchor(null)}
      >
        <MenuItem onClick={handleBulkGenerate}>
          <GenerateIcon fontSize="small" sx={{ mr: 1 }} />
          Generate Selected
        </MenuItem>
        <MenuItem onClick={() => { setSelectedRows(new Set()); setBulkMenuAnchor(null); }}>
          <IconButton size="small" sx={{ mr: 1 }}>✓</IconButton>
          Clear Selection
        </MenuItem>
      </Menu>
    </Box>
  );
};
