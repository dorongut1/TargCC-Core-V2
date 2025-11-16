namespace TargCC.Core.Interfaces.Models;

/// <summary>
/// Represents a database table with all its metadata, columns, indexes, and relationships.
/// </summary>
/// <remarks>
/// <para>
/// The Table class is a central model that contains all information about a database table
/// required for code generation, including columns, indexes, foreign key relationships, and
/// TargCC-specific metadata from extended properties.
/// </para>
/// <para>
/// <strong>Key Components:</strong>
/// </para>
/// <list type="bullet">
/// <item><term>Columns</term><description>All table columns with their types, prefixes, and behavior</description></item>
/// <item><term>Indexes</term><description>Unique and non-unique indexes that determine generated query methods</description></item>
/// <item><term>Relationships</term><description>Foreign key relationships to other tables</description></item>
/// <item><term>Primary Key</term><description>Primary key column(s) that determine GetByID method</description></item>
/// <item><term>Extended Properties</term><description>TargCC metadata (ccAuditLevel, ccUICreate*, etc.)</description></item>
/// </list>
/// <para>
/// <strong>Code Generation Impact:</strong>
/// </para>
/// <list type="bullet">
/// <item>Table name → Class name (Customer table → clsCustomer class)</item>
/// <item>Indexes → GetByXXX and FillByXXX methods</item>
/// <item>Relationships → Parent/Child navigation properties</item>
/// <item>Primary Key → GetByID method signature</item>
/// </list>
/// </remarks>
/// <example>
/// <para><strong>Example 1: Basic table with columns and indexes</strong></para>
/// <code>
/// var customerTable = new Table
/// {
///     Name = "Customer",
///     SchemaName = "dbo",
///     PrimaryKeyColumns = new List&lt;string&gt; { "ID" },
///     Columns = new List&lt;Column&gt;
///     {
///         new() { Name = "ID", DataType = "int", IsPrimaryKey = true, IsIdentity = true },
///         new() { Name = "Email", DataType = "nvarchar", MaxLength = 100 },
///         new() { Name = "FullName", DataType = "nvarchar", MaxLength = 200 }
///     },
///     Indexes = new List&lt;Index&gt;
///     {
///         new() { Name = "PK_Customer", IsUnique = true, IsPrimaryKey = true },
///         new() { Name = "IX_Customer_Email", IsUnique = true, Columns = new() { "Email" } }
///     }
/// };
///
/// Console.WriteLine(customerTable.FullName);  // Output: dbo.Customer
/// // Generates: clsCustomer class with GetByID(int id) and GetByEmail(string email) methods
/// </code>
/// </example>
/// <example>
/// <para><strong>Example 2: Table with foreign key relationships</strong></para>
/// <code>
/// var orderTable = new Table
/// {
///     Name = "Order",
///     SchemaName = "dbo",
///     PrimaryKeyColumns = new List&lt;string&gt; { "ID" },
///     Columns = new List&lt;Column&gt;
///     {
///         new() { Name = "ID", IsPrimaryKey = true },
///         new() { Name = "CustomerID", IsForeignKey = true, ReferencedTable = "Customer" },
///         new() { Name = "OrderDate", DataType = "datetime" }
///     },
///     Relationships = new List&lt;Relationship&gt;
///     {
///         new()
///         {
///             ParentTable = "Customer",
///             ParentColumn = "ID",
///             ChildTable = "Order",
///             ChildColumn = "CustomerID",
///             Type = RelationshipType.OneToMany
///         }
///     }
/// };
///
/// // Generates:
/// // - clsOrder.LoadCustomer() method (parent)
/// // - clsCustomer.FillOrders() method (children)
/// </code>
/// </example>
/// <example>
/// <para><strong>Example 3: System table with extended properties</strong></para>
/// <code>
/// var userTable = new Table
/// {
///     Name = "c_User",
///     SchemaName = "dbo",
///     IsSystemTable = true,
///     ExtendedProperties = new Dictionary&lt;string, string&gt;
///     {
///         { "ccAuditLevel", "2" },  // Full audit with triggers
///         { "ccUICreateMenu", "1" },  // Create menu entry
///         { "ccUICreateEntity", "1" },  // Create entity form
///         { "ccUICreateCollection", "1" }  // Create collection grid
///     }
/// };
///
/// // System table (c_ prefix) → used for TargCC's internal operations
/// // UI properties → controls what gets generated in WinForms
/// </code>
/// </example>
public class Table
{
    /// <summary>
    /// Gets or sets the table name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema name.
    /// </summary>
    public string SchemaName { get; set; } = "dbo";

