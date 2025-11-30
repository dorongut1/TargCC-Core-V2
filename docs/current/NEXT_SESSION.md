# Next Session: Day 34 - Enhanced Features & Polish

**Date:** Next Session  
**Phase:** 3C - Local Web UI  
**Day:** 34 of 45  
**Duration:** ~4-6 hours  
**Status:** Ready to Start

---

## 🎯 Day 34 Objectives

### Primary Goal
Enhance the schema integration with multi-database support, performance improvements, and polish the user experience.

### Specific Deliverables

1. **Database Connection Manager** (90 min)
   - UI for managing connections
   - Add/edit/delete connections
   - Connection testing
   - Persist connections

2. **Database Selector** (60 min)
   - Dropdown to switch databases
   - Recently used list
   - Current database indicator
   - Smooth switching

3. **Enhanced Schema Features** (90 min)
   - Table preview with sample data
   - Column statistics
   - Index information
   - Performance metrics

4. **Performance Improvements** (60 min)
   - Schema caching layer
   - Lazy loading optimization
   - Virtual scrolling
   - Debounced search

5. **Testing & Polish** (60 min)
   - API integration tests
   - Hook tests
   - UI polish
   - Documentation

---

## 📋 Detailed Implementation Plan

### Part 1: Database Connection Manager (90 minutes)

#### 1.1 Create Connection API (Backend)

```csharp
// Services/IConnectionService.cs

public interface IConnectionService
{
    Task<List<ConnectionInfo>> GetConnectionsAsync();
    Task<ConnectionInfo> GetConnectionAsync(string id);
    Task<ConnectionInfo> AddConnectionAsync(ConnectionInfo connection);
    Task UpdateConnectionAsync(ConnectionInfo connection);
    Task DeleteConnectionAsync(string id);
    Task<bool> TestConnectionAsync(string connectionString);
}

// Models/ConnectionInfo.cs

public class ConnectionInfo
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Server { get; set; }
    public string Database { get; set; }
    public string ConnectionString { get; set; }
    public DateTime LastUsed { get; set; }
    public DateTime Created { get; set; }
}
```

#### 1.2 Create Connection Manager UI (Frontend)

```typescript
// src/components/schema/ConnectionManager.tsx

import { Dialog, DialogTitle, DialogContent, List, ListItem, IconButton } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import CheckIcon from '@mui/icons-material/Check';

interface Connection {
  id: string;
  name: string;
  server: string;
  database: string;
  lastUsed: Date;
}

interface ConnectionManagerProps {
  open: boolean;
  onClose: () => void;
  onSelect: (connection: Connection) => void;
}

const ConnectionManager = ({ open, onClose, onSelect }: ConnectionManagerProps) => {
  const [connections, setConnections] = useState<Connection[]>([]);
  const [testing, setTesting] = useState<string | null>(null);

  const handleTest = async (connection: Connection) => {
    setTesting(connection.id);
    try {
      // Test connection
      const result = await testConnection(connection);
      // Show success
    } catch (error) {
      // Show error
    } finally {
      setTesting(null);
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>
        Database Connections
        <IconButton onClick={() => {/* Open add dialog */}}>
          <AddIcon />
        </IconButton>
      </DialogTitle>
      <DialogContent>
        <List>
          {connections.map(conn => (
            <ListItem key={conn.id}>
              <Box sx={{ flex: 1 }}>
                <Typography variant="h6">{conn.name}</Typography>
                <Typography variant="body2" color="text.secondary">
                  {conn.server} / {conn.database}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  Last used: {formatLastUsed(conn.lastUsed)}
                </Typography>
              </Box>
              <IconButton onClick={() => handleTest(conn)}>
                <CheckIcon />
              </IconButton>
              <IconButton onClick={() => {/* Edit */}}>
                <EditIcon />
              </IconButton>
              <IconButton onClick={() => {/* Delete */}}>
                <DeleteIcon />
              </IconButton>
            </ListItem>
          ))}
        </List>
      </DialogContent>
    </Dialog>
  );
};
```

---

### Part 2: Database Selector (60 minutes)

