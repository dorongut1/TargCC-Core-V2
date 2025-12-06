# Master-Detail Views - Implementation Specification

## 1. Overview

This document specifies the implementation of Master-Detail Views in TargCC - a feature that automatically generates UI components and backend methods for displaying related data based on Foreign Key relationships.

**Goal**: Given a table with Foreign Keys (e.g., `Order` with `CustomerID`), automatically generate:
- SQL stored procedures to fetch related records
- Repository methods to call these procedures
- API endpoints to expose related data
- React hooks to consume the endpoints
- React Detail components showing master data + nested DataGrid for related records

**Example Use Case**:
- **Customer Detail Page**: Shows customer info + all their orders in a DataGrid
- **Order Detail Page**: Shows order info + all order items in a DataGrid

---

## 2. Database Schema Analysis

### 2.1 FK Relationship Detection

The system will analyze tables and identify Foreign Key relationships:

```csharp
// Example: Order table
Table: Order
Columns:
  - ID (PK)
  - CustomerID (FK → Customer.ID)
  - OrderDate
  - TotalAmount

// Identifies relationship:
Relationship {
  ParentTable: "Customer",
  ParentColumn: "ID",
  ChildTable: "Order",
  ChildColumn: "CustomerID"
}
```

### 2.2 Relationship Types

**1-to-Many** (Focus of this spec):
- Customer → Orders
- Order → OrderItems
- Category → Products

**Future**: Many-to-Many via junction tables

---

## 3. SQL Layer: Stored Procedure Generation

### 3.1 New SP Template: SpGetRelatedTemplate.cs

Create a new template to generate stored procedures for fetching related records:

**Location**: `src/TargCC.Core.Generators/Sql/Templates/SpGetRelatedTemplate.cs`

**Example Output**:

```sql
-- =========================================
-- SP_GetCustomerOrders
-- Fetches all orders for a given customer
-- =========================================
CREATE OR ALTER PROCEDURE [dbo].[SP_GetCustomerOrders]
    @CustomerID INT,
    @Skip INT = NULL,
    @Take INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TotalCount INT;

    -- Get total count
    SELECT @TotalCount = COUNT(*)
    FROM [Order]
    WHERE [CustomerID] = @CustomerID;

    -- Return paginated results
    SELECT
        o.[ID],
        o.[CustomerID],
        o.[OrderDate],
        o.[TotalAmount],
        o.[lkp_Status] AS StatusCode,
        s.[StatusText],
        o.[AddedOn],
        o.[ChangedOn]
    FROM [Order] o
    LEFT JOIN [LookupStatus] s ON o.[lkp_Status] = s.[Code]
    WHERE o.[CustomerID] = @CustomerID
    ORDER BY o.[OrderDate] DESC
    OFFSET COALESCE(@Skip, 0) ROWS
    FETCH NEXT COALESCE(@Take, 1000) ROWS ONLY;

    -- Return total count
    SELECT @TotalCount AS TotalCount;
END
GO
```

### 3.2 Template Implementation

```csharp
public static class SpGetRelatedTemplate
{
    public static Task<string> GenerateAsync(
        Table parentTable,
        Table childTable,
        string fkColumnName)
    {
        var sb = new StringBuilder();

        // Procedure name: SP_Get{ParentName}{ChildrenName}
        var procName = $"SP_Get{parentTable.Name}{Pluralize(childTable.Name)}";

        sb.AppendLine($"CREATE OR ALTER PROCEDURE [dbo].[{procName}]");
        sb.AppendLine($"    @{parentTable.Name}ID {MapSqlType(parentTable.PrimaryKey)},");
        sb.AppendLine("    @Skip INT = NULL,");
        sb.AppendLine("    @Take INT = NULL");
        sb.AppendLine("AS");
        sb.AppendLine("BEGIN");
        sb.AppendLine("    SET NOCOUNT ON;");
        sb.AppendLine();

        // Generate SELECT with all columns
        GenerateSelectStatement(sb, childTable, fkColumnName);

        sb.AppendLine("END");
        return Task.FromResult(sb.ToString());
    }
}
```

