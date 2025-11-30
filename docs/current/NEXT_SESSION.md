# Next Session: Day 32 - Schema Designer Advanced Features

**Date:** Next Session  
**Phase:** 3C - Local Web UI  
**Day:** 32 of 45  
**Duration:** ~4-5 hours  
**Status:** Ready to Start

---

## 🎯 Day 32 Objectives

### Primary Goal
Enhance the schema designer with relationship visualization, statistics display, and export capabilities for comprehensive schema exploration.

### Specific Deliverables

1. **RelationshipGraph Component** (90 min)
   - Visual relationship diagram
   - Table boxes with connections
   - One-to-many indicators
   - Interactive hover tooltips

2. **SchemaStats Component** (60 min)
   - Statistics dashboard
   - Table and column counts
   - Data type distribution
   - Relationship metrics
   - TargCC usage percentage

3. **ExportMenu Component** (45 min)
   - Export as JSON
   - Export as SQL DDL
   - Export as Markdown docs
   - Download with proper filenames

4. **Advanced Filtering** (45 min)
   - Filter by TargCC tables
   - Filter by relationship presence
   - Combine multiple filters
   - Clear filters button

5. **Testing & Polish** (60 min)
   - 10-12 new tests
   - UI polish
   - Accessibility improvements

---

## 📋 Detailed Implementation Plan

### Part 1: SchemaStats Component (60 minutes)

#### 1.1 Create Stats Component

```typescript
// src/components/schema/SchemaStats.tsx

import { Paper, Grid, Typography, Box, LinearProgress } from '@mui/material';
import { DatabaseSchema } from '../../types/schema';

interface SchemaStatsProps {
  schema: DatabaseSchema;
}

const SchemaStats = ({ schema }: SchemaStatsProps) => {
  // Calculate statistics
  const totalTables = schema.tables.length;
  const totalColumns = schema.tables.reduce((sum, t) => sum + t.columns.length, 0);
  const totalRelationships = schema.relationships.length;
  const targccTables = schema.tables.filter(t => t.hasTargCCColumns).length;
  const avgColumnsPerTable = (totalColumns / totalTables).toFixed(1);

  // Data type distribution
  const dataTypes = schema.tables.flatMap(t => 
    t.columns.map(c => c.type)
  );
  const typeCount = dataTypes.reduce((acc, type) => {
    acc[type] = (acc[type] || 0) + 1;
    return acc;
  }, {} as Record<string, number>);

  return (
    <Paper sx={{ p: 3 }}>
      <Typography variant="h6" gutterBottom>
        Schema Statistics
      </Typography>
      
      <Grid container spacing={2}>
        <Grid item xs={6} md={3}>
          <StatCard label="Tables" value={totalTables} />
        </Grid>
        <Grid item xs={6} md={3}>
          <StatCard label="Columns" value={totalColumns} />
        </Grid>
        <Grid item xs={6} md={3}>
          <StatCard label="Relationships" value={totalRelationships} />
        </Grid>
        <Grid item xs={6} md={3}>
          <StatCard 
            label="TargCC Tables" 
            value={targccTables}
            subtitle={`${((targccTables/totalTables)*100).toFixed(0)}%`}
          />
        </Grid>
      </Grid>

      <Box sx={{ mt: 3 }}>
        <Typography variant="subtitle2" gutterBottom>
          Data Type Distribution
        </Typography>
        {Object.entries(typeCount)
          .sort(([,a], [,b]) => b - a)
          .slice(0, 5)
          .map(([type, count]) => (
            <Box key={type} sx={{ mb: 1 }}>
              <Typography variant="body2">
                {type}: {count} ({((count/totalColumns)*100).toFixed(1)}%)
              </Typography>
              <LinearProgress 
                variant="determinate" 
                value={(count/totalColumns)*100}
                sx={{ height: 6, borderRadius: 1 }}
              />
            </Box>
          ))
        }
      </Box>
    </Paper>
  );
};
```

---

### Part 2: RelationshipGraph Component (90 minutes)

#### 2.1 Simple SVG-Based Graph

```typescript
// src/components/schema/RelationshipGraph.tsx

import { Paper, Typography, Box } from '@mui/material';
import { DatabaseSchema, Relationship } from '../../types/schema';

interface RelationshipGraphProps {
  schema: DatabaseSchema;
}

const RelationshipGraph = ({ schema }: RelationshipGraphProps) => {
  const tablePositions = calculateTablePositions(schema.tables);
  
  return (
    <Paper sx={{ p: 3 }}>
      <Typography variant="h6" gutterBottom>
        Relationship Diagram
      </Typography>
      
      <Box sx={{ overflow: 'auto' }}>
        <svg width="800" height="600" style={{ border: '1px solid #e0e0e0' }}>
          {/* Draw relationships first (lines) */}
          {schema.relationships.map((rel, idx) => (
            <RelationshipLine
              key={idx}
              relationship={rel}
              positions={tablePositions}
            />
          ))}
          
          {/* Draw tables on top */}
          {schema.tables.map((table, idx) => (
            <TableBox
              key={table.name}
              table={table}
              position={tablePositions[table.name]}
            />
          ))}
        </svg>
      </Box>
    </Paper>
  );
};

function calculateTablePositions(tables: Table[]) {
  // Simple grid layout
  const cols = Math.ceil(Math.sqrt(tables.length));
  const spacing = { x: 180, y: 150 };
  
  return tables.reduce((acc, table, idx) => {
    acc[table.name] = {
      x: (idx % cols) * spacing.x + 50,
      y: Math.floor(idx / cols) * spacing.y + 50
    };
    return acc;
  }, {} as Record<string, {x: number, y: number}>);
}
```

