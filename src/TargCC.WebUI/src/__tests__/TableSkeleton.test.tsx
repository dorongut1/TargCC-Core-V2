import { describe, it, expect } from 'vitest';
import { render } from '@testing-library/react';
import { Table, TableBody } from '@mui/material';
import TableSkeleton from '../components/TableSkeleton';

describe('TableSkeleton', () => {
  it('renders without crashing', () => {
    const { container } = render(
      <Table>
        <TableBody>
          <TableSkeleton />
        </TableBody>
      </Table>
    );
    
    expect(container.querySelector('.MuiTableRow-root')).toBeInTheDocument();
  });

  it('renders default 10 rows', () => {
    const { container } = render(
      <Table>
        <TableBody>
          <TableSkeleton />
        </TableBody>
      </Table>
    );
    
    const rows = container.querySelectorAll('.MuiTableRow-root');
    expect(rows).toHaveLength(10);
  });

  it('renders custom number of rows', () => {
    const { container } = render(
      <Table>
        <TableBody>
          <TableSkeleton rows={5} />
        </TableBody>
      </Table>
    );
    
    const rows = container.querySelectorAll('.MuiTableRow-root');
    expect(rows).toHaveLength(5);
  });

  it('renders default 6 columns per row', () => {
    const { container } = render(
      <Table>
        <TableBody>
          <TableSkeleton />
        </TableBody>
      </Table>
    );
    
    const firstRow = container.querySelector('.MuiTableRow-root');
    const cells = firstRow?.querySelectorAll('.MuiTableCell-root');
    expect(cells).toHaveLength(6);
  });

  it('renders custom number of columns', () => {
    const { container } = render(
      <Table>
        <TableBody>
          <TableSkeleton columns={4} />
        </TableBody>
      </Table>
    );
    
    const firstRow = container.querySelector('.MuiTableRow-root');
    const cells = firstRow?.querySelectorAll('.MuiTableCell-root');
    expect(cells).toHaveLength(4);
  });

  it('renders skeleton in each cell', () => {
    const { container } = render(
      <Table>
        <TableBody>
          <TableSkeleton rows={3} columns={3} />
        </TableBody>
      </Table>
    );
    
    const skeletons = container.querySelectorAll('.MuiSkeleton-root');
    expect(skeletons).toHaveLength(9); // 3 rows × 3 columns
  });

  it('uses text variant for skeletons', () => {
    const { container } = render(
      <Table>
        <TableBody>
          <TableSkeleton />
        </TableBody>
      </Table>
    );
    
    const textSkeletons = container.querySelectorAll('.MuiSkeleton-text');
    expect(textSkeletons.length).toBeGreaterThan(0);
  });

  it('renders correct structure for large tables', () => {
    const { container } = render(
      <Table>
        <TableBody>
          <TableSkeleton rows={20} columns={8} />
        </TableBody>
      </Table>
    );
    
    const rows = container.querySelectorAll('.MuiTableRow-root');
    const skeletons = container.querySelectorAll('.MuiSkeleton-root');
    
    expect(rows).toHaveLength(20);
    expect(skeletons).toHaveLength(160); // 20 × 8
  });

  it('handles edge case of 1 row and 1 column', () => {
    const { container } = render(
      <Table>
        <TableBody>
          <TableSkeleton rows={1} columns={1} />
        </TableBody>
      </Table>
    );
    
    const rows = container.querySelectorAll('.MuiTableRow-root');
    const cells = container.querySelectorAll('.MuiTableCell-root');
    const skeletons = container.querySelectorAll('.MuiSkeleton-root');
    
    expect(rows).toHaveLength(1);
    expect(cells).toHaveLength(1);
    expect(skeletons).toHaveLength(1);
  });

  it('all cells contain skeletons', () => {
    const { container } = render(
      <Table>
        <TableBody>
          <TableSkeleton rows={5} columns={4} />
        </TableBody>
      </Table>
    );
    
    const cells = container.querySelectorAll('.MuiTableCell-root');
    
    cells.forEach(cell => {
      const skeleton = cell.querySelector('.MuiSkeleton-root');
      expect(skeleton).toBeInTheDocument();
    });
  });
});
