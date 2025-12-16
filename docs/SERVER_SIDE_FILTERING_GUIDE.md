# Server-Side Filtering, Sorting, and Pagination Guide

## Overview

TargCC-generated projects now include built-in support for server-side filtering, sorting, and pagination. This enables efficient handling of large datasets by performing data operations on the backend rather than in the browser.

## Features

- **Server-Side Pagination**: Only fetch the records needed for the current page
- **Server-Side Sorting**: Sort data at the database level using SQL ORDER BY
- **Server-Side Filtering**: Filter data using SQL WHERE clauses
- **URL State Management**: Filters, sorting, and pagination are persisted in the URL for shareable links
- **Type-Safe APIs**: Fully typed with TypeScript across frontend and backend

## Architecture

### Backend (C# / ASP.NET Core)

#### 1. Shared Models (`{Project}.Domain/Common/`)

Generated automatically for every project:

**PagedResult.cs** - Generic paginated response
```csharp
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
```

**PaginationParams.cs** - Common pagination parameters
```csharp
public class PaginationParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 100;
    public string? SortBy { get; set; }
    public string SortDirection { get; set; } = "asc";
}
```

#### 2. CQRS Queries (`{Project}.Application/Queries/`)

Each entity gets a query with pagination support:

```csharp
public class GetCardIssuersQuery : IRequest<Result<PagedResult<CardIssuerDto>>>
{
    public CardIssuerFilters? Filters { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 100;
    public string? SortBy { get; init; } = "Id";
    public string SortDirection { get; init; } = "asc";
}
```

#### 3. Query Handlers

Handlers implement filtering and sorting logic:

```csharp
public async Task<Result<PagedResult<CardIssuerDto>>> Handle(
    GetCardIssuersQuery request,
    CancellationToken cancellationToken)
{
    var query = _context.CardIssuers.AsQueryable();

    // Apply filters
    query = ApplyFilters(query, request.Filters);

    // Get total count BEFORE pagination
    var totalCount = await query.CountAsync(cancellationToken);

    // Apply sorting
    query = ApplySorting(query, request.SortBy, request.SortDirection);

    // Apply pagination
    query = query
        .Skip((request.Page - 1) * request.PageSize)
        .Take(request.PageSize);

    // Execute query
    var entities = await query.ToListAsync(cancellationToken);
    var dtos = entities.Select(MapToDto).ToList();

    return Result<PagedResult<CardIssuerDto>>.Success(new PagedResult<CardIssuerDto>
    {
        Items = dtos,
        TotalCount = totalCount,
        Page = request.Page,
        PageSize = request.PageSize
    });
}
```

#### 4. API Controllers

Controllers accept pagination parameters:

```csharp
[HttpGet]
public async Task<ActionResult<PagedResult<CardIssuerDto>>> GetAll(
    [FromQuery] CardIssuerFilters? filters,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 100,
    [FromQuery] string? sortBy = "Id",
    [FromQuery] string sortDirection = "asc")
{
    var query = new GetCardIssuersQuery
    {
        Filters = filters,
        Page = page,
        PageSize = pageSize,
        SortBy = sortBy,
        SortDirection = sortDirection
    };

    var result = await _mediator.Send(query);
    return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
}
```

### Frontend (React / TypeScript)

#### 1. Common Types (`client/src/types/common.types.ts`)

Generated once per project:

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

