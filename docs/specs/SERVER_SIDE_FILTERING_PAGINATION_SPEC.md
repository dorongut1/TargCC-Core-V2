# Server-Side Filtering & Pagination - Specification Document

## 📋 Document Information

- **Feature:** Server-Side Filtering & Pagination
- **Version:** 1.0
- **Date:** 2025-12-15
- **Author:** Claude (TargCC Core Team)
- **Status:** Approved for Development

---

## 🎯 Executive Summary

### Problem Statement

Currently, the generated code loads **all records** from the database and performs filtering/sorting/pagination on the client-side. This approach:

- ❌ Fails with large datasets (10,000+ records)
- ❌ Causes browser memory issues
- ❌ Results in slow initial page loads
- ❌ Wastes network bandwidth
- ❌ Poor user experience with loading spinners

**Example of current behavior:**
```typescript
// Loads ALL 50,000 customers into browser memory!
const { data: allCustomers } = useCustomers();
const filtered = allCustomers.filter(c => c.name.includes(search)); // Client-side filtering
```

### Solution

Implement **server-side filtering, sorting, and pagination** throughout the entire stack:

- ✅ Backend builds dynamic SQL queries based on filters
- ✅ Only requested page of data is returned (e.g., 100 records instead of 50,000)
- ✅ Sorting happens in database (optimized with indexes)
- ✅ Pagination handled by database (OFFSET/FETCH)
- ✅ Filter state preserved in URL query parameters

**Example of new behavior:**
```typescript
// Loads only 100 customers from page 1 with filters applied
const { data } = useCustomers({
  filters: { name: 'John', status: 'Active' },
  page: 1,
  pageSize: 100,
  sortBy: 'createdAt',
  sortDirection: 'desc'
});
```

### Success Criteria

1. ✅ Page loads under 2 seconds even with 1M+ records in database
2. ✅ Browser memory usage stays under 100MB regardless of dataset size
3. ✅ Filters/sorting/pagination state preserved in URL (shareable links)
4. ✅ Backward compatible - existing generated code continues to work
5. ✅ All existing tests pass + new tests added

---

## 🏗️ Architecture Overview

### Data Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                         USER INTERFACE                          │
│  ┌────────────┐  ┌────────────┐  ┌────────────┐  ┌──────────┐ │
│  │  Filters   │  │   Sort     │  │ Pagination │  │  Search  │ │
│  └─────┬──────┘  └──────┬─────┘  └──────┬─────┘  └────┬─────┘ │
│        │                 │               │             │       │
│        └─────────────────┴───────────────┴─────────────┘       │
│                              ▼                                  │
│                    ┌──────────────────┐                         │
│                    │  useEntities()   │                         │
│                    │   React Hook     │                         │
│                    └────────┬─────────┘                         │
└─────────────────────────────┼─────────────────────────────────┘
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                         FRONTEND API                            │
│                    ┌──────────────────┐                         │
│                    │  entityApi.ts    │                         │
│                    │ GET /api/entities│                         │
│                    │  ?page=1         │                         │
│                    │  &pageSize=100   │                         │
│                    │  &name=John      │                         │
│                    │  &sortBy=id      │                         │
│                    └────────┬─────────┘                         │
└─────────────────────────────┼─────────────────────────────────┘
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      BACKEND API LAYER                          │
│                    ┌──────────────────┐                         │
│                    │ EntitiesController│                        │
│                    │   [HttpGet]      │                         │
│                    └────────┬─────────┘                         │
│                             ▼                                   │
│                    ┌──────────────────┐                         │
│                    │   MediatR        │                         │
│                    │ Send(GetEntities │                         │
│                    │      Query)      │                         │
│                    └────────┬─────────┘                         │
└─────────────────────────────┼─────────────────────────────────┘
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    APPLICATION LAYER (CQRS)                     │
│                    ┌──────────────────┐                         │
│                    │GetEntitiesQuery  │                         │
│                    │  Handler         │                         │
│                    │                  │                         │
│                    │ Builds dynamic   │                         │
│                    │ EF Core query    │                         │
│                    │ with Where(),    │                         │
│                    │ OrderBy(),       │                         │
│                    │ Skip(), Take()   │                         │
│                    └────────┬─────────┘                         │
└─────────────────────────────┼─────────────────────────────────┘
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    INFRASTRUCTURE LAYER                         │
│                    ┌──────────────────┐                         │
│                    │   Repository     │                         │
│                    │ GetPagedAsync()  │                         │
│                    └────────┬─────────┘                         │
│                             ▼                                   │
│                    ┌──────────────────┐                         │
│                    │   EF Core /      │                         │
│                    │   Dapper         │                         │
│                    └────────┬─────────┘                         │
└─────────────────────────────┼─────────────────────────────────┘
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                         DATABASE                                │
│   SELECT * FROM Customers                                       │
│   WHERE Name LIKE '%John%' AND Status = 'Active'               │
│   ORDER BY CreatedAt DESC                                       │
│   OFFSET 0 ROWS FETCH NEXT 100 ROWS ONLY                       │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📦 Components to Generate

