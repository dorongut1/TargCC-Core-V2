# CCV2 Enhancement Plan
**Version:** 1.1
**Date:** 2025-12-31
**Base Version:** v1.0-stable-working

## Executive Summary

This document outlines planned enhancements to TargCC-Core-V2 (CCV2) code generator. The current version (v1.0-stable-working) successfully generates working CRUD applications with:
- 8 table types fully functional (Customer, Product, Order, OrderLine, Payment, etc.)
- Complete enum system (c_Enumeration → TypeScript/C# → React hooks → API → UI dropdowns)
- Clean Architecture (Domain → Application → Infrastructure → API)
- Zero compilation errors on generated code

**Current Gap:** Views (especially MN Views) don't get UI screens, limiting developers' ability to create report-based functionality.

**Goal:** Transform CCV2 from a pure CRUD generator into a comprehensive business application generator that includes reports, master-detail forms, and advanced UI patterns.

---

## Priority 1: MN Views → Report Screens (CRITICAL)

### Problem
- Views starting with "MN" (Manual Views) are user-created SQL views
- Currently, CCV2 skips views entirely - no UI screens generated
- Developers create these views to present business data but have no UI to display them
- Missing a critical development pattern: SQL VIEW → Auto-generated report screen → Developer upgrades to functional screen

### User Requirement
**Quote:** "אני רוצה VIEWS כן יקבלו מסך פשוט יהיו מסך של REPORT או משהו כזה כדי שיהיה אפשר לראות את הנתונים ולהציג אותם בצורה יפה ונוחה"

Translation: "I want VIEWS to get a simple screen, like a REPORT screen, so it's possible to see the data and display it nicely and conveniently"

### Solution Design

#### 1. View Detection Logic
```csharp
// In SchemaAnalyzer
public class ViewInfo
{
    public string ViewName { get; set; }
    public string SchemaName { get; set; }
    public List<ViewColumn> Columns { get; set; }
    public ViewType Type { get; set; }  // Manual (MN), ComboList, Other
}

public enum ViewType
{
    Manual,      // Starts with "MN" or "mn" → Generate report screen
    ComboList,   // Starts with "ccvwComboList_" → Skip (used for dropdowns)
    Other        // Unknown pattern → Skip for now
}
```

**Detection:**
- `mn*` or `MN*` → ViewType.Manual
- `ccvwComboList_*` → ViewType.ComboList
- Everything else → ViewType.Other

#### 2. Backend Generation (Read-Only Repository)

**File:** `src/TargCC.Core.Generators/Repositories/ViewRepositoryGenerator.cs` (NEW)

Generate read-only repository for Manual Views:

```csharp
public interface IMnCustomerOrdersRepository
{
    Task<IEnumerable<MnCustomerOrders>> GetAllAsync();
    Task<IEnumerable<MnCustomerOrders>> SearchAsync(string searchTerm);
}

public class MnCustomerOrdersRepository : IMnCustomerOrdersRepository
{
    private readonly IConfiguration _configuration;

    public async Task<IEnumerable<MnCustomerOrders>> GetAllAsync()
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        return await connection.QueryAsync<MnCustomerOrders>("SELECT * FROM dbo.mnCustomerOrders");
    }

    public async Task<IEnumerable<MnCustomerOrders>> SearchAsync(string searchTerm)
    {
        // Generate search across all text columns
    }
}
```

**Features:**
- GetAllAsync() - retrieve all rows
- SearchAsync() - full-text search across string columns
- No Update/Delete/Create methods (read-only)

#### 3. Application Layer (CQRS Queries)

**File:** `src/TargCC.Core.Generators/CQRS/ViewQueryGenerator.cs` (NEW)

```csharp
// Query
public record GetAllMnCustomerOrdersQuery : IRequest<List<MnCustomerOrdersDto>>;

// Handler
public class GetAllMnCustomerOrdersQueryHandler : IRequestHandler<GetAllMnCustomerOrdersQuery, List<MnCustomerOrdersDto>>
{
    private readonly IMnCustomerOrdersRepository _repository;

    public async Task<List<MnCustomerOrdersDto>> Handle(...)
    {
        var results = await _repository.GetAllAsync();
        return results.Select(r => r.ToDto()).ToList();
    }
}

// Search Query
public record SearchMnCustomerOrdersQuery(string SearchTerm) : IRequest<List<MnCustomerOrdersDto>>;
```

#### 4. API Controller (Read-Only Endpoints)

**File:** `src/TargCC.Core.Generators/API/ViewControllerGenerator.cs` (NEW)

```csharp
[ApiController]
[Route("api/[controller]")]
public class MnCustomerOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpGet]
    public async Task<ActionResult<List<MnCustomerOrdersDto>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllMnCustomerOrdersQuery());
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<MnCustomerOrdersDto>>> Search([FromQuery] string term)
    {
        var result = await _mediator.Send(new SearchMnCustomerOrdersQuery(term));
        return Ok(result);
    }
}
```

**No POST/PUT/DELETE endpoints** - views are read-only.

#### 5. React Report Screen

**File:** `src/TargCC.Core.Generators/UI/Components/ReactReportComponentGenerator.cs` (NEW)

Generate Material-UI DataGrid-based report screen:

```tsx
export function MnCustomerOrdersReport() {
  const { data: rows, isLoading } = useQuery({
    queryKey: ['mnCustomerOrders'],
    queryFn: () => api.get('/api/MnCustomerOrders').then(res => res.data)
  });

  const [searchTerm, setSearchTerm] = useState('');

  const columns: GridColDef[] = [
    { field: 'customerName', headerName: 'Customer', width: 200 },
    { field: 'orderDate', headerName: 'Order Date', width: 150, type: 'date' },
    { field: 'totalAmount', headerName: 'Total', width: 120, type: 'number' },
    // ... auto-generated from view columns
  ];

  return (
    <Paper>
      <Toolbar>
        <Typography variant="h6">Customer Orders Report</Typography>
        <TextField
          label="Search"
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
        />
        <Button startIcon={<DownloadIcon />}>Export CSV</Button>
        <Button startIcon={<PrintIcon />}>Print</Button>
      </Toolbar>

      <DataGrid
        rows={rows ?? []}
        columns={columns}
        loading={isLoading}
        autoHeight
        pageSizeOptions={[10, 25, 50, 100]}
        initialState={{
          pagination: { paginationModel: { pageSize: 25 } }
        }}
      />
    </Paper>
  );
}
```

**Features:**
- Material-UI DataGrid with sorting, filtering, pagination
- Search bar (queries SearchAsync endpoint)
- Export to CSV button
- Print button
- Auto-detected column types (string, number, date, boolean)
- Responsive design

#### 6. Integration into ProjectGenerationService

Update `ProjectGenerationService.cs`:

```csharp
// After Step 5: Generate React Components
Console.WriteLine("\n=== Step 5b: Generating React Report Screens ===");

var manualViews = schemaAnalysis.Views.Where(v => v.Type == ViewType.Manual).ToList();
Console.WriteLine($"Found {manualViews.Count} manual views (MN)");

foreach (var view in manualViews)
{
    Console.WriteLine($"  - Generating report for: {view.ViewName}");

    // Generate backend
    GenerateViewRepository(view);
    GenerateViewQueries(view);
    GenerateViewController(view);

    // Generate frontend
    GenerateReactReportScreen(view);
    GenerateViewApiClient(view);

    // Register in DI
    RegisterViewServices(view);
}
```

#### 7. Type Detection for View Columns

Smart column type detection:

```csharp
private static string DetectColumnType(ViewColumn column)
{
    var typeLower = column.DataType.ToLower();

    if (typeLower.Contains("int") || typeLower.Contains("decimal") || typeLower.Contains("money"))
        return "number";

    if (typeLower.Contains("date") || typeLower.Contains("time"))
        return "date";

    if (typeLower.Contains("bit") || typeLower.Contains("bool"))
        return "boolean";

    return "string";
}
```

#### 8. Export to CSV Functionality

```tsx
const exportToCsv = () => {
  const csv = rows.map(row =>
    columns.map(col => row[col.field]).join(',')
  ).join('\n');

  const blob = new Blob([csv], { type: 'text/csv' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = 'mnCustomerOrders.csv';
  a.click();
};
```

### Testing Checklist
- [ ] Create MN view in OrdersDB (e.g., mnCustomerOrders)
- [ ] Run CCV2 generation
- [ ] Verify ViewRepository generated
- [ ] Verify ViewController generated with GET endpoints only
- [ ] Verify React report screen generated
- [ ] Verify DataGrid renders with data
- [ ] Test search functionality
- [ ] Test CSV export
- [ ] Test print functionality
- [ ] Verify pagination works
- [ ] Verify column sorting works

### Success Criteria
✅ All MN views get read-only report screens
✅ Reports load data from view via API
✅ Search works across text columns
✅ Export to CSV works
✅ DataGrid features (sort, filter, paginate) work
✅ Zero compilation errors
✅ Developer can easily upgrade report screen to functional screen by copying/modifying component

---

## Priority 2: Master-Detail Forms

### Problem
Current CCV2 generates separate screens for parent and child tables:
- Order screen (standalone)
- OrderLine screen (standalone)

Business users want to edit Order + OrderLines in **single screen**.

### Solution Design

#### 1. Detect Master-Detail Relationships

```csharp
public class MasterDetailRelationship
{
    public Table MasterTable { get; set; }
    public Table DetailTable { get; set; }
    public ForeignKey ForeignKey { get; set; }
    public bool IsOneToMany { get; set; }
}
```

**Detection Logic:**
- Find all foreign keys pointing from Detail → Master
- Check if Detail table name contains Master table name (e.g., OrderLine → Order)
- Or check if FK column matches pattern (e.g., CustomerID in Order table)

**Example:**
- Order (Master) ← OrderLine (Detail) via FK_OrderLine_Order

#### 2. React Master-Detail Form Component

**File:** `src/TargCC.Core.Generators/UI/Components/ReactMasterDetailFormGenerator.cs` (NEW)

```tsx
export function OrderMasterDetailForm() {
  const { id } = useParams();
  const [order, setOrder] = useState<Order>({});
  const [orderLines, setOrderLines] = useState<OrderLine[]>([]);

  // Master section
  return (
    <Paper>
      <Typography variant="h5">Order Details</Typography>

      {/* Master Form */}
      <Grid container spacing={2}>
        <Grid item xs={6}>
          <TextField label="Customer" {...} />
        </Grid>
        <Grid item xs={6}>
          <TextField label="Order Date" {...} />
        </Grid>
      </Grid>

      {/* Detail Grid */}
      <Box mt={4}>
        <Typography variant="h6">Order Lines</Typography>
        <Button onClick={handleAddLine}>Add Line</Button>

        <DataGrid
          rows={orderLines}
          columns={[
            { field: 'product', headerName: 'Product' },
            { field: 'quantity', headerName: 'Qty', editable: true },
            { field: 'price', headerName: 'Price', editable: true },
            { field: 'total', headerName: 'Total', valueGetter: (params) => params.row.quantity * params.row.price }
          ]}
          processRowUpdate={handleRowUpdate}
        />
      </Box>

      {/* Save Both */}
      <Button onClick={handleSaveAll}>Save Order & Lines</Button>
    </Paper>
  );
}
```

#### 3. Backend API Support

**Option A:** Nested DTOs
```csharp
public class OrderWithLinesDto
{
    public OrderDto Order { get; set; }
    public List<OrderLineDto> OrderLines { get; set; }
}

[HttpPost]
public async Task<ActionResult> CreateOrderWithLines([FromBody] OrderWithLinesDto dto)
{
    // Transaction: Create Order, then create OrderLines
}
```

**Option B:** Separate endpoints (simpler)
- Use existing Order and OrderLine endpoints
- Frontend handles coordination with transactions

**Recommended:** Option B initially (no backend changes needed), Option A as enhancement.

#### 4. Generator Logic

```csharp
// In ProjectGenerationService
var masterDetailPairs = DetectMasterDetailRelationships(tables);

foreach (var pair in masterDetailPairs)
{
    Console.WriteLine($"Generating Master-Detail form: {pair.MasterTable.Name} + {pair.DetailTable.Name}");
    GenerateMasterDetailForm(pair);
}
```

### Testing Checklist
- [ ] Detect Order ← OrderLine relationship
- [ ] Generate OrderMasterDetailForm.tsx
- [ ] Master section renders Order fields
- [ ] Detail section renders OrderLines grid
- [ ] Add new line button works
- [ ] Edit line in grid works
- [ ] Delete line works
- [ ] Save master + details in transaction
- [ ] Validation works on both levels

### Success Criteria
✅ Master-Detail forms generated for detected FK relationships
✅ Single screen edits parent + children
✅ Grid supports inline editing
✅ Totals calculated automatically
✅ Transaction ensures data consistency

---

## Priority 3: ComboList Dropdown Integration

### Problem
Current CCV2 generates combo views (ccvwComboList_Customer) but doesn't integrate them into forms automatically.

Forms show hardcoded TODO dropdowns:
```tsx
<Select label="Customer">
  <MenuItem value="">TODO: Load from API</MenuItem>
</Select>
```

### Solution Design

#### 1. Detect ComboList Views

Views matching pattern `ccvwComboList_{TableName}`:
```sql
-- Auto-generated by CCV2
CREATE VIEW ccvwComboList_Customer AS
SELECT
    ID,
    CustomerName + ' (' + CustomerCode + ')' AS DisplayText
FROM dbo.Customer
WHERE DeletedOn IS NULL
ORDER BY CustomerName
```

#### 2. Generate React Hook

**File:** `src/TargCC.Core.Generators/React/ComboListHooksGenerator.cs` (NEW)

```tsx
// useCustomerCombo.ts
export function useCustomerCombo() {
  return useQuery({
    queryKey: ['combo', 'customer'],
    queryFn: () => api.get('/api/ComboList/Customer').then(res => res.data)
  });
}
```

#### 3. Generate Backend API

**File:** `src/TargCC.Core.Generators/API/ComboListControllerGenerator.cs` (NEW)

```csharp
[ApiController]
[Route("api/[controller]")]
public class ComboListController : ControllerBase
{
    private readonly IConfiguration _configuration;

    [HttpGet("Customer")]
    public async Task<ActionResult<List<ComboItem>>> GetCustomerCombo()
    {
        using var connection = new SqlConnection(...);
        var items = await connection.QueryAsync<ComboItem>(
            "SELECT ID, DisplayText FROM dbo.ccvwComboList_Customer"
        );
        return Ok(items);
    }
}

public record ComboItem(int ID, string DisplayText);
```

#### 4. Update ReactFormComponentGenerator

Detect FK columns and generate combo dropdowns:

```csharp
private static string GenerateForeignKeyField(Column column, Table table)
{
    // column.Name = "CustomerID"
    var referencedTable = DetectReferencedTable(column); // "Customer"
    var hookName = $"use{referencedTable}Combo";
    var dataVar = $"{ToCamelCase(referencedTable)}Options";

    return $@"
    const {{ data: {dataVar} }} = {hookName}();

    <Select label=""Customer"" name=""CustomerID"">
      <MenuItem value="""">Select...</MenuItem>
      {{{dataVar}?.map(item => (
        <MenuItem key={{item.id}} value={{item.id}}>
          {{item.displayText}}
        </MenuItem>
      ))}}
    </Select>
    ";
}
```

### Testing Checklist
- [ ] ComboList views detected
- [ ] ComboListController generated
- [ ] React hooks generated (useCustomerCombo, useProductCombo)
- [ ] Forms use combo hooks automatically
- [ ] Dropdowns load data from API
- [ ] Display text shows correctly (e.g., "Acme Corp (ACME)")

### Success Criteria
✅ All FK columns automatically get combo dropdowns
✅ Combos load from ccvwComboList views
✅ Display text formatting works
✅ No hardcoded TODO comments in forms

---

## Priority 4: Auto Filters

### Problem
List screens show all records without filtering capabilities.

### Solution Design

Generate filter components for common column types:

```tsx
export function CustomerList() {
  const [filters, setFilters] = useState({
    customerName: '',
    status: '',
    dateFrom: null,
    dateTo: null
  });

  return (
    <Box>
      <Paper sx={{ p: 2, mb: 2 }}>
        <Grid container spacing={2}>
          <Grid item xs={3}>
            <TextField
              label="Customer Name"
              value={filters.customerName}
              onChange={(e) => setFilters({...filters, customerName: e.target.value})}
            />
          </Grid>
          <Grid item xs={3}>
            <Select label="Status" {...}>
              {/* Auto-generated from enum */}
            </Select>
          </Grid>
          <Grid item xs={2}>
            <DatePicker label="From Date" {...} />
          </Grid>
          <Grid item xs={2}>
            <DatePicker label="To Date" {...} />
          </Grid>
          <Grid item xs={2}>
            <Button onClick={handleApplyFilters}>Apply</Button>
          </Grid>
        </Grid>
      </Paper>

      <DataGrid ... />
    </Box>
  );
}
```

**Backend Support:**
```csharp
public record GetCustomersQuery(
    string? CustomerName,
    string? Status,
    DateTime? DateFrom,
    DateTime? DateTo
) : IRequest<List<CustomerDto>>;
```

### Success Criteria
✅ Filter UI generated for string, enum, date, boolean columns
✅ Filters applied on backend (SQL WHERE clauses)
✅ Clear filters button works
✅ Filter state persisted in URL query params

---

## Priority 5: Quick Actions

### Problem
List screens require navigating to detail screen to perform simple actions (e.g., Delete, Activate, Archive).

### Solution Design

Add action column to DataGrid:

```tsx
const columns: GridColDef[] = [
  // ... data columns
  {
    field: 'actions',
    headerName: 'Actions',
    width: 150,
    renderCell: (params) => (
      <Box>
        <IconButton onClick={() => handleEdit(params.row.id)}>
          <EditIcon />
        </IconButton>
        <IconButton onClick={() => handleDelete(params.row.id)}>
          <DeleteIcon />
        </IconButton>
        {params.row.status === 'Draft' && (
          <IconButton onClick={() => handleActivate(params.row.id)}>
            <CheckIcon />
          </IconButton>
        )}
      </Box>
    )
  }
];
```

**Generated Actions:**
- Edit (always)
- Delete (if table has DeletedOn column → soft delete)
- Custom actions based on status enum values

### Success Criteria
✅ Action column added to all list screens
✅ Edit/Delete icons work
✅ Confirmation dialog for delete
✅ Status-based actions generated (Activate, Archive, etc.)

---

## Priority 6: Export Buttons

### Problem
Users need to export data to Excel/CSV for reporting.

### Solution Design

Add export buttons to list screens:

```tsx
<Toolbar>
  <Button startIcon={<DownloadIcon />} onClick={handleExportCsv}>
    Export CSV
  </Button>
  <Button startIcon={<DownloadIcon />} onClick={handleExportExcel}>
    Export Excel
  </Button>
</Toolbar>
```

**CSV Export (Client-Side):**
```tsx
const handleExportCsv = () => {
  const csv = rows.map(row =>
    columns.map(col => row[col.field]).join(',')
  ).join('\n');

  const blob = new Blob([csv], { type: 'text/csv' });
  saveAs(blob, 'customers.csv');
};
```

**Excel Export (Server-Side):**
```csharp
[HttpGet("export/excel")]
public async Task<IActionResult> ExportToExcel()
{
    var customers = await _mediator.Send(new GetAllCustomersQuery());

    using var package = new ExcelPackage();
    var worksheet = package.Workbook.Worksheets.Add("Customers");
    worksheet.Cells["A1"].LoadFromCollection(customers, true);

    return File(package.GetAsByteArray(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "customers.xlsx");
}
```

**Dependencies:**
- Client: `file-saver` package
- Server: `EPPlus` or `ClosedXML` NuGet package

### Success Criteria
✅ Export CSV button on all list screens
✅ Export Excel button on all list screens
✅ CSV export works (client-side)
✅ Excel export works (server-side)
✅ Exported files include column headers

---

## Priority 7: Validation from Schema

### Problem
Forms don't validate based on database schema constraints (required fields, max length, etc.).

### Solution Design

#### 1. Extract Validation Rules from Schema

```csharp
public class ValidationRule
{
    public string ColumnName { get; set; }
    public bool IsRequired { get; set; }  // NOT NULL
    public int? MaxLength { get; set; }   // VARCHAR(50)
    public string? Pattern { get; set; }  // Email, Phone, etc.
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
}
```

#### 2. Generate Yup Schema (React)

```tsx
import * as yup from 'yup';

const customerSchema = yup.object({
  customerName: yup.string()
    .required('Customer Name is required')
    .max(100, 'Customer Name cannot exceed 100 characters'),

  email: yup.string()
    .email('Invalid email format')
    .max(255, 'Email cannot exceed 255 characters'),

  phoneNumber: yup.string()
    .matches(/^[0-9-]+$/, 'Phone number must contain only digits and dashes')
    .max(20, 'Phone number cannot exceed 20 characters'),

  creditLimit: yup.number()
    .min(0, 'Credit limit cannot be negative')
    .max(999999.99, 'Credit limit too large')
});
```

#### 3. Generate FluentValidation (Backend)

```csharp
public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer Name is required")
            .MaximumLength(100).WithMessage("Customer Name cannot exceed 100 characters");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Credit limit cannot be negative")
            .LessThanOrEqualTo(999999.99m).WithMessage("Credit limit too large");
    }
}
```

#### 4. Integration

```csharp
// Generate validation schema file
GenerateYupSchema(table);  // customer.schema.ts
GenerateFluentValidation(table);  // CreateCustomerCommandValidator.cs

// Use in form
const { register, handleSubmit, formState: { errors } } = useForm({
  resolver: yupResolver(customerSchema)
});
```

### Success Criteria
✅ Required fields validated (both client and server)
✅ Max length enforced
✅ Email/Phone patterns validated
✅ Numeric ranges enforced
✅ Validation errors displayed in UI
✅ Backend returns 400 with validation errors

---

## Priority 8: Audit Columns Handling

### Problem
Audit columns (CreatedBy, CreatedOn, UpdatedBy, UpdatedOn, DeletedOn, DeletedBy) are generated in forms but should be hidden/automatic.

### Solution Design

#### 1. Detect Audit Columns

```csharp
private static readonly HashSet<string> AuditColumns = new()
{
    "CreatedBy", "CreatedOn",
    "UpdatedBy", "UpdatedOn",
    "DeletedBy", "DeletedOn"
};

private static bool IsAuditColumn(Column column)
{
    return AuditColumns.Contains(column.Name);
}
```

#### 2. Exclude from Forms

```csharp
// In ReactFormComponentGenerator
var editableColumns = GetDataColumns(table)
    .Where(c => !c.IsPrimaryKey && !c.IsIdentity && !IsAuditColumn(c))
    .ToList();
```

#### 3. Auto-Populate in Backend

```csharp
public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, int>
{
    private readonly ICurrentUserService _currentUserService;

    public async Task<int> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = new Customer
        {
            CustomerName = request.CustomerName,
            // ... other fields

            // Auto-populate audit columns
            CreatedBy = _currentUserService.UserId,
            CreatedOn = DateTime.UtcNow
        };

        await _repository.CreateAsync(customer);
        return customer.ID;
    }
}
```

#### 4. Display in Read-Only Mode

Show audit info at bottom of form (read-only):

```tsx
<Box mt={4} p={2} bgcolor="grey.100">
  <Typography variant="caption">
    Created by {customer.createdBy} on {formatDate(customer.createdOn)}
  </Typography>
  {customer.updatedOn && (
    <Typography variant="caption">
      Last updated by {customer.updatedBy} on {formatDate(customer.updatedOn)}
    </Typography>
  )}
</Box>
```

### Success Criteria
✅ Audit columns excluded from editable form fields
✅ CreatedBy/CreatedOn auto-populated on create
✅ UpdatedBy/UpdatedOn auto-populated on update
✅ Audit info displayed read-only at bottom of forms
✅ Soft delete sets DeletedBy/DeletedOn instead of hard delete

---

## Priority 9: Permissions & Roles

### Problem
All generated endpoints are public - no authorization checks.

### Solution Design

#### 1. Generate Authorization Policies

```csharp
// In Startup/Program.cs
services.AddAuthorization(options =>
{
    options.AddPolicy("CanViewCustomers", policy =>
        policy.RequireRole("Admin", "Sales"));

    options.AddPolicy("CanEditCustomers", policy =>
        policy.RequireRole("Admin", "Sales"));

    options.AddPolicy("CanDeleteCustomers", policy =>
        policy.RequireRole("Admin"));
});
```

#### 2. Apply to Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]  // Require authentication
public class CustomersController : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "CanViewCustomers")]
    public async Task<ActionResult<List<CustomerDto>>> GetAll() { }

    [HttpPost]
    [Authorize(Policy = "CanEditCustomers")]
    public async Task<ActionResult<int>> Create([FromBody] CreateCustomerCommand command) { }

    [HttpDelete("{id}")]
    [Authorize(Policy = "CanDeleteCustomers")]
    public async Task<ActionResult> Delete(int id) { }
}
```

#### 3. Frontend Permission Checks

```tsx
export function CustomerList() {
  const { hasPermission } = useAuth();

  return (
    <Box>
      {hasPermission('CanViewCustomers') ? (
        <>
          <DataGrid ... />
          {hasPermission('CanEditCustomers') && (
            <Button onClick={handleCreate}>Add Customer</Button>
          )}
        </>
      ) : (
        <Typography>You don't have permission to view customers</Typography>
      )}
    </Box>
  );
}
```

#### 4. Configuration

Allow developer to configure permissions via config file:

```json
// targcc-permissions.json
{
  "Customer": {
    "View": ["Admin", "Sales", "Support"],
    "Create": ["Admin", "Sales"],
    "Update": ["Admin", "Sales"],
    "Delete": ["Admin"]
  },
  "Order": {
    "View": ["Admin", "Sales", "Warehouse"],
    "Create": ["Admin", "Sales"],
    "Update": ["Admin", "Sales"],
    "Delete": ["Admin"]
  }
}
```

### Success Criteria
✅ Authorization policies generated per table
✅ Controllers decorated with [Authorize] attributes
✅ Frontend checks permissions before showing actions
✅ 403 Forbidden returned for unauthorized requests
✅ Configuration file allows customizing permissions

---

## Priority 10: Dashboard Widgets

### Problem
Generated apps have no landing page/dashboard - users land on empty screen.

### Solution Design

Generate basic dashboard with:

#### 1. Metric Cards

```tsx
<Grid container spacing={3}>
  <Grid item xs={3}>
    <Card>
      <CardContent>
        <Typography variant="h4">152</Typography>
        <Typography color="textSecondary">Total Customers</Typography>
        <Typography variant="body2" color="success.main">
          +12% from last month
        </Typography>
      </CardContent>
    </Card>
  </Grid>

  <Grid item xs={3}>
    <Card>
      <CardContent>
        <Typography variant="h4">$45,230</Typography>
        <Typography color="textSecondary">Monthly Revenue</Typography>
        <Typography variant="body2" color="success.main">
          +8% from last month
        </Typography>
      </CardContent>
    </Card>
  </Grid>