### 3.3 Integration into SqlGenerator

Modify `SqlGenerator.cs` to generate related data SPs:

```csharp
public async Task<string> GenerateAsync(DatabaseSchema schema)
{
    var sb = new StringBuilder();

    // ... existing table SPs ...

    // Generate FK relationship SPs
    var relationships = AnalyzeRelationships(schema);
    foreach (var rel in relationships)
    {
        var relatedSp = await SpGetRelatedTemplate.GenerateAsync(
            rel.ParentTable,
            rel.ChildTable,
            rel.ForeignKeyColumn);
        sb.AppendLine(relatedSp);
        sb.AppendLine("GO");
    }

    return sb.ToString();
}
```

---

## 4. Repository Layer: GetRelated Methods

### 4.1 Repository Interface Extension

Modify `RepositoryInterfaceGenerator.cs` to add GetRelated methods:

```csharp
// ICustomerRepository.cs
public interface ICustomerRepository : IRepository<Customer, int>
{
    // ... existing methods ...

    // Generated for FK relationships:
    Task<IEnumerable<Order>> GetOrdersAsync(
        int customerId,
        int? skip = null,
        int? take = null,
        CancellationToken cancellationToken = default);
}
```

### 4.2 Repository Implementation

Modify `RepositoryGenerator.cs` to implement GetRelated methods:

```csharp
// CustomerRepository.cs
public async Task<IEnumerable<Order>> GetOrdersAsync(
    int customerId,
    int? skip = null,
    int? take = null,
    CancellationToken cancellationToken = default)
{
    _logger.LogDebug("Fetching orders for customer ID: {CustomerId}", customerId);

    try
    {
        var parameters = new DynamicParameters();
        parameters.Add("@CustomerID", customerId);
        parameters.Add("@Skip", skip);
        parameters.Add("@Take", take);

        var result = await _connection.QueryAsync<Order>(
            "SP_GetCustomerOrders",
            parameters,
            commandType: CommandType.StoredProcedure);

        _logger.LogInformation(
            "Retrieved {Count} orders for customer ID: {CustomerId}",
            result.Count(),
            customerId);

        return result;
    }
    catch (Exception ex)
    {
        _logger.LogError(
            ex,
            "Error fetching orders for customer ID: {CustomerId}",
            customerId);
        throw;
    }
}
```

---

## 5. API Layer: Controller Endpoints

### 5.1 API Controller Extension

Modify `ApiControllerGenerator.cs` to add endpoints for related data:

```csharp
// CustomersController.cs
[HttpGet("{id}/orders")]
[ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders(
    int id,
    [FromQuery] int? skip = null,
    [FromQuery] int? take = null,
    CancellationToken cancellationToken = default)
{
    // Check if customer exists
    if (!await _repository.ExistsAsync(id, cancellationToken))
    {
        return NotFound($"Customer with ID {id} not found.");
    }

    var orders = await _repository.GetOrdersAsync(id, skip, take, cancellationToken);
    var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(orders);

    return Ok(orderDtos);
}
```

### 5.2 Routing Pattern

All related data endpoints follow the pattern:
```
GET /api/customers/{id}/orders
GET /api/orders/{id}/items
GET /api/categories/{id}/products
```

---

## 6. TypeScript API Client

### 6.1 React API Generator Extension

Modify `ReactApiGenerator.cs` to generate API methods for related data:

```typescript
// customersApi.ts
export const customersApi = {
  // ... existing methods ...

  getOrders: (id: number, skip?: number, take?: number): Promise<Order[]> =>
    apiClient.get<Order[]>(`/api/customers/${id}/orders`, {
      params: { skip, take }
    }).then(res => res.data),
};
```

---

## 7. React Hooks

### 7.1 Hook Generator Extension

Modify `ReactHookGenerator.cs` to generate hooks for related data:

```typescript
// useCustomer.ts
export function useCustomerOrders(
  customerId: number | null,
  skip?: number,
  take?: number
) {
  return useQuery<Order[], Error>({
    queryKey: ['customer-orders', customerId, skip, take],
    queryFn: () => {
      if (!customerId) throw new Error('Customer ID is required');
      return customersApi.getOrders(customerId, skip, take);
    },
    enabled: !!customerId,
  });
}
```