### 1. Backend Components

#### A. Shared Models (Domain Layer)

**File:** `Domain/Common/PagedResult.cs`
```csharp
public class PagedResult<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
```

**File:** `Domain/Common/FilterOperator.cs`
```csharp
public enum FilterOperator
{
    Equals,
    NotEquals,
    Contains,
    StartsWith,
    EndsWith,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    In,
    NotIn,
    IsNull,
    IsNotNull,
    Between
}
```

**File:** `Domain/Common/FilterCriteria.cs`
```csharp
public class FilterCriteria
{
    public string Field { get; set; }
    public FilterOperator Operator { get; set; }
    public object Value { get; set; }
}
```

**File:** `Domain/Common/PaginationParams.cs`
```csharp
public class PaginationParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 100;
    public string SortBy { get; set; }
    public string SortDirection { get; set; } = "asc";
}
```

#### B. Updated CQRS Queries

**File:** `Application/Customers/Queries/GetCustomersQuery.cs`
```csharp
public class GetCustomersQuery : IRequest<PagedResult<CustomerDto>>
{
    // Filters - auto-generated from CustomerFilters type
    public CustomerFilters Filters { get; set; }

    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 100;

    // Sorting
    public string SortBy { get; set; } = "Id";
    public string SortDirection { get; set; } = "asc";
}
```

**File:** `Application/Customers/Queries/GetCustomersQueryHandler.cs`
```csharp
public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, PagedResult<CustomerDto>>
{
    public async Task<PagedResult<CustomerDto>> Handle(GetCustomersQuery request, ...)
    {
        var query = _context.Customers.AsQueryable();

        // Apply filters dynamically
        query = ApplyFilters(query, request.Filters);

        // Get total count BEFORE pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortDirection);

        // Apply pagination
        query = query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize);

        var items = await query.ToListAsync();
        var dtos = _mapper.Map<List<CustomerDto>>(items);

        return new PagedResult<CustomerDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    private IQueryable<Customer> ApplyFilters(IQueryable<Customer> query, CustomerFilters filters)
    {
        if (filters == null) return query;

        // Auto-generated for each filter property
        if (!string.IsNullOrEmpty(filters.Name))
            query = query.Where(c => c.Name.Contains(filters.Name));

        if (!string.IsNullOrEmpty(filters.Email))
            query = query.Where(c => c.Email.Contains(filters.Email));

        if (filters.Status != null)
            query = query.Where(c => c.Status == filters.Status);

        // Date range filters
        if (filters.CreatedAtFrom.HasValue)
            query = query.Where(c => c.CreatedAt >= filters.CreatedAtFrom.Value);

        if (filters.CreatedAtTo.HasValue)
            query = query.Where(c => c.CreatedAt <= filters.CreatedAtTo.Value);

        return query;
    }

    private IQueryable<Customer> ApplySorting(IQueryable<Customer> query, string sortBy, string direction)
    {
        // Dynamic sorting based on property name
        return sortBy?.ToLower() switch
        {
            "name" => direction == "desc" ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "email" => direction == "desc" ? query.OrderByDescending(c => c.Email) : query.OrderBy(c => c.Email),
            "createdat" => direction == "desc" ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => query.OrderBy(c => c.Id) // Default sort
        };
    }
}
```

#### C. Updated API Controllers