```typescript
// src/components/schema/DatabaseSelector.tsx

import { Select, MenuItem, Chip, Box } from '@mui/material';
import StorageIcon from '@mui/icons-material/Storage';

interface DatabaseSelectorProps {
  currentDatabase: string;
  databases: string[];
  onSelect: (database: string) => void;
  onManage: () => void;
}

const DatabaseSelector = ({ 
  currentDatabase, 
  databases, 
  onSelect, 
  onManage 
}: DatabaseSelectorProps) => {
  return (
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
      <Chip
        icon={<StorageIcon />}
        label="Database"
        size="small"
        color="primary"
        variant="outlined"
      />
      <Select
        value={currentDatabase}
        onChange={(e) => onSelect(e.target.value)}
        size="small"
        sx={{ minWidth: 200 }}
      >
        {databases.map(db => (
          <MenuItem key={db} value={db}>
            {db}
          </MenuItem>
        ))}
        <MenuItem onClick={onManage}>
          <em>Manage Connections...</em>
        </MenuItem>
      </Select>
    </Box>
  );
};
```

---

### Part 3: Schema Caching (60 minutes)

```typescript
// src/hooks/useSchemaCache.ts

interface CacheEntry<T> {
  data: T;
  timestamp: number;
  expiresAt: number;
}

const CACHE_TTL = 5 * 60 * 1000; // 5 minutes

class SchemaCache {
  private cache: Map<string, CacheEntry<any>> = new Map();

  set<T>(key: string, data: T, ttl: number = CACHE_TTL): void {
    this.cache.set(key, {
      data,
      timestamp: Date.now(),
      expiresAt: Date.now() + ttl,
    });
  }

  get<T>(key: string): T | null {
    const entry = this.cache.get(key);
    if (!entry) return null;
    
    if (Date.now() > entry.expiresAt) {
      this.cache.delete(key);
      return null;
    }
    
    return entry.data as T;
  }

  invalidate(key: string): void {
    this.cache.delete(key);
  }

  clear(): void {
    this.cache.clear();
  }
}

export const schemaCache = new SchemaCache();

// Update useSchema to use cache
export function useSchema(schemaName: string | null): UseSchemaResult {
  // ... existing code ...

  const loadSchema = useCallback(async (name: string) => {
    // Check cache first
    const cached = schemaCache.get<DatabaseSchema>(`schema:${name}`);
    if (cached) {
      setSchema(cached);
      setLastUpdated(new Date());
      setIsConnected(true);
      setLoading(false);
      return;
    }

    // Load from API if not cached
    setLoading(true);
    try {
      const data = await fetchSchemaDetails(name);
      schemaCache.set(`schema:${name}`, data);
      setSchema(data);
      // ... rest of code
    } catch (error) {
      // ... error handling
    }
  }, []);

  // Refresh bypasses cache
  const refresh = useCallback(async () => {
    if (!schemaName) return;
    schemaCache.invalidate(`schema:${schemaName}`);
    await loadSchema(schemaName);
  }, [schemaName, loadSchema]);

  // ... return
}
```

---

### Part 4: Table Preview (90 minutes)

```typescript
// src/components/schema/TablePreview.tsx

interface TablePreviewProps {
  tableName: string;
  schemaName: string;
}

const TablePreview = ({ tableName, schemaName }: TablePreviewProps) => {
  const [data, setData] = useState<any[]>([]);
  const [columns, setColumns] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadPreview();
  }, [tableName, schemaName]);

  const loadPreview = async () => {
    setLoading(true);
    try {
      // API call to get TOP 10 rows
      const response = await fetch(
        `${API_CONFIG.baseUrl}/api/schema/${schemaName}/${tableName}/preview`
      );
      const result = await response.json();
      setData(result.data);
      setColumns(result.columns);
    } catch (error) {
      console.error('Preview error:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Paper sx={{ p: 2 }}>
      <Typography variant="h6" gutterBottom>
        Table Preview: {tableName}
        <Chip label="Top 10 rows" size="small" sx={{ ml: 1 }} />
      </Typography>
      
      {loading ? (
        <CircularProgress />
      ) : (
        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow>
                {columns.map(col => (
                  <TableCell key={col}>{col}</TableCell>
                ))}
              </TableRow>
            </TableHead>
            <TableBody>
              {data.map((row, idx) => (
                <TableRow key={idx}>
                  {columns.map(col => (
                    <TableCell key={col}>{row[col]}</TableCell>
                  ))}
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Paper>
  );
};
```