---

## 8. React Detail Component

### 8.1 New Generator: ReactDetailComponentGenerator.cs

**Location**: `src/TargCC.Core.Generators/UI/Components/ReactDetailComponentGenerator.cs`

**Output Example**: `CustomerDetail.tsx`

```typescript
// CustomerDetail.tsx
import React from 'react';
import { useParams } from 'react-router-dom';
import { Box, Card, CardContent, Typography, CircularProgress, Alert } from '@mui/material';
import { DataGrid, GridColDef } from '@mui/x-data-grid';
import { useCustomer, useCustomerOrders } from '../../hooks/useCustomer';

export const CustomerDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const customerId = id ? Number(id) : null;

  const { data: customer, isPending: isLoadingCustomer, error: customerError } = useCustomer(customerId);
  const { data: orders, isPending: isLoadingOrders, error: ordersError } = useCustomerOrders(customerId);

  if (isLoadingCustomer) {
    return <CircularProgress />;
  }

  if (customerError) {
    return <Alert severity="error">Error loading customer: {customerError.message}</Alert>;
  }

  if (!customer) {
    return <Alert severity="warning">Customer not found</Alert>;
  }

  const orderColumns: GridColDef<Order>[] = [
    { field: 'id', headerName: 'Order ID', width: 100 },
    {
      field: 'orderDate',
      headerName: 'Order Date',
      width: 150,
      valueFormatter: (value) => new Date(value).toLocaleDateString()
    },
    { field: 'totalAmount', headerName: 'Total', width: 120 },
    {
      field: 'statusText',
      headerName: 'Status',
      width: 120,
      valueGetter: (value, row) => row.statusText || row.statusCode
    },
  ];

  return (
    <Box sx={{ p: 3 }}>
      {/* Master: Customer Info */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h4" gutterBottom>
            {customer.name}
          </Typography>
          <Typography variant="body1">Email: {customer.email}</Typography>
          <Typography variant="body1">Phone: {customer.phone}</Typography>
        </CardContent>
      </Card>

      {/* Detail: Orders DataGrid */}
      <Card>
        <CardContent>
          <Typography variant="h5" gutterBottom>
            Orders
          </Typography>

          <Box sx={{ height: 400, width: '100%' }}>
            <DataGrid
              rows={orders || []}
              columns={orderColumns}
              loading={isLoadingOrders}
              pageSizeOptions={[5, 10, 25]}
              initialState={{
                pagination: { paginationModel: { pageSize: 10 } },
              }}
            />
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
};
```

### 8.2 Routing Integration

Modify `App.tsx` generation to include detail routes:

```typescript
<Routes>
  <Route path="/customers" element={<CustomerList />} />
  <Route path="/customers/:id" element={<CustomerDetail />} />
  <Route path="/customers/:id/edit" element={<CustomerForm />} />
</Routes>
```

---

## 9. Integration Summary

### 9.1 Complete Flow

1. **Database Analysis** → Detect FK: `Order.CustomerID → Customer.ID`
2. **SQL SP Generation** → Create `SP_GetCustomerOrders`
3. **Repository** → Add `ICustomerRepository.GetOrdersAsync()` + implementation
4. **API** → Add `GET /api/customers/{id}/orders` endpoint
5. **TypeScript API** → Generate `customersApi.getOrders()`
6. **React Hook** → Generate `useCustomerOrders()`
7. **React Component** → Generate `CustomerDetail.tsx`
8. **Routing** → Add `/customers/:id` route

### 9.2 Code Generation Entry Point

Modify `ProjectGenerationService.cs`:

