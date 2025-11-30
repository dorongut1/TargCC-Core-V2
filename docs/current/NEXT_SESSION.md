# Next Session: Day 31 - Schema Designer Foundation

**Date:** Next Session  
**Phase:** 3C - Local Web UI  
**Day:** 31 of 45  
**Duration:** ~3-4 hours  
**Status:** Ready to Start

---

## 🎯 Day 31 Objectives

### Primary Goal
Create a visual schema designer that displays database structure with tables, columns, relationships, and interactive exploration features.

### Specific Deliverables

1. **SchemaViewer Component** (90 min)
   - Visual table display
   - Column list with types
   - Primary/Foreign key indicators
   - Search and filter
   - Responsive grid layout

2. **TableCard Component** (45 min)
   - Table name header
   - Column details
   - Relationship indicators
   - Expandable sections
   - Action buttons

3. **ColumnList Component** (30 min)
   - Column names and types
   - Nullable indicators
   - Key icons
   - Type badges

4. **Mock Schema Data** (30 min)
   - Sample database structure
   - Multiple tables with relationships
   - Realistic column definitions
   - Primary/Foreign keys

5. **Testing & Polish** (45 min)
   - 8-10 new tests
   - UI polish
   - Accessibility improvements

---

## 📋 Detailed Implementation Plan

### Part 1: Mock Schema Data (30 minutes)

#### 1.1 Create Schema Types

```typescript
// src/types/schema.ts

export interface Column {
  name: string;
  type: string;
  nullable: boolean;
  isPrimaryKey: boolean;
  isForeignKey: boolean;
  foreignKeyTable?: string;
  foreignKeyColumn?: string;
  maxLength?: number;
  defaultValue?: string;
}

export interface Table {
  name: string;
  schema: string;
  columns: Column[];
  rowCount?: number;
  hasTargCCColumns: boolean;
}

export interface DatabaseSchema {
  tables: Table[];
  relationships: Relationship[];
}

export interface Relationship {
  fromTable: string;
  fromColumn: string;
  toTable: string;
  toColumn: string;
  type: 'one-to-one' | 'one-to-many' | 'many-to-many';
}
```

#### 1.2 Create Mock Schema

```typescript
// src/utils/mockSchema.ts

import { DatabaseSchema } from '../types/schema';

export const mockSchema: DatabaseSchema = {
  tables: [
    {
      name: 'Customer',
      schema: 'dbo',
      rowCount: 1250,
      hasTargCCColumns: true,
      columns: [
        { name: 'CustomerId', type: 'int', nullable: false, isPrimaryKey: true, isForeignKey: false },
        { name: 'eno_FirstName', type: 'nvarchar', nullable: false, isPrimaryKey: false, isForeignKey: false, maxLength: 50 },
        { name: 'eno_LastName', type: 'nvarchar', nullable: false, isPrimaryKey: false, isForeignKey: false, maxLength: 50 },
        { name: 'eno_Email', type: 'nvarchar', nullable: true, isPrimaryKey: false, isForeignKey: false, maxLength: 100 },
        { name: 'ent_CreatedDate', type: 'datetime2', nullable: false, isPrimaryKey: false, isForeignKey: false },
      ]
    },
    {
      name: 'Order',
      schema: 'dbo',
      rowCount: 5430,
      hasTargCCColumns: true,
      columns: [
        { name: 'OrderId', type: 'int', nullable: false, isPrimaryKey: true, isForeignKey: false },
        { name: 'CustomerId', type: 'int', nullable: false, isPrimaryKey: false, isForeignKey: true, foreignKeyTable: 'Customer', foreignKeyColumn: 'CustomerId' },
        { name: 'OrderDate', type: 'datetime2', nullable: false, isPrimaryKey: false, isForeignKey: false },
        { name: 'clc_TotalAmount', type: 'decimal', nullable: false, isPrimaryKey: false, isForeignKey: false },
        { name: 'ent_ModifiedDate', type: 'datetime2', nullable: true, isPrimaryKey: false, isForeignKey: false },
      ]
    },
    // Add more tables...
  ],
  relationships: [
    {
      fromTable: 'Order',
      fromColumn: 'CustomerId',
      toTable: 'Customer',
      toColumn: 'CustomerId',
      type: 'many-to-one'
    },
    // Add more relationships...
  ]
};
```