export interface UseEntityOptions<TFilters> extends PaginationParams {
  filters?: TFilters;
  enabled?: boolean;
}
```

#### 2. API Client

API functions return PagedResult:

```typescript
export const cardIssuerApi = {
  getAll: async (options?: UseCardIssuerOptions): Promise<PagedResult<CardIssuer>> => {
    const params = new URLSearchParams();
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

    const response = await api.get<PagedResult<CardIssuer>>(
      `/cardissuers?${params.toString()}`
    );
    return response.data;
  }
};
```

#### 3. React Hooks

Hooks accept options parameter:

```typescript
export const useCardIssuers = (options?: UseCardIssuerOptions) => {
  return useQuery({
    queryKey: ['cardIssuers', options],
    queryFn: () => cardIssuerApi.getAll(options),
    enabled: options?.enabled !== false,
    staleTime: 30000, // 30 seconds
  });
};
```

#### 4. List Components

Components use URL-based state and server-side DataGrid:

```typescript
export const CardIssuerList: React.FC = () => {
  const [searchParams, setSearchParams] = useSearchParams();

  // Extract state from URL
  const page = parseInt(searchParams.get('page') || '1', 10);
  const pageSize = parseInt(searchParams.get('pageSize') || '10', 10);
  const sortBy = searchParams.get('sortBy') || 'id';
  const sortDirection = (searchParams.get('sortDirection') || 'asc') as 'asc' | 'desc';

  // Extract filters from URL
  const filters = React.useMemo(() => {
    const filtersObj: Record<string, string> = {};
    searchParams.forEach((value, key) => {
      if (!['page', 'pageSize', 'sortBy', 'sortDirection'].includes(key)) {
        filtersObj[key] = value;
      }
    });
    return filtersObj;
  }, [searchParams]);

  // Fetch data with server-side options
  const { data, isLoading, error } = useCardIssuers({
    page,
    pageSize,
    sortBy,
    sortDirection,
    filters: Object.keys(filters).length > 0 ? filters : undefined,
  });

  const cardIssuers = data?.items || [];
  const totalCount = data?.totalCount || 0;

  // DataGrid with server-side mode
  return (
    <DataGrid
      rows={cardIssuers}
      columns={columns}
      // Server-side pagination
      paginationMode="server"
      rowCount={totalCount}
      paginationModel={{
        page: page - 1, // MUI is 0-based, API is 1-based
        pageSize: pageSize,
      }}
      onPaginationModelChange={(model) =>
        handlePaginationChange(model.page, model.pageSize)
      }
      // Server-side sorting
      sortingMode="server"
      sortModel={[{
        field: sortBy,
        sort: sortDirection,
      }]}
      onSortModelChange={(model) => {
        if (model.length > 0) {
          handleSortChange(model[0].field, model[0].sort);
        }
      }}
    />
  );
};
```

## Usage Examples

### 1. Basic Pagination

Navigate to a list page:
```
/cardissuers
```

The URL will automatically update as you interact:
```
/cardissuers?page=2&pageSize=25
```

### 2. Sorting

Click a column header to sort:
```
/cardissuers?page=1&pageSize=10&sortBy=name&sortDirection=asc
```

### 3. Filtering

Use the filter fields at the top of the list:
```
/cardissuers?page=1&pageSize=10&name=visa&status=Active
```

### 4. Combined Operations

All parameters work together:
```
/cardissuers?page=2&pageSize=25&sortBy=createdDate&sortDirection=desc&status=Active
```

## Performance Benefits

### Before (Client-Side)
- Fetches ALL records: 10,000+ records
- Browser downloads: ~5MB of JSON
- Initial load time: 3-5 seconds
- Memory usage: High
- Browser may freeze with large datasets

### After (Server-Side)
- Fetches ONLY current page: 10-100 records
- Browser downloads: ~50KB of JSON
- Initial load time: 200-500ms
- Memory usage: Low
- Smooth performance regardless of dataset size

## Customizing Filters

### Backend: Add Custom Filter Logic

Edit the `ApplyFilters` method in your query handler:

```csharp
private static IQueryable<CardIssuer> ApplyFilters(
    IQueryable<CardIssuer> query,
    CardIssuerFilters? filters)
{
    if (filters == null) return query;

    // String filters (case-insensitive contains)
    if (!string.IsNullOrWhiteSpace(filters.Name))
    {
        query = query.Where(x => x.Name.Contains(filters.Name));
    }

    // Exact match filters
    if (!string.IsNullOrWhiteSpace(filters.Code))
    {
        query = query.Where(x => x.Code == filters.Code);
    }

    // Boolean filters
    if (filters.IsActive.HasValue)
    {
        query = query.Where(x => x.IsActive == filters.IsActive.Value);
    }

    // Date range filters
    if (filters.CreatedDateFrom.HasValue)
    {
        query = query.Where(x => x.CreatedDate >= filters.CreatedDateFrom.Value);
    }

    if (filters.CreatedDateTo.HasValue)
    {
        query = query.Where(x => x.CreatedDate <= filters.CreatedDateTo.Value);
    }

    return query;
}
```

### Frontend: Add Filter Fields

Filter fields are automatically generated based on column types:

```typescript
// Text filter
<TextField
  label="Name"
  value={filters['name'] || ''}
  onChange={(e) => updateUrlParam('name', e.target.value)}