```csharp
public async Task GenerateFullStackProject(DatabaseSchema schema, string outputPath)
{
    // ... existing generation ...

    // NEW: Analyze relationships
    var relationships = RelationshipAnalyzer.Analyze(schema);

    // Generate master-detail artifacts
    foreach (var rel in relationships.Where(r => r.Type == RelationshipType.OneToMany))
    {
        await GenerateMasterDetailArtifacts(rel, outputPath);
    }
}

private async Task GenerateMasterDetailArtifacts(Relationship rel, string outputPath)
{
    // SQL SPs
    var spSql = await SpGetRelatedTemplate.GenerateAsync(rel.ParentTable, rel.ChildTable, rel.ForeignKeyColumn);
    await WriteFile($"{outputPath}/sql/SP_Get{rel.ParentTable.Name}{Pluralize(rel.ChildTable.Name)}.sql", spSql);

    // Repository methods (already done in RepositoryGenerator)

    // API endpoints (already done in ApiControllerGenerator)

    // TypeScript API (already done in ReactApiGenerator)

    // React hooks (already done in ReactHookGenerator)

    // React Detail component
    var detailComponent = await ReactDetailComponentGenerator.GenerateAsync(rel.ParentTable, schema);
    await WriteFile($"{outputPath}/client/src/components/{rel.ParentTable.Name}/{rel.ParentTable.Name}Detail.tsx", detailComponent);
}
```

---

## 10. Implementation Phases

### Phase 1: SQL SP Templates (Week 1, Days 1-2)
- Create `SpGetRelatedTemplate.cs`
- Implement `AnalyzeRelationships()` in `SqlGenerator`
- Generate SQL SPs for all FK relationships
- **Output**: Working SQL SPs in generated code

### Phase 2: Repository Layer (Week 1, Days 3-4)
- Extend `RepositoryInterfaceGenerator` to add GetRelated methods
- Extend `RepositoryGenerator` to implement GetRelated methods
- **Output**: Repository classes with working GetRelated methods

### Phase 3: API Layer (Week 1, Day 5)
- Extend `ApiControllerGenerator` to add related data endpoints
- Test endpoints with Swagger
- **Output**: Working API endpoints `/api/customers/{id}/orders`

### Phase 4: React Hooks (Week 2, Days 1-2)
- Extend `ReactHookGenerator` to generate hooks for related data
- Extend `ReactApiGenerator` to generate API methods
- **Output**: Working `useCustomerOrders()` hook

### Phase 5: React Detail Components (Week 2, Days 3-5)
- Create `ReactDetailComponentGenerator.cs`
- Generate master-detail UI components
- Add routing for detail pages
- **Output**: Working Detail pages showing master + related data

---

## 11. Example End-to-End Flow

### Given Schema:
```sql
CREATE TABLE Customer (
    ID INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100),
    Email NVARCHAR(100)
);

CREATE TABLE [Order] (
    ID INT PRIMARY KEY IDENTITY,
    CustomerID INT FOREIGN KEY REFERENCES Customer(ID),
    OrderDate DATETIME,
    TotalAmount DECIMAL(18,2)
);
```

### Generated Artifacts:

**1. SQL**: `SP_GetCustomerOrders`
```sql
CREATE OR ALTER PROCEDURE SP_GetCustomerOrders
    @CustomerID INT, @Skip INT = NULL, @Take INT = NULL
AS BEGIN
    SELECT * FROM [Order] WHERE CustomerID = @CustomerID
    ORDER BY OrderDate DESC
    OFFSET COALESCE(@Skip, 0) ROWS FETCH NEXT COALESCE(@Take, 1000) ROWS ONLY;
END
```

**2. Repository Interface**:
```csharp
public interface ICustomerRepository : IRepository<Customer, int>
{
    Task<IEnumerable<Order>> GetOrdersAsync(int customerId, int? skip, int? take, CancellationToken ct);
}
```

**3. Repository Implementation**:
```csharp
public async Task<IEnumerable<Order>> GetOrdersAsync(int customerId, int? skip, int? take, CancellationToken ct)
{
    return await _connection.QueryAsync<Order>("SP_GetCustomerOrders", new { CustomerID = customerId, Skip = skip, Take = take }, commandType: CommandType.StoredProcedure);
}
```