    /// <summary>
    /// Gets or sets the collection of columns that define the table structure.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each column contains type information, prefixes, extended properties, and determines
    /// properties in the generated .NET class.
    /// </para>
    /// <para>
    /// <strong>Code Generation Impact:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>Column names → Property names</item>
    /// <item>Column prefixes (eno, ent, etc.) → Special property behavior</item>
    /// <item>Foreign key columns → Parent navigation properties</item>
    /// <item>Calculated columns → Read-only properties</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// table.Columns = new List&lt;Column&gt;
    /// {
    ///     new() { Name = "ID", DataType = "int", IsPrimaryKey = true },
    ///     new() { Name = "Email", DataType = "nvarchar", MaxLength = 100 },
    ///     new() { Name = "eno_Password", DataType = "varchar", MaxLength = 64, Prefix = ColumnPrefix.OneWayEncryption },
    ///     new() { Name = "clc_FullName", IsComputed = true, Prefix = ColumnPrefix.Calculated }
    /// };
    ///
    /// // Generates properties:
    /// // public int ID { get; set; }
    /// // public string Email { get; set; }
    /// // public string PasswordHashed { get; set; }  // SHA256 hashed
    /// // public string FullName { get; }  // Read-only, computed by SQL
    /// </code>
    /// </example>
    public List<Column> Columns { get; set; } = new ();

    /// <summary>
    /// Gets or sets the collection of indexes that determine query methods in generated code.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Indexes are critical for code generation as they determine which query methods are created.
    /// Each index generates specific methods based on its type and columns.
    /// </para>
    /// <para>
    /// <strong>Index Type → Generated Methods:</strong>
    /// </para>
    /// <list type="table">
    /// <listheader><term>Index Type</term><description>Generated Method</description></listheader>
    /// <item><term>Primary Key</term><description>GetByID(primaryKeyType id)</description></item>
    /// <item><term>Unique Index</term><description>GetByXXX(type xxx)</description></item>
    /// <item><term>Non-Unique Index</term><description>FillByXXX(type xxx)</description></item>
    /// <item><term>Composite Unique</term><description>GetByXXXAndYYY(type xxx, type yyy)</description></item>
    /// <item><term>Composite Non-Unique</term><description>FillByXXXAndYYY(type xxx, type yyy)</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// table.Indexes = new List&lt;Index&gt;
    /// {
    ///     // Primary key → GetByID(int id)
    ///     new() { Name = "PK_Customer", IsUnique = true, IsPrimaryKey = true, Columns = new() { "ID" } },
    ///
    ///     // Unique index → GetByEmail(string email)
    ///     new() { Name = "IX_Customer_Email", IsUnique = true, Columns = new() { "Email" } },
    ///
    ///     // Non-unique index → FillByCountry(string country)
    ///     new() { Name = "IX_Customer_Country", IsUnique = false, Columns = new() { "Country" } },
    ///
    ///     // Composite unique → GetByLastNameAndFirstName(string lastName, string firstName)
    ///     new() { Name = "IX_Customer_Name", IsUnique = true, Columns = new() { "LastName", "FirstName" } }
    /// };
    /// </code>
    /// </example>
    public List<Index> Indexes { get; set; } = new ();

    /// <summary>
    /// Gets or sets the collection of foreign key relationships to other tables.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Relationships define parent-child connections between tables and generate navigation methods.
    /// Each relationship creates methods for loading related data.
    /// </para>
    /// <para>
    /// <strong>Relationship Type → Generated Methods:</strong>
    /// </para>
    /// <list type="table">
    /// <listheader><term>Relationship</term><description>Generated Methods</description></listheader>
    /// <item><term>Parent (1:N from parent)</term><description>Child.LoadParent() - loads parent object</description></item>
    /// <item><term>Children (1:N from child)</term><description>Parent.FillChildren() - fills collection of children</description></item>
    /// <item><term>One-to-One</term><description>LoadRelated() and LoadRelated() in both directions</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Order table with CustomerID foreign key
    /// orderTable.Relationships = new List&lt;Relationship&gt;
    /// {
    ///     new()
    ///     {
    ///         ParentTable = "Customer",
    ///         ParentColumn = "ID",
    ///         ChildTable = "Order",
    ///         ChildColumn = "CustomerID",
    ///         Type = RelationshipType.OneToMany
    ///     }
    /// };
    ///
    /// // Generates:
    /// // In clsOrder:
    /// //   public clsFault LoadCustomer()  // Loads parent customer
    /// //   public clsCustomer Customer { get; }  // Parent property
    /// //
    /// // In clsCustomer:
    /// //   public clsFault FillOrders()  // Fills collection of orders
    /// //   public colOrders Orders { get; }  // Children collection
    /// </code>
    /// </example>
    public List<Relationship> Relationships { get; set; } = new ();

    /// <summary>
    /// Gets or sets the primary key column name.
    /// </summary>
    public string? PrimaryKey { get; set; }