**File:** `API/Controllers/CustomersController.cs`
```csharp
[HttpGet]
public async Task<ActionResult<PagedResult<CustomerDto>>> GetAll(
    [FromQuery] CustomerFilters filters,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 100,
    [FromQuery] string sortBy = "Id",
    [FromQuery] string sortDirection = "asc")
{
    var query = new GetCustomersQuery
    {
        Filters = filters,
        Page = page,
        PageSize = pageSize,
        SortBy = sortBy,
        SortDirection = sortDirection
    };

    var result = await _mediator.Send(query);
    return Ok(result);
}
```

### 2. Frontend Components

#### A. TypeScript Types

**File:** `client/src/types/common.types.ts`
```typescript
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface PaginationParams {
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface UseEntityOptions<TFilters> {
  filters?: TFilters;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
  enabled?: boolean;
}
```

#### B. Updated API Functions

**File:** `client/src/api/customerApi.ts`
```typescript
export const customerApi = {
  getAll: async (options?: UseCustomersOptions): Promise<PagedResult<Customer>> => {
    const params = new URLSearchParams();

    // Add pagination
    if (options?.page) params.append('page', options.page.toString());
    if (options?.pageSize) params.append('pageSize', options.pageSize.toString());
    if (options?.sortBy) params.append('sortBy', options.sortBy);
    if (options?.sortDirection) params.append('sortDirection', options.sortDirection);

    // Add filters
    if (options?.filters) {
      Object.entries(options.filters).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          params.append(key, value.toString());
        }
      });
    }

    const { data } = await axios.get<PagedResult<Customer>>(
      `/api/customers?${params.toString()}`
    );
    return data;
  },

  // ... other methods
};
```

#### C. Updated React Hooks

**File:** `client/src/hooks/useCustomer.ts`
```typescript
export const useCustomers = (options?: UseCustomersOptions) => {
  return useQuery({
    queryKey: ['customers', options],
    queryFn: () => customerApi.getAll(options),
    enabled: options?.enabled !== false,
    staleTime: 30000 // 30 seconds
  });
};
```

#### D. Updated List Components

**File:** `client/src/components/Customer/CustomerList.tsx`
```typescript
export const CustomerList: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();

  // Read state from URL
  const [filters, setFilters] = useState<CustomerFilters>({
    name: searchParams.get('name') || '',
    email: searchParams.get('email') || '',
    status: searchParams.get('status') || undefined
  });

  const [page, setPage] = useState(Number(searchParams.get('page')) || 1);
  const [pageSize, setPageSize] = useState(Number(searchParams.get('pageSize')) || 100);
  const [sortModel, setSortModel] = useState<GridSortModel>([
    { field: searchParams.get('sortBy') || 'id', sort: (searchParams.get('sortDirection') as 'asc' | 'desc') || 'asc' }
  ]);

  // Fetch data with server-side filtering
  const { data, isPending, error } = useCustomers({
    filters,
    page,
    pageSize,
    sortBy: sortModel[0]?.field,
    sortDirection: sortModel[0]?.sort
  });

  // Update URL when filters/pagination change
  useEffect(() => {
    const params = new URLSearchParams();

    // Add filters to URL
    Object.entries(filters).forEach(([key, value]) => {
      if (value) params.set(key, value.toString());
    });

    // Add pagination to URL
    if (page !== 1) params.set('page', page.toString());
    if (pageSize !== 100) params.set('pageSize', pageSize.toString());
    if (sortModel[0]?.field) params.set('sortBy', sortModel[0].field);
    if (sortModel[0]?.sort) params.set('sortDirection', sortModel[0].sort);

    setSearchParams(params);
  }, [filters, page, pageSize, sortModel, setSearchParams]);

  const handleFilterChange = (field: keyof CustomerFilters, value: any) => {
    setFilters(prev => ({ ...prev, [field]: value }));
    setPage(1); // Reset to page 1 when filters change
  };

  const handlePageChange = (newPage: number) => {
    setPage(newPage + 1); // MUI DataGrid is 0-indexed
  };

  const handlePageSizeChange = (newPageSize: number) => {
    setPageSize(newPageSize);
    setPage(1);
  };

  const handleSortModelChange = (model: GridSortModel) => {
    setSortModel(model);
  };

  if (isPending) return <CircularProgress />;
  if (error) return <Alert severity="error">Error: {error.message}</Alert>;

  return (
    <Box>
      {/* Filter Controls */}
      <Paper sx={{ p: 2, mb: 2 }}>
        <Typography variant="h6">Filters</Typography>
        <Grid container spacing={2}>
          <Grid item xs={12} md={4}>
            <TextField
              label="Name"
              value={filters.name || ''}
              onChange={(e) => handleFilterChange('name', e.target.value)}
              fullWidth
            />
          </Grid>
          <Grid item xs={12} md={4}>
            <TextField
              label="Email"
              value={filters.email || ''}
              onChange={(e) => handleFilterChange('email', e.target.value)}
              fullWidth
            />
          </Grid>
          <Grid item xs={12} md={4}>
            <FormControl fullWidth>
              <InputLabel>Status</InputLabel>
              <Select
                value={filters.status || ''}
                onChange={(e) => handleFilterChange('status', e.target.value)}
              >
                <MenuItem value="">All</MenuItem>
                <MenuItem value="Active">Active</MenuItem>
                <MenuItem value="Inactive">Inactive</MenuItem>
              </Select>
            </FormControl>
          </Grid>
        </Grid>
        <Button onClick={() => setFilters({})}>Clear Filters</Button>
      </Paper>

      {/* Data Grid */}
      <DataGrid
        rows={data?.items || []}
        columns={columns}
        rowCount={data?.totalCount || 0}

        // Server-side pagination
        paginationMode="server"
        page={page - 1}
        pageSize={pageSize}
        onPageChange={handlePageChange}
        onPageSizeChange={handlePageSizeChange}
        pageSizeOptions={[25, 50, 100, 200]}

        // Server-side sorting
        sortingMode="server"
        sortModel={sortModel}
        onSortModelChange={handleSortModelChange}

        loading={isPending}
        checkboxSelection
        disableRowSelectionOnClick
      />
    </Box>
  );
};
```

