import { TableRow, TableCell, Skeleton } from '@mui/material';

interface TableSkeletonProps {
  /** Number of rows to display */
  rows?: number;
  /** Number of columns to display */
  columns?: number;
}

/**
 * Loading skeleton for table rows.
 * Displays placeholder rows with skeleton cells.
 */
const TableSkeleton = ({ rows = 10, columns = 6 }: TableSkeletonProps) => {
  return (
    <>
      {Array.from({ length: rows }).map((_, rowIndex) => (
        <TableRow key={rowIndex}>
          {Array.from({ length: columns }).map((_, colIndex) => (
            <TableCell key={colIndex}>
              <Skeleton
                variant="text"
                width={colIndex === 0 ? '80%' : '90%'}
                height={20}
              />
            </TableCell>
          ))}
        </TableRow>
      ))}
    </>
  );
};

export default TableSkeleton;