---

## 📁 Files to Create/Modify

### New Files (Backend)
```
Services/IConnectionService.cs           (40 lines)
Services/ConnectionService.cs           (150 lines)
Models/ConnectionInfo.cs                 (30 lines)
```

### New Files (Frontend)
```
src/components/schema/ConnectionManager.tsx      (180 lines)
src/components/schema/DatabaseSelector.tsx       (80 lines)
src/components/schema/TablePreview.tsx          (150 lines)
src/hooks/useSchemaCache.ts                     (100 lines)
src/hooks/useConnections.ts                     (120 lines)
src/api/connectionApi.ts                        (100 lines)
```

### Modified Files
```
src/pages/Schema.tsx                     (+100 lines)
Program.cs                                (+80 lines)
```

### Test Files
```
src/__tests__/api/connectionApi.test.ts          (80 lines)
src/__tests__/hooks/useConnections.test.ts       (70 lines)
src/__tests__/hooks/useSchemaCache.test.ts       (60 lines)
```

---

## ✅ Success Criteria

### Functionality
- [ ] Can add/edit/delete connections
- [ ] Can switch between databases
- [ ] Connection testing works
- [ ] Schema caching reduces API calls
- [ ] Table preview displays sample data
- [ ] All existing features still work

### Performance
- [ ] Schema loads in <1 second (from cache)
- [ ] Database switching is smooth
- [ ] No UI freezing
- [ ] Debounced search reduces lag

### Testing
- [ ] 8-12 new tests written
- [ ] Connection CRUD tested
- [ ] Cache logic tested
- [ ] Build successful

### Code Quality
- [ ] TypeScript compliant
- [ ] C# StyleCop compliant
- [ ] Proper error handling
- [ ] No console warnings

### Documentation
- [ ] STATUS.md updated
- [ ] HANDOFF.md for Day 35
- [ ] API docs updated
- [ ] PROGRESS.md updated

---

## 🚀 Getting Started

### 1. Verify Current State
```bash
# Backend should be running
cd C:\Disk1\TargCC-Core-V2\src\TargCC.WebAPI
dotnet run

# Frontend should be running
cd C:\Disk1\TargCC-Core-V2\src\TargCC.WebUI
npm run dev

# Verify Day 33 works
# Visit http://localhost:5179/schema
# Should see live data from TargCCOrdersNew
```

### 2. Development Order
1. Create connection models and service (Backend)
2. Add connection endpoints (Backend)
3. Create connection API client (Frontend)
4. Build ConnectionManager UI (Frontend)
5. Build DatabaseSelector component (Frontend)
6. Add caching layer (Frontend)
7. Create TablePreview component (Frontend)
8. Test everything
9. Write tests
10. Update docs

---

## 💡 Tips for Success

### Connection Management
- Store connections in localStorage (client-side)
- Or persist in database (server-side)
- Encrypt sensitive data
- Test before saving

### Caching Strategy
- Cache for 5 minutes by default
- Invalidate on manual refresh
- Clear cache on connection switch
- Show cache age in UI

### Database Selector
- Show in page header
- Dropdown with icons
- Remember last selection
- Quick access to manager

### Performance
- Lazy load components
- Virtual scrolling for large lists
- Debounce search (300ms)
- Show loading skeletons

---

## 📞 Quick Commands

```bash
# Start dev environment
cd C:\Disk1\TargCC-Core-V2\src\TargCC.WebAPI
dotnet run

cd C:\Disk1\TargCC-Core-V2\src\TargCC.WebUI
npm run dev

# Run tests
npm test -- --run
dotnet test

# Type check
npx tsc --noEmit

# Build
npm run build
dotnet build
```

---

**Ready to Start:** ✅  
**Estimated Duration:** 4-6 hours  
**Expected Output:** Multi-database support with enhanced features  
**Next Day:** Day 35 - Generation Integration

---

**Created:** 01/12/2025  
**Status:** Ready for Day 34! 🚀