---

## 🔧 Implementation Details

### Filter Generation Logic

For each table, the generator will:

1. **Analyze Filter Properties** (from existing `{Entity}Filters` type)
   - String properties → `Contains` operator
   - Numeric properties → `Equals`, `GreaterThan`, `LessThan` operators
   - Date properties → `Between` operator (From/To)
   - Boolean properties → `Equals` operator
   - Enum properties → `In` operator

2. **Generate Backend Filter Application**
   ```csharp
   // Auto-generated based on filter property type
   if (!string.IsNullOrEmpty(filters.PropertyName))
   {
       if (typeof(PropertyType) == typeof(string))
           query = query.Where(e => e.PropertyName.Contains(filters.PropertyName));
       else
           query = query.Where(e => e.PropertyName == filters.PropertyName);
   }
   ```

3. **Generate Frontend Filter Controls**
   - String → `<TextField>`
   - Boolean → `<Checkbox>` or `<Select>` (True/False/All)
   - Enum → `<Select>` with enum values
   - Date → `<DatePicker>` with From/To

### Sorting Generation Logic

1. **Generate Allowed Sort Fields** (from entity properties)
   - All data columns except binary/blob types
   - Primary key always allowed
   - Date columns always allowed

2. **Generate Dynamic OrderBy**
   ```csharp
   return sortBy?.ToLower() switch
   {
       "property1" => direction == "desc" ? query.OrderByDescending(e => e.Property1) : query.OrderBy(e => e.Property1),
       "property2" => direction == "desc" ? query.OrderByDescending(e => e.Property2) : query.OrderBy(e => e.Property2),
       // ... one case per sortable property
       _ => query.OrderBy(e => e.Id)
   };
   ```

### Pagination Generation Logic

1. **Default Page Size:** 100 (configurable)
2. **Max Page Size:** 1000 (to prevent abuse)
3. **Page Size Options:** [25, 50, 100, 200, 500]
4. **Always calculate TotalCount** before pagination for UI

---

## 🧪 Testing Strategy

### Unit Tests

