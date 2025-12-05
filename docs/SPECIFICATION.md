# TargCC Code Generator - Specification Document

**Version:** 2.0
**Date:** 2025-12-05
**Branch:** claude/review-test-script-01Te4Z88CcBDuBAs9H2PvxYk
**Status:** Stages 1-3.4 Complete, Stage 3.5 & Master-Detail Pending

---

## Executive Summary

TargCC is an advanced code generator that creates full-stack applications (ASP.NET Core Backend + React Frontend) from database schemas. This document specifies the features implemented in this development session and outlines future enhancements.

---

## Implemented Features (Current Session)

### ✅ Stage 1: Fix CREATE Bug (CRITICAL)

**Problem:**
- Navigating to `/customers/new` showed a blank screen
- Form component expected `customerId` as prop but routes didn't pass it
- Users couldn't create new records

**Solution:**
- Changed form component from props-based to `useParams()`-based
- Extracts ID from URL: `/new` → undefined, `/:id/edit` → Number(id)
- Handles both create and edit modes in single component

**Files Modified:**
- `ReactFormComponentGenerator.cs:51` - Added `useParams` to imports
- `ReactFormComponentGenerator.cs:359-363` - Use URL params instead of props

**Result:**
- ✅ `/customers/new` shows empty form for creating
- ✅ `/customers/123/edit` loads customer #123 for editing

**Commit:** `144a1f3`

---

### ✅ Stage 2: Excel Export (Client-Side)

**Feature:**
Export currently visible data to Excel file with one click.

**Implementation:**
- Added `xlsx` (v0.18.5) npm package
- Export button next to Create button on all list views
- Filename format: `TableName_YYYY-MM-DD.xlsx`
- Client-side generation (suitable for <10,000 records)
- Alert if no data to export

**Files Modified:**
- `ProjectGenerationService.cs:465` - Added xlsx dependency
- `ReactListComponentGenerator.cs:50` - Import xlsx library
- `ReactListComponentGenerator.cs:56` - Added FileDownloadIcon
- `ReactListComponentGenerator.cs:141-151` - handleExportToExcel function
- `ReactListComponentGenerator.cs:199-205` - Export button UI

**Generated Code Example:**
```typescript
const handleExportToExcel = () => {
  if (!customers || customers.length === 0) {
    alert('No data to export');
    return;
  }

  const ws = XLSX.utils.json_to_sheet(customers);
  const wb = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(wb, ws, 'Customers');
  XLSX.writeFile(wb, `Customers_${new Date().toISOString().split('T')[0]}.xlsx`);
};
```

**User Experience:**
- Click "Export to Excel" → immediate download
- Works on any table (Customers, Orders, Products, etc.)
- Includes all visible columns

**Future Enhancement:**
- Server-side export for large datasets (>10K records)
- Custom column selection
- Styling and formatting options
- Multiple sheets support

**Commit:** `d77445a`

---

### ✅ Stage 3: Index-Based Filtering

Automatic generation of filter functionality based on table indexes.

#### Stage 3.1-3.2: SQL Stored Procedures ✅

**Feature:**
Generate `SP_GetFiltered[TableName]s` stored procedures with parameters for each indexed column.

**Logic:**
- Detects non-primary key indexes
- Creates `@ColumnName` parameter for each indexed column (nullable)
- WHERE clauses:
  - Text columns: `LIKE '%@Value%'` for partial match
  - Other columns: `= @Value` for exact match
- NULL handling: `(@Param IS NULL OR [Column] = @Param)`
- Includes pagination (@Skip, @Take)

**Files Created:**
- `SpGetFilteredTemplate.cs` - New template generator

**Files Modified:**
- `SqlGenerator.cs:131-145` - Call to SpGetFilteredTemplate

