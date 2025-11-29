import { Box, Pagination as MuiPagination, Select, MenuItem, Typography, TextField } from '@mui/material';
import { useState } from 'react';

interface PaginationProps {
  total: number;
  page: number;
  pageSize: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (size: number) => void;
}

const Pagination = ({
  total,
  page,
  pageSize,
  onPageChange,
  onPageSizeChange
}: PaginationProps) => {
  const [jumpToPage, setJumpToPage] = useState('');
  
  const totalPages = Math.ceil(total / pageSize);
  const startItem = total === 0 ? 0 : (page - 1) * pageSize + 1;
  const endItem = Math.min(page * pageSize, total);

  const handlePageSizeChange = (event: any) => {
    const newSize = Number(event.target.value);
    onPageSizeChange(newSize);
    // Reset to page 1 when changing page size
    onPageChange(1);
  };

  const handleJumpToPage = (event: React.KeyboardEvent<HTMLInputElement>) => {
    if (event.key === 'Enter') {
      const pageNum = Number(jumpToPage);
      if (pageNum >= 1 && pageNum <= totalPages) {
        onPageChange(pageNum);
        setJumpToPage('');
      }
    }
  };

  if (total === 0) {
    return null;
  }

  return (
    <Box 
      display="flex" 
      alignItems="center" 
      justifyContent="space-between"
      flexWrap="wrap"
      gap={2}
      py={2}
    >
      {/* Left side - Page size selector */}
      <Box display="flex" alignItems="center" gap={1}>
        <Typography variant="body2" color="text.secondary">
          Rows per page:
        </Typography>
        <Select
          value={pageSize}
          onChange={handlePageSizeChange}
          size="small"
          sx={{ minWidth: 70 }}
        >
          <MenuItem value={10}>10</MenuItem>
          <MenuItem value={25}>25</MenuItem>
          <MenuItem value={50}>50</MenuItem>
          <MenuItem value={100}>100</MenuItem>
        </Select>
      </Box>

      {/* Center - Pagination controls */}
      <Box display="flex" alignItems="center" gap={2}>
        <MuiPagination
          count={totalPages}
          page={page}
          onChange={(_, value) => onPageChange(value)}
          color="primary"
          showFirstButton
          showLastButton
          siblingCount={1}
          boundaryCount={1}
        />
      </Box>

      {/* Right side - Jump to page and info */}
      <Box display="flex" alignItems="center" gap={2}>
        <Box display="flex" alignItems="center" gap={1}>
          <Typography variant="body2" color="text.secondary">
            Jump to:
          </Typography>
          <TextField
            size="small"
            type="number"
            value={jumpToPage}
            onChange={(e) => setJumpToPage(e.target.value)}
            onKeyDown={handleJumpToPage}
            placeholder="Page"
            inputProps={{
              min: 1,
              max: totalPages,
              style: { width: '60px', textAlign: 'center' }
            }}
            sx={{
              '& input::-webkit-outer-spin-button, & input::-webkit-inner-spin-button': {
                display: 'none'
              }
            }}
          />
        </Box>
        
        <Typography variant="body2" color="text.secondary">
          {startItem}-{endItem} of {total}
        </Typography>
      </Box>
    </Box>
  );
};

export default Pagination;