</Grid>
```

#### 2. Recent Activity Table

```tsx
<Paper>
  <Typography variant="h6">Recent Orders</Typography>
  <Table>
    <TableHead>
      <TableRow>
        <TableCell>Order #</TableCell>
        <TableCell>Customer</TableCell>
        <TableCell>Date</TableCell>
        <TableCell>Amount</TableCell>
      </TableRow>
    </TableHead>
    <TableBody>
      {recentOrders?.map(order => (
        <TableRow key={order.id}>
          <TableCell>{order.orderNumber}</TableCell>
          <TableCell>{order.customerName}</TableCell>
          <TableCell>{formatDate(order.orderDate)}</TableCell>
          <TableCell>{formatCurrency(order.totalAmount)}</TableCell>
        </TableRow>
      ))}
    </TableBody>
  </Table>
</Paper>
```

#### 3. Charts

```tsx
import { LineChart, Line, XAxis, YAxis, Tooltip } from 'recharts';

<Paper>
  <Typography variant="h6">Sales Trend</Typography>
  <LineChart width={600} height={300} data={salesData}>
    <XAxis dataKey="month" />
    <YAxis />
    <Tooltip />
    <Line type="monotone" dataKey="sales" stroke="#8884d8" />
  </LineChart>
</Paper>
```

#### 4. Backend Endpoints

```csharp
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    [HttpGet("metrics")]
    public async Task<ActionResult<DashboardMetrics>> GetMetrics()
    {
        return Ok(new DashboardMetrics
        {
            TotalCustomers = await _customerRepo.CountAsync(),
            MonthlyRevenue = await _orderRepo.SumThisMonth(o => o.TotalAmount),
            // ...
        });
    }

    [HttpGet("recent-orders")]
    public async Task<ActionResult<List<RecentOrderDto>>> GetRecentOrders()
    {
        return Ok(await _orderRepo.GetRecentAsync(10));
    }
}
```

### Success Criteria
✅ Dashboard route generated (/)
✅ Metric cards show key statistics
✅ Recent activity table works
✅ Basic chart displays trends
✅ Data refreshes automatically

---

## Implementation Phases

### Phase 1: Core Enhancements (v1.1)
- ✅ Priority 1: MN Views → Report Screens (CRITICAL)
- ✅ Priority 3: ComboList Dropdown Integration
- ✅ Priority 8: Audit Columns Handling

**Estimated Effort:** 2-3 days
**Testing:** Full regeneration of OrdersDB

### Phase 2: Advanced UI (v1.2)
- ✅ Priority 2: Master-Detail Forms
- ✅ Priority 4: Auto Filters
- ✅ Priority 5: Quick Actions
- ✅ Priority 6: Export Buttons

**Estimated Effort:** 3-4 days
**Testing:** Full regeneration of OrdersDB

### Phase 3: Quality & Security (v1.3)
- ✅ Priority 7: Validation from Schema
- ✅ Priority 9: Permissions & Roles

**Estimated Effort:** 2-3 days
**Testing:** Full regeneration of OrdersDB

### Phase 4: Analytics (v1.4)
- ✅ Priority 10: Dashboard Widgets

**Estimated Effort:** 1-2 days
**Testing:** Full regeneration of OrdersDB

---

## Testing Strategy

After each phase:

1. **Delete Generated Code:**
   ```bash
   rm -rf C:/Disk1/orders/Generated
   ```

2. **Run CCV2:**
   ```bash
   cd C:/Disk1/TargCC-Core-V2
   dotnet run --project src/TargCC.CLI -- generate --connection "Server=.;Database=OrdersDB;Trusted_Connection=True;TrustServerCertificate=True;" --output C:/Disk1/orders/Generated --namespace OrdersManagement
   ```

3. **Build Backend:**
   ```bash
   cd C:/Disk1/orders/Generated
   dotnet build
   ```
   Expected: 0 errors

4. **Build Frontend:**
   ```bash
   cd C:/Disk1/orders/Generated/client
   npm install
   npm run build
   ```
   Expected: 0 errors

5. **Run Application:**
   ```bash
   # Terminal 1: Backend
   cd C:/Disk1/orders/Generated
   dotnet run --project src/OrdersManagement.API

   # Terminal 2: Frontend
   cd C:/Disk1/orders/Generated/client
   npm run dev
   ```

6. **Manual Testing Checklist:**
   - [ ] All CRUD screens load
   - [ ] All MN view report screens load
   - [ ] Enum dropdowns populate
   - [ ] ComboList dropdowns populate
   - [ ] Master-detail forms work
   - [ ] Filters apply correctly
   - [ ] Export CSV/Excel works
   - [ ] Validation errors display
   - [ ] Audit columns hidden/auto-populated
   - [ ] Dashboard loads with metrics

---

## Git Workflow

### Before Each Phase

```bash
# Create feature branch
git checkout -b feature/mn-views-report-screens