**Backend:**
```csharp
// Test: ApplyFilters_WithNameFilter_ReturnsFilteredResults
[Fact]
public async Task ApplyFilters_WithNameFilter_ReturnsFilteredResults()
{
    // Arrange
    var query = new GetCustomersQuery
    {
        Filters = new CustomerFilters { Name = "John" }
    };

    // Act
    var result = await _handler.Handle(query, CancellationToken.None);

    // Assert
    Assert.All(result.Items, c => Assert.Contains("John", c.Name));
}

// Test: Pagination_ReturnsCorrectPage
[Fact]
public async Task Pagination_ReturnsCorrectPage()
{
    // Arrange
    var query = new GetCustomersQuery { Page = 2, PageSize = 10 };

    // Act
    var result = await _handler.Handle(query, CancellationToken.None);

    // Assert
    Assert.Equal(2, result.Page);
    Assert.Equal(10, result.Items.Count);
}
```

**Frontend:**
```typescript
// Test: useCustomers hook with filters
test('useCustomers applies filters', async () => {
  const { result } = renderHook(() => useCustomers({
    filters: { name: 'John' }
  }));

  await waitFor(() => expect(result.current.isSuccess).toBe(true));

  expect(result.current.data.items).toHaveLength(5);
  expect(result.current.data.items[0].name).toContain('John');
});
```

### Integration Tests

```csharp
// Test: Full stack filtering via API
[Fact]
public async Task API_GetCustomers_WithFilters_ReturnsPagedResults()
{
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync(
        "/api/customers?name=John&page=1&pageSize=10&sortBy=name&sortDirection=asc"
    );

    // Assert
    response.EnsureSuccessStatusCode();
    var result = await response.Content.ReadAsAsync<PagedResult<CustomerDto>>();

    Assert.NotEmpty(result.Items);
    Assert.True(result.TotalCount > 0);
    Assert.Equal(1, result.Page);
}
```

---

## 📝 Generator Changes Required

### 1. TypeScriptTypeGenerator.cs

**Changes:**
- Add generation of `PagedResult<T>` type
- Add generation of `UseEntityOptions<TFilters>` type
- Keep existing `{Entity}Filters` interface (already has correct properties)

### 2. ReactApiGenerator.cs

**Changes:**
- Update `getAll()` function signature:
  ```typescript
  // OLD:
  getAll: async (): Promise<Customer[]>

  // NEW:
  getAll: async (options?: UseCustomersOptions): Promise<PagedResult<Customer>>
  ```
- Build query string from options (filters + pagination)

### 3. ReactHookGenerator.cs

**Changes:**
- Update `useEntities()` hook signature:
  ```typescript
  // OLD:
  export const useCustomers = ()

  // NEW:
  export const useCustomers = (options?: UseCustomersOptions)
  ```
- Pass options to API function
- Update queryKey to include options for cache invalidation

### 4. ReactListComponentGenerator.cs

**Major Changes:**
- Remove client-side filtering logic
- Add URL state management (useSearchParams)
- Add server-side pagination props to DataGrid:
  ```typescript
  paginationMode="server"
  rowCount={data?.totalCount || 0}
  ```
- Add server-side sorting props:
  ```typescript
  sortingMode="server"
  sortModel={sortModel}
  onSortModelChange={handleSortModelChange}
  ```
- Update filter handlers to update URL and trigger refetch
- Show pagination info: "Showing 1-100 of 5,234 records"

### 5. CQRSGenerator.cs

**Changes:**
- Update `Get{Entity}Query` to include:
  - `{Entity}Filters Filters { get; set; }`
  - `int Page { get; set; } = 1`
  - `int PageSize { get; set; } = 100`
  - `string SortBy { get; set; }`
  - `string SortDirection { get; set; } = "asc"`

- Update `Get{Entity}QueryHandler`:
  - Generate `ApplyFilters()` method
  - Generate `ApplySorting()` method
  - Add `CountAsync()` before pagination
  - Add `.Skip().Take()` for pagination
  - Return `PagedResult<TDto>` instead of `List<TDto>`

### 6. APIControllerGenerator.cs

**Changes:**
- Update `GetAll()` endpoint:
  ```csharp
  // OLD:
  [HttpGet]
  public async Task<ActionResult<List<CustomerDto>>> GetAll()

  // NEW:
  [HttpGet]
  public async Task<ActionResult<PagedResult<CustomerDto>>> GetAll(
      [FromQuery] CustomerFilters filters,
      [FromQuery] int page = 1,
      [FromQuery] int pageSize = 100,
      [FromQuery] string sortBy = "Id",
      [FromQuery] string sortDirection = "asc")
  ```