**Generated SQL Example:**
```sql
CREATE OR ALTER PROCEDURE [dbo].[SP_GetFilteredCustomers]
    @Email NVARCHAR(255) = NULL,
    @City NVARCHAR(100) = NULL,
    @Skip INT = NULL,
    @Take INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [Customer]
    WHERE 1=1
      AND (@Email IS NULL OR [Email] LIKE '%' + @Email + '%')
      AND (@City IS NULL OR [City] = @City)
    ORDER BY [ID]
    OFFSET ISNULL(@Skip, 0) ROWS
    FETCH NEXT ISNULL(@Take, 2147483647) ROWS ONLY;
END
```

**Commits:** `4184436`

---

#### Stage 3.3: Repository Layer ✅

**Feature:**
Add `GetFilteredAsync` method to repository interfaces and implementations.

**Implementation:**

**Interface (ICustomerRepository.cs):**
```csharp
/// <summary>
/// Gets filtered Customer entities based on indexed columns.
/// </summary>
/// <param name="email">Filter by Email (optional).</param>
/// <param name="city">Filter by City (optional).</param>
/// <param name="skip">Number of entities to skip (for paging).</param>
/// <param name="take">Number of entities to take (for paging).</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>Collection of filtered Customer entities.</returns>
Task<IEnumerable<Customer>> GetFilteredAsync(
    string? email = null,
    string? city = null,
    int? skip = null,
    int? take = null,
    CancellationToken cancellationToken = default);
```

**Implementation (CustomerRepository.cs):**
```csharp
public async Task<IEnumerable<Customer>> GetFilteredAsync(
    string? email = null, string? city = null,
    int? skip = null, int? take = null,
    CancellationToken cancellationToken = default)
{
    var parameters = new {
        Email = email,
        City = city,
        Skip = skip,
        Take = take
    };

    return await _connection.QueryAsync<Customer>(
        "SP_GetFilteredCustomers",
        parameters,
        commandType: CommandType.StoredProcedure);
}
```

**Files Modified:**
- `RepositoryInterfaceGenerator.cs:200` - Added call to GenerateGetFilteredAsyncMethod
- `RepositoryInterfaceGenerator.cs:374-439` - New GenerateGetFilteredAsyncMethod
- `RepositoryGenerator.cs:95` - Added call to GenerateGetFilteredAsync
- `RepositoryGenerator.cs:275-359` - New GenerateGetFilteredAsync method

**Features:**
- Dynamic method generation based on table indexes
- All parameters nullable (optional filtering)
- Proper logging and error handling
- XML documentation

**Commit:** `a18e5f1`

---

#### Stage 3.4: API Controller Endpoints ✅

**Feature:**
Add `GetFiltered` HTTP endpoint to API controllers.

**Implementation:**
```csharp
/// <summary>
/// Gets filtered Customers based on indexed columns.
/// </summary>
/// <param name="email">Filter by Email.</param>
/// <param name="city">Filter by City.</param>
/// <returns>Collection of filtered Customer entities.</returns>
[HttpGet("filter")]
[ProducesResponseType(typeof(IEnumerable<Customer>), 200)]
public async Task<ActionResult<IEnumerable<Customer>>> GetFiltered(
    [FromQuery] string? email = null,
    [FromQuery] string? city = null)
{
    var entities = await _repository.GetFilteredAsync(email, city)
        .ConfigureAwait(false);
    return Ok(entities);
}
```

**URL Example:**
```
GET /api/customers/filter?email=john&city=TelAviv
```

**Files Modified:**
- `ApiControllerGenerator.cs:52` - Pass Table object to GenerateControllerClass
- `ApiControllerGenerator.cs:71` - Updated signature with Table parameter
- `ApiControllerGenerator.cs:103` - Added GenerateGetFilteredMethod call
- `ApiControllerGenerator.cs:302-367` - New GenerateGetFilteredMethod
- `ApiControllerGenerator.cs:369-382` - Helper GetCSharpTypeName

**Features:**
- Dynamic endpoint generation based on indexes
- Query string parameters ([FromQuery])
- Swagger documentation support
- Calls repository GetFilteredAsync