---

### Part 2: ColumnList Component (30 minutes)

```typescript
// src/components/schema/ColumnList.tsx

import { Box, Typography, Chip } from '@mui/material';
import KeyIcon from '@mui/icons-material/Key';
import LinkIcon from '@mui/icons-material/Link';
import { Column } from '../../types/schema';

interface ColumnListProps {
  columns: Column[];
}

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
            {column.isPrimaryKey && <KeyIcon fontSize="small" color="primary" />}
            {column.isForeignKey && <LinkIcon fontSize="small" color="secondary" />}
            
            <Typography variant="body2" fontWeight={column.isPrimaryKey ? 'bold' : 'normal'}>
              {column.name}
            </Typography>
          </Box>

          <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
            <Chip 
              label={column.type} 
              size="small" 
              variant="outlined"
            />
            {!column.nullable && (
              <Chip 
                label="NOT NULL" 
                size="small" 
                color="error"
                variant="outlined"
              />
            )}
          </Box>
        </Box>
      ))}
    </Box>
  );
};

export default ColumnList;
```

---

### Part 3: TableCard Component (45 minutes)

```typescript
// src/components/schema/TableCard.tsx

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
import { Table } from '../../types/schema';
import ColumnList from './ColumnList';

interface TableCardProps {
  table: Table;
}

const TableCard = ({ table }: TableCardProps) => {
  const [expanded, setExpanded] = useState(true);

  return (
    <Card elevation={2}>
      <CardHeader
        avatar={<TableChartIcon />}
        action={
          <IconButton
            onClick={() => setExpanded(!expanded)}
            sx={{
              transform: expanded ? 'rotate(180deg)' : 'rotate(0deg)',
              transition: 'transform 0.3s'
            }}
          >
            <ExpandMoreIcon />
          </IconButton>
        }
        title={
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Typography variant="h6">{table.name}</Typography>
            {table.hasTargCCColumns && (
              <Chip label="TargCC" size="small" color="primary" />
            )}
          </Box>
        }
        subheader={
          <Box sx={{ display: 'flex', gap: 1, mt: 0.5 }}>
            <Chip 
              label={`${table.columns.length} columns`} 
              size="small" 
              variant="outlined"
            />
            {table.rowCount && (
              <Chip 
                label={`${table.rowCount.toLocaleString()} rows`} 
                size="small" 
                variant="outlined"
              />
            )}
          </Box>
        }
      />
      
      <Collapse in={expanded}>
        <CardContent sx={{ pt: 0 }}>
          <ColumnList columns={table.columns} />
        </CardContent>
      </Collapse>
    </Card>
  );
};

export default TableCard;
```

---

### Part 4: SchemaViewer Component (90 minutes)

```typescript
// src/components/schema/SchemaViewer.tsx

import { useState } from 'react';
import { 
  Box, 
  TextField, 
  Typography,
  InputAdornment,
  Chip
} from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import { DatabaseSchema } from '../../types/schema';
import TableCard from './TableCard';

interface SchemaViewerProps {
  schema: DatabaseSchema;
}

const SchemaViewer = ({ schema }: SchemaViewerProps) => {
  const [searchTerm, setSearchTerm] = useState('');

  const filteredTables = schema.tables.filter(table =>
    table.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    table.columns.some(col => 
      col.name.toLowerCase().includes(searchTerm.toLowerCase())
    )
  );

  return (
    <Box>
      <Box sx={{ mb: 3, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Typography variant="h5">
          Database Schema
        </Typography>
        <Chip 
          label={`${schema.tables.length} tables`}
          color="primary"
        />
      </Box>

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
        sx={{ mb: 3 }}
      />

      <Box sx={{ 
        display: 'grid', 
        gridTemplateColumns: 'repeat(auto-fill, minmax(400px, 1fr))', 
        gap: 2 
      }}>
        {filteredTables.map((table) => (
          <TableCard key={table.name} table={table} />
        ))}
      </Box>

      {filteredTables.length === 0 && (
        <Typography variant="body1" color="text.secondary" textAlign="center" sx={{ mt: 4 }}>
          No tables found matching "{searchTerm}"
        </Typography>
      )}
    </Box>
  );
};

export default SchemaViewer;
```