---

### Part 3: ExportMenu Component (45 minutes)

#### 3.1 Export Utilities

```typescript
// src/utils/schemaExport.ts

import { DatabaseSchema } from '../types/schema';

export function exportAsJSON(schema: DatabaseSchema): string {
  return JSON.stringify(schema, null, 2);
}

export function exportAsSQL(schema: DatabaseSchema): string {
  let sql = '-- Database Schema DDL\\n\\n';
  
  schema.tables.forEach(table => {
    sql += `CREATE TABLE ${table.schema}.${table.name} (\\n`;
    
    const columns = table.columns.map(col => {
      let def = `  ${col.name} ${col.type}`;
      if (col.maxLength) def += `(${col.maxLength})`;
      if (!col.nullable) def += ' NOT NULL';
      if (col.defaultValue) def += ` DEFAULT ${col.defaultValue}`;
      if (col.isPrimaryKey) def += ' PRIMARY KEY';
      return def;
    });
    
    sql += columns.join(',\\n') + '\\n);\\n\\n';
  });
  
  return sql;
}

export function exportAsMarkdown(schema: DatabaseSchema): string {
  let md = '# Database Schema\\n\\n';
  
  schema.tables.forEach(table => {
    md += `## ${table.name}\\n\\n`;
    md += `**Schema:** ${table.schema}\\n`;
    if (table.rowCount) md += `**Rows:** ${table.rowCount.toLocaleString()}\\n`;
    md += `\\n### Columns\\n\\n`;
    md += '| Column | Type | Nullable | Keys |\\n';
    md += '|--------|------|----------|------|\\n';
    
    table.columns.forEach(col => {
      const keys = [];
      if (col.isPrimaryKey) keys.push('PK');
      if (col.isForeignKey) keys.push('FK');
      
      md += `| ${col.name} | ${col.type} | ${col.nullable ? 'Yes' : 'No'} | ${keys.join(', ')} |\\n`;
    });
    
    md += '\\n';
  });
  
  return md;
}
```

#### 3.2 Export Menu Component

```typescript
// src/components/schema/ExportMenu.tsx

import { useState } from 'react';
import { 
  IconButton, 
  Menu, 
  MenuItem, 
  ListItemIcon, 
  ListItemText 
} from '@mui/material';
import DownloadIcon from '@mui/icons-material/Download';
import CodeIcon from '@mui/icons-material/Code';
import DescriptionIcon from '@mui/icons-material/Description';
import { DatabaseSchema } from '../../types/schema';
import { exportAsJSON, exportAsSQL, exportAsMarkdown } from '../../utils/schemaExport';
import { downloadFile } from '../../utils/downloadCode';

interface ExportMenuProps {
  schema: DatabaseSchema;
}

const ExportMenu = ({ schema }: ExportMenuProps) => {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  
  const handleExport = (format: 'json' | 'sql' | 'md') => {
    let content: string;
    let filename: string;
    
    switch (format) {
      case 'json':
        content = exportAsJSON(schema);
        filename = 'schema.json';
        break;
      case 'sql':
        content = exportAsSQL(schema);
        filename = 'schema.sql';
        break;
      case 'md':
        content = exportAsMarkdown(schema);
        filename = 'schema.md';
        break;
    }
    
    downloadFile(content, filename);
    setAnchorEl(null);
  };
  
  return (
    <>
      <IconButton onClick={(e) => setAnchorEl(e.currentTarget)}>
        <DownloadIcon />
      </IconButton>
      
      <Menu
        anchorEl={anchorEl}
        open={Boolean(anchorEl)}
        onClose={() => setAnchorEl(null)}
      >
        <MenuItem onClick={() => handleExport('json')}>
          <ListItemIcon><CodeIcon /></ListItemIcon>
          <ListItemText>Export as JSON</ListItemText>
        </MenuItem>
        <MenuItem onClick={() => handleExport('sql')}>
          <ListItemIcon><CodeIcon /></ListItemIcon>
          <ListItemText>Export as SQL</ListItemText>
        </MenuItem>
        <MenuItem onClick={() => handleExport('md')}>
          <ListItemIcon><DescriptionIcon /></ListItemIcon>
          <ListItemText>Export as Markdown</ListItemText>
        </MenuItem>
      </Menu>
    </>
  );
};
```

---

### Part 4: Advanced Filtering (45 minutes)

#### 4.1 Update SchemaViewer with Filters

```typescript
// Add to SchemaViewer.tsx

const [filters, setFilters] = useState({
  targccOnly: false,
  withRelationships: false
});