**Commit:** `27e92fe`

---

#### Stage 3.5: React UI Filter Components ⏳ PENDING

**Planned Implementation:**

**UI Components:**
1. Filter component above DataGrid
2. TextField for text columns (search)
3. DatePicker for date columns (from/to)
4. Dropdown/Autocomplete for FK columns
5. Apply Filters / Clear Filters buttons

**Example UI:**
```
┌─────────────────────────────────────┐
│ Customers                           │
├─────────────────────────────────────┤
│ 🔍 Email: [________] 🔍           │
│ 📍 City:  [All ▼] [Tel Aviv] [...] │
│ [Apply Filters] [Clear]             │
├─────────────────────────────────────┤
│ ID │ Name     │ Email          │... │
│  1 │ John Doe │ john@test.com  │... │
└─────────────────────────────────────┘
```

**Files to Create/Modify:**
- `ReactFilterComponentGenerator.cs` (NEW) - Generate filter UI
- `ReactApiGenerator.cs` - Add getFiltered() API call
- `ReactListComponentGenerator.cs` - Integrate filters with DataGrid

**Deferred Reason:**
Token usage limit approaching. React UI implementation requires:
- Complex component generation logic
- State management integration
- Material-UI form controls
- React Query integration

**Estimated Effort:** 4-6 hours

---

## Future Major Features

### Master-Detail Views

**Concept:**
Generate views that show relationships between tables based on foreign keys.

**Examples:**
1. **Customer Details** → List of Orders
2. **Order Details** → List of OrderItems with Product names
3. **Navigation breadcrumbs** between related entities

**Architecture:**
1. Detect Foreign Key relationships
2. Generate SQL Views or SPs with JOINs
3. Create API endpoints (e.g., `GET /api/customers/1/orders`)
4. Generate React Master-Detail components
5. Nested DataGrids (Master top, Detail bottom)

**Files to Create:**
- `SpGetWithRelatedTemplate.cs` - SQL template for JOINs
- `RelatedDataEndpointGenerator.cs` - API controller extensions
- `ReactMasterDetailComponentGenerator.cs` - UI components

**Estimated Effort:** 1-2 weeks

---

## Architecture Overview

### Code Generation Pipeline

```
Database Schema (SQL Server/SQLite)
    ↓
TargCC.CLI (test_targcc_v2.sh)
    ↓
┌─────────────────────────────────────┐
│   Code Generators (TargCC.Core)     │
├─────────────────────────────────────┤
│ 1. SQL Generators                   │
│    - Stored Procedures (CRUD)       │
│    - SP_GetAll, SP_GetFiltered      │
│                                      │
│ 2. Domain Layer                     │
│    - Entities (C# classes)          │
│    - Repository Interfaces          │
│                                      │
│ 3. Infrastructure Layer             │
│    - Repository Implementations     │
│    - Dapper + SQL Server/SQLite     │
│    - Dependency Injection           │
│                                      │
│ 4. API Layer                        │
│    - Controllers (REST endpoints)   │
│    - Swagger documentation          │
│    - CORS configuration             │
│                                      │
│ 5. Frontend (React)                 │
│    - TypeScript API clients         │
│    - Material-UI components         │
│    - React Query hooks              │
│    - Forms (react-hook-form)        │
│    - DataGrid (MUI X)               │
└─────────────────────────────────────┘
    ↓
Generated Full-Stack Application
    ↓
┌──────────────────┐   ┌─────────────────┐
│  Backend API     │   │  React Frontend │
│  (localhost:5000)│   │  (localhost:5173)│
└──────────────────┘   └─────────────────┘
```

### Technology Stack

**Backend:**
- ASP.NET Core 8.0
- Clean Architecture (Domain, Application, Infrastructure, API)
- Dapper (Micro-ORM)
- SQL Server / SQLite
- Stored Procedures for all data access
- Dependency Injection
- Serilog (Logging)