---

### Part 5: Testing (45 minutes)

```typescript
// src/__tests__/schema/SchemaViewer.test.tsx

describe('SchemaViewer', () => {
  it('renders all tables', () => {
    // Test rendering
  });

  it('filters tables by search term', () => {
    // Test search
  });

  it('shows table count', () => {
    // Test count display
  });
});

// src/__tests__/schema/TableCard.test.tsx

describe('TableCard', () => {
  it('renders table name', () => {
    // Test name
  });

  it('expands and collapses', () => {
    // Test expand/collapse
  });

  it('shows TargCC badge when applicable', () => {
    // Test badge
  });
});

// src/__tests__/schema/ColumnList.test.tsx

describe('ColumnList', () => {
  it('renders all columns', () => {
    // Test columns
  });

  it('shows primary key icons', () => {
    // Test PK icons
  });

  it('shows foreign key icons', () => {
    // Test FK icons
  });
});
```

---

## 📁 Files to Create/Modify

### New Files
```
src/types/
└── schema.ts (100 lines)

src/utils/
└── mockSchema.ts (200 lines)

src/components/schema/
├── SchemaViewer.tsx (120 lines)
├── TableCard.tsx (100 lines)
└── ColumnList.tsx (80 lines)

src/pages/
└── Schema.tsx (40 lines)

src/__tests__/schema/
├── SchemaViewer.test.tsx (60 lines)
├── TableCard.test.tsx (50 lines)
└── ColumnList.test.tsx (40 lines)
```

### Modified Files
```
src/App.tsx
└── (+1 route for /schema)
```

---

## ✅ Success Criteria

### Functionality
- [ ] Schema viewer displays all tables
- [ ] Search filters tables/columns
- [ ] Cards expand/collapse smoothly
- [ ] Primary/Foreign keys clearly marked
- [ ] TargCC columns highlighted
- [ ] Row counts displayed

### Testing
- [ ] 8-10 new tests written
- [ ] Schema viewer tested
- [ ] Table card tested
- [ ] Column list tested
- [ ] Build successful (dev)

### Code Quality
- [ ] TypeScript compliant
- [ ] Components under 150 lines
- [ ] Proper type definitions
- [ ] Clean, readable code
- [ ] No console warnings

### Documentation
- [ ] STATUS.md updated
- [ ] HANDOFF.md for Day 32
- [ ] Phase3_Checklist.md updated

---

## 🚀 Getting Started

### 1. Development Order
1. Create schema types
2. Create mock schema data
3. Create ColumnList component
4. Create TableCard component
5. Create SchemaViewer component
6. Add route to App
7. Write tests
8. Polish UI
9. Update docs

---

## 💡 Tips for Success

### Schema Display
- Use cards for clean separation
- Make search instant (no debounce needed for small datasets)
- Expand first table by default
- Use consistent icons

### Type Definitions
- Keep types in separate file
- Make them reusable
- Add comments for complex types
- Export from index if needed

### Mock Data
- Keep it realistic
- Include various column types
- Add relationships
- Use TargCC prefixes

### UI/UX
- Grid layout for responsiveness
- Smooth expand/collapse animations
- Clear visual hierarchy
- Good use of whitespace

---

## 📞 Quick Commands

```bash
# Start dev server
npm run dev

# Run tests
npm test

# Type check
npx tsc --noEmit

# Schema page URL
http://localhost:5174/schema
```

---

**Ready to Start:** ✅  
**Estimated Duration:** 3-4 hours  
**Expected Output:** Visual schema designer with interactive exploration  
**Next Day:** Day 32 - Schema Designer Advanced Features

---

**Created:** 01/12/2025  
**Status:** Ready for Day 31! 🚀