    /// <summary>
    /// Gets or sets the table description/comment.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a TargCC system table (c_ prefix).
    /// </summary>
    /// <remarks>
    /// <para>
    /// System tables are used by TargCC for its internal operations (users, roles, permissions, auditing, etc.).
    /// They are identified by the "c_" prefix and have special handling during code generation.
    /// </para>
    /// <para>
    /// <strong>System Table Examples:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>c_User - User accounts and authentication</item>
    /// <item>c_Role - User roles for permissions</item>
    /// <item>c_Process - List of all processes (functions) in the system</item>
    /// <item>c_Permission - Role permissions for each process</item>
    /// <item>c_Audit - Audit trail of data changes</item>
    /// <item>c_Enumeration - Enum values for code generation</item>
    /// <item>c_Lookup - Dynamic lookup values</item>
    /// </list>
    /// <para>
    /// System tables are typically excluded from certain UI generation but included in core functionality.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var userTable = new Table
    /// {
    ///     Name = "c_User",
    ///     IsSystemTable = true  // Detected from "c_" prefix
    /// };
    ///
    /// var customerTable = new Table
    /// {
    ///     Name = "Customer",
    ///     IsSystemTable = false  // Regular application table
    /// };
    ///
    /// // System tables get special treatment:
    /// // - May be excluded from certain menu generations
    /// // - Required for TargCC functionality
    /// // - Usually have tighter security
    /// </code>
    /// </example>
    public bool IsSystemTable { get; set; }

    /// <summary>
    /// Gets or sets the number of rows in the table.
    /// </summary>
    public long RowCount { get; set; }

    /// <summary>
    /// Gets the fully qualified table name in SQL Server format (SchemaName.TableName).
    /// </summary>
    /// <remarks>
    /// <para>
    /// FullName combines the schema name and table name in the standard SQL Server format.
    /// This is used for generating SQL queries and stored procedures.
    /// </para>
    /// <para>
    /// <strong>Format:</strong> [SchemaName].[TableName].
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var table = new Table
    /// {
    ///     Name = "Customer",
    ///     SchemaName = "dbo"
    /// };
    ///
    /// Console.WriteLine(table.FullName);  // Output: dbo.Customer
    ///
    /// // Used in SQL generation:
    /// // SELECT * FROM dbo.Customer
    /// // CREATE PROCEDURE dbo.SP_GetCustomer
    /// </code>
    /// </example>
    public string FullName => $"{SchemaName}.{Name}";

    /// <summary>
    /// Gets or sets the SQL Server object ID.
    /// </summary>
    public int ObjectId { get; set; }

    /// <summary>
    /// Gets or sets the table creation date.
    /// </summary>
    public DateTime? CreateDate { get; set; }

    /// <summary>
    /// Gets or sets the table last modification date.
    /// </summary>
    public DateTime? ModifyDate { get; set; }

    /// <summary>
    /// Gets or sets the list of primary key column names.
    /// </summary>
    public List<string> PrimaryKeyColumns { get; set; } = new ();

    /// <summary>
    /// Gets or sets the extended properties that control TargCC code generation behavior.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Extended properties are SQL Server metadata stored on the table level that control
    /// various aspects of code generation, UI creation, and auditing behavior.
    /// </para>
    /// <para>
    /// <strong>Common Extended Properties:</strong>
    /// </para>
    /// <list type="table">
    /// <listheader><term>Property</term><description>Purpose</description></listheader>
    /// <item><term>ccAuditLevel</term><description>0=None, 1=Track changes only, 2=Full audit with triggers</description></item>
    /// <item><term>ccUICreateMenu</term><description>1=Create menu entry in WinForms, 0=Don't create</description></item>
    /// <item><term>ccUICreateEntity</term><description>1=Create entity form in WinForms, 0=Don't create</description></item>
    /// <item><term>ccUICreateCollection</term><description>1=Create collection grid in WinForms, 0=Don't create</description></item>
    /// <item><term>ccIsSingleRow</term><description>1=Table has only one row (no primary key), 0=Normal table</description></item>
    /// <item><term>ccUsedForIdentity</term><description>1=Table can identify users (Customer, Branch), 0=Normal</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var settingsTable = new Table
    /// {
    ///     Name = "SystemSettings",
    ///     ExtendedProperties = new Dictionary&lt;string, string&gt;
    ///     {
    ///         { "ccAuditLevel", "2" },  // Full audit
    ///         { "ccIsSingleRow", "1" },  // Only one row in table
    ///         { "ccUICreateMenu", "1" },
    ///         { "ccUICreateEntity", "1" },
    ///         { "ccUICreateCollection", "0" }  // No grid needed for single row
    ///     }
    /// };
    ///
    /// var customerTable = new Table
    /// {
    ///     Name = "Customer",
    ///     ExtendedProperties = new Dictionary&lt;strinשg, string&gt;
    ///     {
    ///         { "ccAuditLevel", "2" },
    ///         { "ccUsedForIdentity", "1" },  // Users can be linked to customers
    ///         { "ccUICreateMenu", "1" },
    ///         { "ccUICreateEntity", "1" },
    ///         { "ccUICreateCollection", "1" }
    ///     }
    /// };
    /// </code>
    /// </example>
    public Dictionary<string, string> ExtendedProperties { get; set; } = new ();
}