# Work on implementation...

# Commit frequently
git add .
git commit -m "WIP: Generate ViewRepository for MN views"
```

### After Phase Completion

```bash
# Final commit
git add .
git commit -m "feat: Add MN Views report screen generation (Priority 1)

- Added ViewRepositoryGenerator for read-only view access
- Added ViewControllerGenerator with GET endpoints only
- Added ReactReportComponentGenerator with DataGrid
- Added CSV export functionality
- Updated ProjectGenerationService to detect MN views
- Tested on OrdersDB: all MN views get report screens

Closes #[issue-number]
"

# Create tag
git tag -a v1.1-mn-views -m "v1.1: MN Views Report Screens"

# Push
git push origin feature/mn-views-report-screens --tags

# Merge to feature/legacy-compatibility
git checkout feature/legacy-compatibility
git merge feature/mn-views-report-screens
git push origin feature/legacy-compatibility
```

---

## Success Metrics

### v1.1 (Phase 1)
- ✅ 100% of MN views get report screens
- ✅ 100% of FK columns use combo dropdowns
- ✅ 0 audit columns appear in forms

### v1.2 (Phase 2)
- ✅ Master-detail forms work for all detected relationships
- ✅ Filters work on all list screens
- ✅ Export buttons work on all list screens

### v1.3 (Phase 3)
- ✅ Validation rules match schema constraints
- ✅ Authorization policies generated for all tables

### v1.4 (Phase 4)
- ✅ Dashboard loads with real metrics
- ✅ Charts display business data

### Overall
- ✅ 0 compilation errors on generated code
- ✅ 0 runtime errors on initial load
- ✅ Developer can build functional business app from generated code in <1 hour

---

## Risk Mitigation

### Risk 1: Breaking Changes
**Mitigation:** Always tag stable versions before major refactoring. Use feature branches.

### Risk 2: Complex SQL Views
**Mitigation:** Start with simple MN views. Add support for complex views (CTEs, JOINs) iteratively.

### Risk 3: Performance
**Mitigation:** Add pagination to all list screens. Lazy-load combo lists.

### Risk 4: Schema Changes
**Mitigation:** Warn user if regenerating over existing code. Offer merge strategies.

---

## Future Considerations (Post v1.4)

- Multi-language support (i18n)
- Dark mode support
- Mobile-responsive layouts
- Real-time updates (SignalR)
- Advanced search (Elasticsearch)
- Batch operations
- Import from Excel/CSV
- Email templates
- PDF report generation
- Scheduled jobs/background tasks
- API documentation (Swagger UI enhancements)
- Unit test generation
- Integration test generation

---

## Conclusion

This plan transforms CCV2 from a basic CRUD generator into a comprehensive business application generator. Priority 1 (MN Views → Report Screens) addresses the user's immediate need and unlocks a powerful development pattern: SQL VIEW → Auto-generated report → Developer-enhanced functional screen.

Each phase builds on previous work while maintaining backward compatibility. The testing strategy ensures quality, and the Git workflow preserves stability.

**Next Steps:**
1. Review and approve this plan
2. Begin Phase 1 implementation
3. Test on OrdersDB
4. Iterate based on feedback

---

**Document Status:** Ready for Review
**Author:** Claude Sonnet 4.5 (via Claude Code)
**Last Updated:** 2025-12-31