const filteredTables = schema.tables.filter(table => {
  // Search filter
  const matchesSearch = 
    table.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    table.columns.some(col => 
      col.name.toLowerCase().includes(searchTerm.toLowerCase())
    );
  
  // TargCC filter
  const matchesTargCC = !filters.targccOnly || table.hasTargCCColumns;
  
  // Relationships filter
  const hasRelationship = schema.relationships.some(
    rel => rel.fromTable === table.name || rel.toTable === table.name
  );
  const matchesRelationships = !filters.withRelationships || hasRelationship;
  
  return matchesSearch && matchesTargCC && matchesRelationships;
});

// Add filter chips UI
<Box sx={{ mb: 2, display: 'flex', gap: 1 }}>
  <Chip
    label="TargCC Only"
    onClick={() => setFilters(prev => ({ ...prev, targccOnly: !prev.targccOnly }))}
    color={filters.targccOnly ? 'primary' : 'default'}
  />
  <Chip
    label="With Relationships"
    onClick={() => setFilters(prev => ({ ...prev, withRelationships: !prev.withRelationships }))}
    color={filters.withRelationships ? 'primary' : 'default'}
  />
  {(filters.targccOnly || filters.withRelationships) && (
    <Chip
      label="Clear Filters"
      onDelete={() => setFilters({ targccOnly: false, withRelationships: false })}
      variant="outlined"
    />
  )}
</Box>
```

---

### Part 5: Testing (60 minutes)

```typescript
// src/__tests__/schema/SchemaStats.test.tsx

describe('SchemaStats', () => {
  it('displays total table count', () => {
    // Test stats calculation
  });

  it('shows TargCC percentage', () => {
    // Test TargCC metrics
  });

  it('displays data type distribution', () => {
    // Test type distribution
  });
});

// src/__tests__/schema/ExportMenu.test.tsx

describe('ExportMenu', () => {
  it('exports as JSON', () => {
    // Test JSON export
  });

  it('exports as SQL', () => {
    // Test SQL export
  });

  it('exports as Markdown', () => {
    // Test MD export
  });
});

// src/__tests__/schema/RelationshipGraph.test.tsx

describe('RelationshipGraph', () => {
  it('renders table boxes', () => {
    // Test table rendering
  });

  it('draws relationship lines', () => {
    // Test relationship lines
  });
});
```

---

## 📁 Files to Create/Modify

### New Files
```
src/components/schema/
├── SchemaStats.tsx           (120 lines)
├── RelationshipGraph.tsx     (150 lines)
└── ExportMenu.tsx            (100 lines)

src/utils/
└── schemaExport.ts           (150 lines)

src/__tests__/schema/
├── SchemaStats.test.tsx       (70 lines)
├── RelationshipGraph.test.tsx (60 lines)
└── ExportMenu.test.tsx        (60 lines)
```

### Modified Files
```
src/components/schema/SchemaViewer.tsx  (+60 lines)
src/pages/Schema.tsx                    (+40 lines)
```

---

## ✅ Success Criteria

### Functionality
- [ ] Schema statistics displayed correctly
- [ ] Relationship graph shows all connections
- [ ] Export generates valid files
- [ ] Filters work independently and combined
- [ ] Download functionality works

### Testing
- [ ] 10-12 new tests written
- [ ] All components tested
- [ ] Export functions tested
- [ ] Build successful (dev)

### Code Quality
- [ ] TypeScript compliant
- [ ] Components under 200 lines
- [ ] Proper type definitions
- [ ] Clean, readable code
- [ ] No console warnings

### Documentation
- [ ] STATUS.md updated
- [ ] HANDOFF.md for Day 33
- [ ] Phase3_Checklist.md updated

---

## 🚀 Getting Started

### 1. Development Order
1. Create schemaExport utilities
2. Create SchemaStats component
3. Create ExportMenu component
4. Create RelationshipGraph component
5. Add filters to SchemaViewer
6. Update Schema page layout
7. Write tests
8. Polish UI
9. Update docs

---

## 💡 Tips for Success

### Statistics Display
- Keep metrics prominent
- Use color coding
- Show percentages alongside counts
- Consider pie/bar charts for distribution

### Relationship Graph
- Start with simple boxes and lines
- Position tables in grid
- Use arrows to show direction
- Add tooltips for details

### Export Functionality
- Generate clean, readable output
- Include comments in SQL
- Format JSON with indentation
- Test with different schemas

### Filtering
- Make filters discoverable
- Show active filter count
- Allow easy clearing
- Combine filters logically

---

## 📞 Quick Commands

```bash
# Start dev server
npm run dev

# Run tests
npm test -- --run src/__tests__/schema

# Type check
npx tsc --noEmit

# Schema page URL
http://localhost:5177/schema
```

---

**Ready to Start:** ✅  
**Estimated Duration:** 4-5 hours  
**Expected Output:** Advanced schema features with stats, graph, and export  
**Next Day:** Day 33 - Continue Web UI Features

---

**Created:** 01/12/2025 21:30  
**Status:** Ready for Day 32! 🚀