**Frontend:**
- React 18.2
- TypeScript 5.3
- Vite (Build Tool)
- Material-UI (MUI) 5.14
- React Router 6.20
- React Query (TanStack Query) 5.12
- react-hook-form 7.48
- Axios 1.6
- xlsx 0.18 (Excel export)

---

## Testing Instructions

### Prerequisites
- .NET 8.0 SDK
- Node.js 20+
- SQL Server or SQLite
- SSMS (for SQL Server) or DB Browser (for SQLite)

### Build & Run

#### 1. Build TargCC Generator
```bash
cd /home/user/TargCC-Core-V2
dotnet build
```

#### 2. Generate Application
```bash
./test_targcc_v2.sh
```

#### 3. Run Generated SQL
Open SSMS and execute all `.sql` files from:
```
[output-directory]/sql/Customer.sql
[output-directory]/sql/Order.sql
[output-directory]/sql/Product.sql
...
```

#### 4. Run Backend API
```bash
cd [output-directory]/src/[ProjectName].API
dotnet run
```

Backend runs at: `http://localhost:5000`
Swagger UI: `http://localhost:5000/swagger`

#### 5. Run Frontend
```bash
cd [output-directory]/client
npm install
npm run dev
```

Frontend runs at: `http://localhost:5173`

### Test Features

**✅ Working Features:**
1. Navigate to `http://localhost:5173`
2. View list of Customers/Orders/Products
3. Sort by clicking column headers
4. Click "Create Customer" → form appears (not blank!)
5. Fill form and Save → redirects to list
6. Click "Export to Excel" → downloads `.xlsx` file
7. API: `GET /api/customers/filter?email=john` → filtered results

**⏳ Not Yet Implemented:**
- React UI filter components (next session)
- Master-Detail views (future feature)

---

## Commits Summary

| Commit | Description | Files Changed |
|--------|-------------|---------------|
| `144a1f3` | Fix CREATE bug - useParams | ReactFormComponentGenerator.cs |
| `d77445a` | Excel export functionality | ProjectGenerationService.cs, ReactListComponentGenerator.cs |
| `4184436` | SP_GetFiltered template | SpGetFilteredTemplate.cs, SqlGenerator.cs |
| `a18e5f1` | Repository GetFilteredAsync | RepositoryInterfaceGenerator.cs, RepositoryGenerator.cs |
| `27e92fe` | Controller GetFiltered endpoint | ApiControllerGenerator.cs |

---

## Known Issues

### Resolved
- ✅ Form component blank on `/customers/new`
- ✅ Missing SP_GetAll stored procedures
- ✅ Duplicate @ChangedBy parameter in UPDATE SPs
- ✅ Double `/api` in API URLs
- ✅ CORS errors (missing IDbConnection registration)
- ✅ SQLite package missing
- ✅ Connection string auto-detection (SQL Server vs SQLite)
- ✅ Code analysis warnings (CA1056, CA1848)

### Open
- ⚠️ Stage 3.5 React UI filters not implemented
- ⚠️ Master-Detail views not implemented
- ⚠️ Server-side Excel export not implemented
- ⚠️ Bulk operations not implemented
- ⚠️ Permissions/security not implemented

---

## Next Session Plan

### Priority 1: Complete Filtering (Stage 3.5)
**Estimated Time:** 2-3 hours

1. Create `ReactFilterComponentGenerator.cs`
2. Generate filter UI components based on indexes
3. Add API calls for filtered queries
4. Integrate filters with DataGrid
5. Test end-to-end filtering

### Priority 2: Master-Detail Views
**Estimated Time:** 1-2 weeks

1. **Analysis Phase** (2-3 hours)
   - Detect Foreign Key relationships
   - Design SQL View/SP templates
   - Plan API endpoint structure
   - Design React component hierarchy

2. **SQL Layer** (1 day)
   - Create `SpGetWithRelatedTemplate.cs`
   - Generate JOINed queries
   - Test performance with indexes