### 7. New: SharedModelsGenerator.cs

**Create new generator for:**
- `Domain/Common/PagedResult.cs`
- `Domain/Common/PaginationParams.cs`
- `Domain/Common/FilterOperator.cs`
- `Domain/Common/FilterCriteria.cs`

These are generated **once per project**, not per table.

---

## 🚀 Migration Strategy

### Backward Compatibility

**Option 1: Dual Endpoints (Recommended)**
```csharp
// NEW endpoint with pagination
[HttpGet]
public async Task<ActionResult<PagedResult<CustomerDto>>> GetAll(...)

// OLD endpoint (deprecated but still works)
[HttpGet("legacy")]
[Obsolete("Use GetAll with pagination instead")]
public async Task<ActionResult<List<CustomerDto>>> GetAllLegacy()
{
    var result = await GetAll(filters: null, page: 1, pageSize: int.MaxValue);
    return Ok(result.Items);
}
```

**Option 2: Default to Non-Paged (if no params provided)**
```csharp
[HttpGet]
public async Task<ActionResult<object>> GetAll(
    [FromQuery] CustomerFilters filters,
    [FromQuery] int? page = null,
    [FromQuery] int? pageSize = null)
{
    if (page.HasValue)
    {
        // Return PagedResult
        var pagedResult = await _mediator.Send(new GetCustomersQuery { ... });
        return Ok(pagedResult);
    }
    else
    {
        // Return List (old behavior)
        var allResults = await _mediator.Send(new GetCustomersQuery { Page = 1, PageSize = int.MaxValue });
        return Ok(allResults.Items);
    }
}
```

### Rollout Plan

1. **Phase 1:** Generate new backend code (CQRS + API)
2. **Phase 2:** Generate new frontend code (API + Hooks)
3. **Phase 3:** Update List components
4. **Phase 4:** Test with existing projects
5. **Phase 5:** Update documentation

---

## 📊 Performance Benchmarks

### Before (Client-Side Filtering)

| Dataset Size | Load Time | Memory Usage | Network Transfer |
|--------------|-----------|--------------|------------------|
| 100 records  | 0.5s      | 5 MB         | 50 KB            |
| 1,000 records| 2s        | 15 MB        | 500 KB           |
| 10,000 records| 10s      | 80 MB        | 5 MB             |
| 100,000 records| ❌ Crash | ❌ Crash    | ❌ Crash         |

### After (Server-Side Filtering)

| Dataset Size | Load Time | Memory Usage | Network Transfer |
|--------------|-----------|--------------|------------------|
| 100 records  | 0.3s      | 3 MB         | 10 KB            |
| 1,000 records| 0.4s      | 3 MB         | 10 KB            |
| 10,000 records| 0.5s     | 3 MB         | 10 KB            |
| 100,000 records| 0.6s    | 3 MB         | 10 KB            |
| 1,000,000 records| 0.8s  | 3 MB         | 10 KB            |

**Note:** Load time/memory/transfer are nearly constant regardless of total dataset size!

---

## 🎨 UI/UX Enhancements

### Visual Indicators

1. **Loading State**
   - Skeleton rows while fetching
   - Progress bar at top of grid
   - Disable filters during load

2. **Pagination Info**
   ```
   Showing 1-100 of 5,234 records
   ```

3. **Active Filters Badge**
   ```
   Filters (3 active) [Clear All]
   ```

4. **Sort Indicator**
   - Arrow icons in column headers
   - Show current sort field

### Shareable URLs

Users can share filtered/sorted views:
```
https://app.com/customers?name=John&status=Active&page=2&sortBy=createdAt&sortDirection=desc
```

---

## 🔐 Security Considerations

### Input Validation

1. **Page/PageSize Limits**
   ```csharp
   if (pageSize > 1000) pageSize = 1000; // Max 1000 records per page
   if (page < 1) page = 1;
   ```

2. **Sort Field Whitelist**
   ```csharp
   var allowedSortFields = new[] { "Id", "Name", "Email", "CreatedAt" };
   if (!allowedSortFields.Contains(sortBy))
       sortBy = "Id";
   ```

3. **SQL Injection Prevention**
   - Use parameterized queries (EF Core handles this)
   - Never use string concatenation for WHERE clauses

### Performance Protection