**4. API Endpoint**:
```csharp
[HttpGet("{id}/orders")]
public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders(int id, int? skip, int? take, CancellationToken ct)
{
    var orders = await _repository.GetOrdersAsync(id, skip, take, ct);
    return Ok(_mapper.Map<IEnumerable<OrderDto>>(orders));
}
```

**5. TypeScript API**:
```typescript
getOrders: (id: number, skip?: number, take?: number) => apiClient.get<Order[]>(`/api/customers/${id}/orders`, { params: { skip, take } })
```

**6. React Hook**:
```typescript
export function useCustomerOrders(customerId: number | null, skip?: number, take?: number) {
    return useQuery({ queryKey: ['customer-orders', customerId, skip, take], queryFn: () => customersApi.getOrders(customerId!, skip, take), enabled: !!customerId });
}
```

**7. React Detail Component**:
```tsx
export const CustomerDetail: React.FC = () => {
    const { id } = useParams();
    const { data: customer } = useCustomer(Number(id));
    const { data: orders } = useCustomerOrders(Number(id));
    return <Box><CustomerInfo data={customer} /><OrdersDataGrid rows={orders} /></Box>;
};
```

---

## 12. Configuration

### 12.1 Opt-In/Opt-Out

Add configuration to `targcc.config.json`:

```json
{
  "masterDetailViews": {
    "enabled": true,
    "generateSqlSPs": true,
    "generateRepositoryMethods": true,
    "generateApiEndpoints": true,
    "generateReactComponents": true,
    "maxNestingLevel": 2  // Customer → Orders → OrderItems (2 levels)
  }
}
```

### 12.2 Exclude Specific Relationships

```json
{
  "masterDetailViews": {
    "excludeRelationships": [
      { "parent": "User", "child": "AuditLog" },  // Don't generate User → AuditLog detail
      { "parent": "Customer", "child": "InternalNotes" }
    ]
  }
}
```

---

## 13. Testing Strategy

### 13.1 Unit Tests

- **SpGetRelatedTemplateTests**: Verify SQL SP generation
- **RepositoryGeneratorTests**: Verify GetRelated method generation
- **ReactDetailComponentGeneratorTests**: Verify component generation

### 13.2 Integration Tests

- **End-to-End Flow**: Create test schema → Generate code → Compile → Run
- **API Tests**: Call `/api/customers/{id}/orders` and verify response
- **UI Tests**: Load Detail page and verify DataGrid renders

---

## 14. Performance Considerations

### 14.1 Pagination

All GetRelated methods support `skip` and `take` parameters for pagination:
```csharp
Task<IEnumerable<Order>> GetOrdersAsync(int customerId, int? skip = null, int? take = null);
```

### 14.2 Lazy Loading

Detail data (e.g., orders) is only fetched when the Detail page is loaded, not on List pages.

### 14.3 Caching

React Query automatically caches related data:
```typescript
queryKey: ['customer-orders', customerId, skip, take]  // Cached per customer + pagination
```

---

## 15. Future Enhancements

### 15.1 Many-to-Many Relationships

Support junction tables:
```
Student ← StudentCourse → Course
```

Generate:
```csharp
Task<IEnumerable<Course>> GetCoursesAsync(int studentId);
Task<IEnumerable<Student>> GetStudentsAsync(int courseId);
```

### 15.2 Nested Detail Views

Support multi-level nesting:
```
Customer Detail
  ├── Orders (DataGrid)
      └── OrderItems (Expandable row / nested DataGrid)
```

### 15.3 Inline Editing

Allow editing related records directly in the Detail DataGrid without navigating away.

### 15.4 Export Related Data

Add "Export Orders to Excel" button on Customer Detail page.

---

## Summary

This specification defines a complete implementation of Master-Detail Views in TargCC, covering:
- ✅ SQL SP generation for related data
- ✅ Repository layer extensions
- ✅ API endpoints for related data
- ✅ TypeScript API clients
- ✅ React hooks for data fetching
- ✅ React Detail components with nested DataGrids
- ✅ Routing and navigation
- ✅ Configuration and opt-out
- ✅ Testing strategy
- ✅ Performance considerations

**Next Steps**: Begin implementation with Phase 1 (SQL SP Templates).