3. **Backend Layer** (1-2 days)
   - Repository methods for related data
   - Controller endpoints (e.g., `/api/customers/1/orders`)
   - DTOs for nested data

4. **Frontend Layer** (2-3 days)
   - Master-Detail component generator
   - Nested DataGrid implementation
   - Navigation breadcrumbs
   - State management

5. **Testing & Polish** (1 day)
   - End-to-end testing
   - Performance optimization
   - Documentation

### Priority 3: Additional Features
**Deferred to future sessions**

- Validation rules (FluentValidation, Yup)
- Audit trail automatic handling
- Bulk operations
- Dashboard/Analytics views
- Role-based permissions
- Localization (i18n)
- Mobile responsiveness enhancements

---

## Code Quality Standards

### C# Code
- ✅ StyleCop compliance
- ✅ Code analysis enabled (CA rules)
- ✅ XML documentation for all public APIs
- ✅ LoggerMessage delegates for performance
- ✅ Nullable reference types enabled
- ✅ Async/await patterns
- ✅ ConfigureAwait(false) in libraries

### TypeScript/React Code
- ✅ TypeScript strict mode
- ✅ ESLint configured
- ✅ Functional components (hooks)
- ✅ React Query for data fetching
- ✅ react-hook-form for forms
- ✅ Material-UI design system
- ✅ Proper error handling

### SQL Code
- ✅ Parameterized queries (no SQL injection)
- ✅ Stored procedures for all data access
- ✅ Proper indexing support
- ✅ NULL handling with ISNULL()
- ✅ OFFSET/FETCH for pagination
- ✅ Comments and documentation

---

## Performance Considerations

### Database
- Uses indexes for filtering (LIKE queries on indexed text columns)
- Pagination with OFFSET/FETCH (server-side)
- Stored procedures (pre-compiled, cached execution plans)
- Connection pooling (ADO.NET)

### Backend
- Dapper (lightweight ORM, ~2x faster than EF Core)
- Async/await throughout (non-blocking I/O)
- LoggerMessage delegates (high-performance logging)
- No AutoMapper overhead

### Frontend
- React Query caching (reduces API calls)
- DataGrid virtualization (MUI X)
- Code splitting (Vite)
- Client-side Excel export (no server load for small datasets)

---

## Security Considerations

### Implemented
- ✅ Parameterized queries (Dapper prevents SQL injection)
- ✅ CORS configured properly
- ✅ HTTPS redirection
- ✅ Input validation (ModelState, react-hook-form)

### Not Implemented (Future)
- ⚠️ Authentication/Authorization
- ⚠️ JWT tokens
- ⚠️ Role-based access control
- ⚠️ Rate limiting
- ⚠️ CSRF protection
- ⚠️ Content Security Policy

---

## Maintenance & Updates

### Updating TargCC Generator
1. Pull latest changes
2. Run `dotnet build`
3. Re-generate applications: `./test_targcc_v2.sh`
4. Review generated code changes
5. Re-run SQL scripts if schema changed
6. Test thoroughly

### Updating Generated Applications
- **Manual changes:** Use `.prt.tsx` files (preserved on regeneration)
- **Schema changes:** Re-run generator and merge carefully
- **New features:** Regenerate and integrate

---

## Support & Documentation

### Repository
- **Main:** `https://github.com/dorongut1/TargCC-Core-V2`
- **Branch:** `claude/review-test-script-01Te4Z88CcBDuBAs9H2PvxYk`
- **Issues:** Track bugs and feature requests via GitHub Issues

### Documentation Files
- `SPECIFICATION.md` (this file) - Feature specifications
- `README.md` - Getting started guide
- `CHANGELOG.md` - Version history (to be created)

### Contact
- Developer: Doron Gut
- Generated with: Claude Code (Anthropic)
- Session Date: 2025-12-05

---

## Appendix: Example Generated Code