1. **Query Timeout**
   ```csharp
   _context.Database.SetCommandTimeout(30); // 30 seconds max
   ```

2. **Rate Limiting**
   ```csharp
   [RateLimit(Requests = 100, Period = "1m")]
   [HttpGet]
   public async Task<ActionResult> GetAll()
   ```

---

## 📚 Documentation Updates Required

### User Documentation

1. **New section:** "Working with Large Datasets"
2. **New section:** "Server-Side Filtering Guide"
3. **Update:** All List component examples
4. **Update:** API documentation with new parameters

### Developer Documentation

1. **New section:** "How Filter Generation Works"
2. **New section:** "Customizing Pagination Behavior"
3. **Update:** CQRS generator documentation
4. **Update:** React generator documentation

---

## ✅ Definition of Done

1. ✅ All generators updated and producing correct code
2. ✅ Shared models (PagedResult, etc.) generated
3. ✅ Backend API returns PagedResult with correct metadata
4. ✅ Frontend DataGrid uses server-side pagination/sorting
5. ✅ Filters update URL and trigger server requests
6. ✅ All existing tests pass
7. ✅ New tests added (unit + integration)
8. ✅ Performance benchmarks meet targets (< 2s load time)
9. ✅ Documentation updated
10. ✅ Tested with MyProject (UpayCard.RiskManagement)
11. ✅ No breaking changes to existing generated projects

---

## 🎯 Success Metrics

### Quantitative

- ✅ Page load time: < 2 seconds (regardless of dataset size)
- ✅ Memory usage: < 100 MB (constant)
- ✅ Network transfer: < 50 KB per page
- ✅ Test coverage: > 90%
- ✅ Zero breaking changes

### Qualitative

- ✅ Developers report faster development
- ✅ Users report better performance
- ✅ No complaints about slow page loads
- ✅ Positive feedback on shareable filtered URLs

---

## 📅 Timeline Estimate

| Task | Estimated Time | Priority |
|------|----------------|----------|
| Create shared models generator | 0.5 day | P0 |
| Update CQRS generator | 1.5 days | P0 |
| Update API controller generator | 1 day | P0 |
| Update TypeScript types generator | 0.5 day | P0 |
| Update React API generator | 1 day | P0 |
| Update React Hook generator | 1 day | P0 |
| Update React List generator | 2 days | P0 |
| Write unit tests | 1.5 days | P1 |
| Write integration tests | 1 day | P1 |
| Test with MyProject | 0.5 day | P1 |
| Documentation | 1 day | P2 |
| **TOTAL** | **10-12 days** | |

---

## 🚨 Risks & Mitigations

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Breaking existing projects | Medium | High | Add backward compatibility mode |
| Performance regression | Low | High | Benchmark before/after, add query optimization |
| Complex filter logic bugs | Medium | Medium | Extensive unit tests per filter type |
| URL query string too long | Low | Low | Add warning when > 2000 chars |
| Database timeout with complex queries | Low | Medium | Add query timeout, optimize indexes |

---

## 🔄 Future Enhancements (Out of Scope)

1. **Advanced Filter Builder UI** - Drag-and-drop query builder
2. **Saved Filters** - Users can save favorite filter combinations
3. **Export Filtered Data** - Excel export respects current filters
4. **Real-time Filter Suggestions** - Autocomplete for filter values
5. **Filter Templates** - Pre-defined filter sets (e.g., "Active customers from last month")

---

## 📞 Contacts & Resources

- **Specification Author:** Claude
- **Approval:** Doron Gut
- **Implementation Team:** Doron + Claude
- **Target Release:** TargCC Core v2.1.0

---

## Appendix A: Example Generated Code

See detailed code examples in sections above.

---

## Appendix B: Database Schema Considerations

### Required Indexes

For optimal performance, the generator should suggest creating indexes on:

1. **Filter fields** (e.g., Status, CreatedAt, Name)
2. **Sort fields** (e.g., Id, CreatedAt, Name)
3. **Composite indexes** for common filter combinations

**Example:**
```sql
CREATE INDEX IX_Customer_Status_CreatedAt ON Customer(Status, CreatedAt);
CREATE INDEX IX_Customer_Name ON Customer(Name);
```

The generator could output a `_SuggestedIndexes.sql` file with recommendations.

---

**End of Specification**