/>

// Dropdown filter
<TextField
  select
  label="Status"
  value={filters['status'] || ''}
  onChange={(e) => updateUrlParam('status', e.target.value)}
>
  <MenuItem value="">All</MenuItem>
  <MenuItem value="Active">Active</MenuItem>
  <MenuItem value="Inactive">Inactive</MenuItem>
</TextField>

// Date filter
<TextField
  type="date"
  label="Created Date"
  value={filters['createdDate'] || ''}
  onChange={(e) => updateUrlParam('createdDate', e.target.value)}
  InputLabelProps={{ shrink: true }}
/>
```

## Advanced Sorting

### Backend: Add Sortable Fields

Edit the `ApplySorting` method:

```csharp
private static IQueryable<CardIssuer> ApplySorting(
    IQueryable<CardIssuer> query,
    string? sortBy,
    string? sortDirection)
{
    if (string.IsNullOrWhiteSpace(sortBy))
        sortBy = "Id";

    var ascending = sortDirection?.ToLowerInvariant() != "desc";

    return sortBy.ToLower() switch
    {
        "id" => ascending ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id),
        "name" => ascending ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name),
        "code" => ascending ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
        "createddate" => ascending ? query.OrderBy(x => x.CreatedDate) : query.OrderByDescending(x => x.CreatedDate),
        _ => ascending ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id)
    };
}
```

## Testing

### Manual Testing

1. Navigate to a list page
2. Try different page sizes: 5, 10, 25, 50, 100
3. Sort by different columns
4. Apply filters
5. Check that the URL updates correctly
6. Copy the URL and paste in a new tab - state should be preserved
7. Check Network tab - verify only 1 page of data is fetched

### Performance Testing

```bash
# Test with large dataset (10,000+ records)
# Before: 3-5 seconds load time, 5MB download
# After: 200-500ms load time, 50KB download

# Use browser DevTools Network tab to verify
```

## Troubleshooting

### "Too Many Results" Error

If the API returns too many results:
- Check that `paginationMode="server"` is set on DataGrid
- Verify the API is receiving pagination parameters
- Check the query handler is actually applying Skip/Take

### Filters Not Working

- Ensure filter values are being added to URL params
- Check that `ApplyFilters` method is being called in the handler
- Verify filter parameter names match exactly (case-sensitive)

### Sorting Not Working

- Ensure `sortingMode="server"` is set on DataGrid
- Check that the sort field name exists in the entity
- Verify `ApplySorting` method handles the field

### Page Count Incorrect

- Ensure `totalCount` is calculated BEFORE pagination (Skip/Take)
- Verify the count includes all filtered records

## Migration from Client-Side

If you have existing projects with client-side filtering:

1. **Regenerate the project** with the updated TargCC version
2. **Update imports** - Add common.types.ts imports
3. **Update API calls** - Change from `T[]` to `PagedResult<T>`
4. **Update hooks** - Pass options parameter
5. **Update components** - Add URL state management
6. **Update DataGrid** - Set server-side modes

## Best Practices

1. **Use appropriate page sizes**: 10-100 records per page
2. **Index filterable columns**: Add database indexes for commonly filtered fields
3. **Optimize queries**: Use `.AsNoTracking()` for read-only queries
4. **Cache results**: Use React Query's staleTime for better UX
5. **Debounce filters**: Wait for user to finish typing before filtering
6. **Provide clear feedback**: Show loading states and result counts
7. **Test with large datasets**: Ensure performance with 10k+ records

## Related Documentation

- [SPEC_REACT_UI_GENERATOR.md](SPEC_REACT_UI_GENERATOR.md) - UI generator specification
- [SERVER_SIDE_FILTERING_PAGINATION_SPEC.md](specs/SERVER_SIDE_FILTERING_PAGINATION_SPEC.md) - Complete technical specification

## Support

For issues or questions:
- GitHub Issues: https://github.com/yourorg/TargCC-Core-V2/issues
- Documentation: /docs