### SQL Stored Procedure
```sql
-- File: sql/Customer.sql
CREATE OR ALTER PROCEDURE [dbo].[SP_GetFilteredCustomers]
    @Email NVARCHAR(255) = NULL,
    @City NVARCHAR(100) = NULL,
    @Skip INT = NULL,
    @Take INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [ID],
        [Name],
        [Email],
        [City],
        [CreatedDate],
        [CreatedBy],
        [ChangedDate],
        [ChangedBy]
    FROM [Customer]
    WHERE 1=1
      AND (@Email IS NULL OR [Email] LIKE '%' + @Email + '%')
      AND (@City IS NULL OR [City] = @City)
    ORDER BY [ID]
    OFFSET ISNULL(@Skip, 0) ROWS
    FETCH NEXT ISNULL(@Take, 2147483647) ROWS ONLY;
END
GO
```

### C# Repository Interface
```csharp
// File: Domain/Interfaces/ICustomerRepository.cs
public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Customer>> GetAllAsync(int? skip = null, int? take = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Customer>> GetFilteredAsync(string? email = null, string? city = null, int? skip = null, int? take = null, CancellationToken cancellationToken = default);
    Task AddAsync(Customer entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Customer entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
```

### C# API Controller
```csharp
// File: API/Controllers/CustomersController.cs
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _repository;
    private readonly ILogger<CustomersController> _logger;

    [HttpGet("filter")]
    [ProducesResponseType(typeof(IEnumerable<Customer>), 200)]
    public async Task<ActionResult<IEnumerable<Customer>>> GetFiltered(
        [FromQuery] string? email = null,
        [FromQuery] string? city = null)
    {
        var entities = await _repository.GetFilteredAsync(email, city).ConfigureAwait(false);
        return Ok(entities);
    }

    // ... other methods
}
```

### TypeScript React Component
```typescript
// File: client/src/components/customers/CustomerList.tsx
import React from 'react';
import { useNavigate } from 'react-router-dom';
import * as XLSX from 'xlsx';
import { DataGrid, GridColDef } from '@mui/x-data-grid';
import { Button, Box } from '@mui/material';
import { Add as AddIcon, FileDownload as FileDownloadIcon } from '@mui/icons-material';
import { useCustomers, useDeleteCustomer } from '../../hooks/useCustomer';

export const CustomerList: React.FC = () => {
  const navigate = useNavigate();
  const [filters, setFilters] = React.useState<CustomerFilters>({});
  const { data: customers, isLoading, error } = useCustomers(filters);
  const { mutate: deleteEntity } = useDeleteCustomer();

  const handleExportToExcel = () => {
    if (!customers || customers.length === 0) {
      alert('No data to export');
      return;
    }

    const ws = XLSX.utils.json_to_sheet(customers);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Customers');
    XLSX.writeFile(wb, `Customers_${new Date().toISOString().split('T')[0]}.xlsx`);
  };

  const columns: GridColDef<Customer>[] = [
    { field: 'id', headerName: 'ID', width: 90 },
    { field: 'name', headerName: 'Name', width: 200 },
    { field: 'email', headerName: 'Email', width: 250 },
    // ... more columns
  ];

  if (isLoading) return <CircularProgress />;
  if (error) return <Alert severity="error">Failed to load customers</Alert>;

  return (
    <Box sx={{ height: 600, width: '100%' }}>
      <Box sx={{ mb: 2, display: 'flex', gap: 2 }}>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => navigate('/customers/new')}
        >
          Create Customer
        </Button>
        <Button
          variant="outlined"
          startIcon={<FileDownloadIcon />}
          onClick={handleExportToExcel}
        >
          Export to Excel
        </Button>
      </Box>
      <DataGrid
        rows={customers || []}
        columns={columns}
        pageSizeOptions={[5, 10, 25]}
        checkboxSelection
        disableRowSelectionOnClick
      />
    </Box>
  );
};
```

---

**End of Specification Document**

*This document will be updated as new features are implemented.*
