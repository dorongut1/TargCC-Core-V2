namespace TargCC.Core.Interfaces.Models;

/// <summary>
/// Represents a database column with TargCC-specific metadata and behavior.
/// </summary>
/// <remarks>
/// <para>
/// The Column class represents a single column in a database table with all its properties,
/// metadata, and TargCC-specific behavior determined by prefixes and extended properties.
/// </para>
/// <para>
/// <strong>Key Concepts:</strong>
/// </para>
/// <list type="bullet">
/// <item><term>Prefix System</term><description>Column name prefixes (eno, ent, enm, lkp, etc.) determine special behavior in generated code.</description></item>
/// <item><term>Extended Properties</term><description>SQL Server extended properties (ccType, ccDNA) provide additional metadata without changing column names.</description></item>
/// <item><term>Type Mapping</term><description>Automatic mapping from SQL types (varchar, int) to .NET types (string, int) for code generation.</description></item>
/// <item><term>Read-Only Detection</term><description>Calculated, business logic, and aggregate fields are read-only on the client side.</description></item>
/// </list>
/// <para>
/// <strong>Common Prefix Behaviors:</strong>
/// </para>
/// <list type="table">
/// <listheader><term>Prefix</term><description>Behavior</description></listheader>
/// <item><term>eno</term><description>One-way encryption (SHA256) - passwords, sensitive data</description></item>
/// <item><term>ent</term><description>Two-way encryption (AES-256) - credit cards, SSN</description></item>
/// <item><term>clc_</term><description>Calculated field - read-only, not in Update methods</description></item>
/// <item><term>blg_</term><description>Business logic - server-side only, UpdateFriend method</description></item>
/// <item><term>agg_</term><description>Aggregate - counters, UpdateAggregates method</description></item>
/// <item><term>spt_</term><description>Separate update - different permissions, dedicated method</description></item>
/// </list>
/// </remarks>
/// <example>
/// <para><strong>Example 1: Basic column with prefix detection</strong></para>
/// <code>
/// var column = new Column
/// {
///     Name = "eno_Password",
///     DataType = "varchar",
///     DotNetType = "string",
///     MaxLength = 64,
///     IsNullable = false,
///     Prefix = ColumnPrefix.OneWayEncryption
/// };
///
/// // In generated code, this becomes:
/// // public string PasswordHashed { get; set; }
/// // Value is SHA256 hashed before saving to database
/// </code>
/// </example>
/// <example>
/// <para><strong>Example 2: Foreign key column with lookup</strong></para>
/// <code>
/// var column = new Column
/// {
///     Name = "lkp_Status",
///     DataType = "varchar",
///     DotNetType = "string",
///     MaxLength = 50,
///     Prefix = ColumnPrefix.Lookup,
///     IsForeignKey = true
/// };
///
/// // Generates:
/// // - ComboBox in UI with lookup values
/// // - Text stored as string, validated against c_Lookup table
/// </code>
/// </example>
/// <example>
/// <para><strong>Example 3: Calculated field (read-only)</strong></para>
/// <code>
/// var column = new Column
/// {
///     Name = "clc_TotalPrice",
///     DataType = "decimal",
///     DotNetType = "decimal",
///     Precision = 18,
///     Scale = 2,
///     IsComputed = true,
///     ComputedDefinition = "([Quantity] * [UnitPrice])",
///     Prefix = ColumnPrefix.Calculated,
///     IsReadOnly = true
/// };
///
///
/// // This field is:
/// // - Not included in Update/Insert methods
/// // - Read-only property in .NET class
/// // - Calculated by SQL Server
/// </code>
/// </example>
/// <example>
/// <para><strong>Example 4: Extended properties for behavior</strong></para>
/// <code>
/// var column = new Column
/// {
///     Name = "InternalNotes",
///     DataType = "varchar",
///     DotNetType = "string",
///     MaxLength = -1, // varchar(MAX)
///     ExtendedProperties = new Dictionary&lt;string, string&gt;
///     {
///         { "ccType", "blg" },  // Business logic field
///         { "ccDNA", "1" }      // Do Not Audit
///     },
///     IsReadOnly = true,
///     DoNotAudit = true
/// };
///
/// // Extended properties allow behavior changes without renaming columns
/// </code>
/// </example>
public class Column
{
    /// <summary>
    /// Gets or sets the column name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SQL data type.
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the .NET data type.
    /// </summary>
    public string DotNetType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the column is nullable.
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is the primary key.
    /// </summary>
    public bool IsPrimaryKey { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is an identity column.
    /// </summary>
    public bool IsIdentity { get; set; }

    /// <summary>
    /// Gets or sets the maximum length (for string types).
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets the precision (for numeric types).
    /// </summary>
    public int? Precision { get; set; }

    /// <summary>
    /// Gets or sets the scale (for numeric types).
    /// </summary>
    public int? Scale { get; set; }

    /// <summary>
    /// Gets or sets the default value.
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Gets or sets the column description/comment.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the column prefix that determines special behavior in generated code.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Prefix is detected automatically from the column name (e.g., "eno_Password" → OneWayEncryption).
    /// It can also be set via the ccType extended property without changing the column name.
    /// </para>
    /// <para>
    /// <strong>Impact on Code Generation:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>OneWayEncryption (eno): Property suffixed with "Hashed", SHA256 hashing before save</item>
    /// <item>TwoWayEncryption (ent): Encrypted in DB, decrypted when read, AES-256</item>
    /// <item>Calculated (clc_): Excluded from Update/Insert methods, read-only</item>
    /// <item>BusinessLogic (blg_): Server-side only, UpdateFriend method</item>
    /// <item>Aggregate (agg_): UpdateAggregates method with increment logic</item>
    /// <item>SeparateUpdate (spt_): Dedicated UpdateXXX method for different permissions</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Password column with one-way encryption
    /// var pwdColumn = new Column
    /// {
    ///     Name = "eno_Password",
    ///     Prefix = ColumnPrefix.OneWayEncryption,
    ///     IsEncrypted = true
    /// };
    /// // Generates: public string PasswordHashed { get; set; }
    ///
    /// // Status with lookup (no name change needed via ccType)
    /// var statusColumn = new Column
    /// {
    ///     Name = "Status",
    ///     Prefix = ColumnPrefix.Lookup,
    ///     ExtendedProperties = new() { { "ccType", "lkp" } }
    /// };
    /// // Generates ComboBox with lookup values from c_Lookup
    /// </code>
    /// </example>
    public ColumnPrefix Prefix { get; set; } = ColumnPrefix.None;

    /// <summary>
    /// Gets or sets extended properties from SQL Server (ccType, ccDNA, etc.).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Extended properties allow behavior changes without modifying column names.
    /// They are stored as SQL Server extended properties on the column.
    /// </para>
    /// <para>
    /// <strong>Common Extended Properties:</strong>
    /// </para>
    /// <list type="table">
    /// <listheader><term>Property</term><description>Usage</description></listheader>
    /// <item><term>ccType</term><description>Column behavior (blg, clc, spt, agg) - comma-separated combinations allowed</description></item>
    /// <item><term>ccDNA</term><description>Do Not Audit (value: 1) - excludes from audit trail</description></item>
    /// <item><term>ccUpdateXXXX</term><description>Partial update group - ordinal position in update method</description></item>
    /// <item><term>ccUsedForTableCleanup</term><description>Date field for automatic table cleanup (value: 1)</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Business logic field that's also not audited
    /// var column = new Column
    /// {
    ///     Name = "InternalScore",
    ///     ExtendedProperties = new()
    ///     {
    ///         { "ccType", "blg" },
    ///         { "ccDNA", "1" }
    ///     },
    ///     IsReadOnly = true,
    ///     DoNotAudit = true
    /// };
    ///
    /// // Combination: calculated + business logic
    /// var totalColumn = new Column
    /// {
    ///     Name = "TotalWithTax",
    ///     ExtendedProperties = new()
    ///     {
    ///         { "ccType", "clc,blg" }  // Both calculated AND business logic
    ///     }
    /// };
    ///
    /// // Partial update group
    /// var commentsColumn = new Column
    /// {
    ///     Name = "ManagerComments",
    ///     ExtendedProperties = new()
    ///     {
    ///         { "ccUpdateApproval", "1" }  // First field in UpdateApproval method
    ///     }
    /// };
    /// </code>
    /// </example>
    public Dictionary<string, string> ExtendedProperties { get; set; } = new ();

    /// <summary>
    /// Gets or sets a value indicating whether this is a foreign key.
    /// </summary>
    public bool IsForeignKey { get; set; }

    /// <summary>
    /// Gets or sets the referenced table name (if foreign key).
    /// </summary>
    public string? ReferencedTable { get; set; }

    /// <summary>
    /// Gets or sets the ordinal position of the column.
    /// </summary>
    public int OrdinalPosition { get; set; }

    /// <summary>
    /// Gets or sets the column ID.
    /// </summary>
    public int ColumnId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a computed column.
    /// </summary>
    public bool IsComputed { get; set; }

    /// <summary>
    /// Gets or sets the computed column definition.
    /// </summary>
    public string? ComputedDefinition { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this column uses encryption (one-way or two-way).
    /// </summary>
    /// <remarks>
    /// <para>
    /// True for columns with eno (one-way) or ent (two-way) prefixes.
    /// Determines special handling in data access and UI layers.
    /// </para>
    /// <para>
    /// <strong>Encryption Types:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>One-way (eno): SHA256 hash, cannot be decrypted - used for passwords</item>
    /// <item>Two-way (ent): AES-256, can be decrypted - used for sensitive data (SSN, credit cards)</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Password with one-way encryption
    /// var password = new Column
    /// {
    ///     Name = "eno_Password",
    ///     Prefix = ColumnPrefix.OneWayEncryption,
    ///     IsEncrypted = true,
    ///     DataType = "varchar",
    ///     MaxLength = 64  // SHA256 output length
    /// };
    /// // User types "MyPassword123"
    /// // Stored as: "5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8"
    ///
    /// // SSN with two-way encryption
    /// var ssn = new Column
    /// {
    ///     Name = "ent_SSN",
    ///     Prefix = ColumnPrefix.TwoWayEncryption,
    ///     IsEncrypted = true,
    ///     DataType = "varchar",
    ///     MaxLength = -1  // Encrypted data needs more space
    /// };
    /// // User types "123-45-6789"
    /// // Stored encrypted, can be decrypted when needed
    /// </code>
    /// </example>
    public bool IsEncrypted { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this column is read-only (excluded from Update/Insert).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Read-only columns are excluded from Update and Insert methods in generated code.
    /// They can only be modified server-side or are calculated by the database.
    /// </para>
    /// <para>
    /// <strong>Common Read-Only Column Types:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>clc_ (Calculated) - Computed by SQL Server (e.g., [Quantity] * [Price])</item>
    /// <item>blg_ (Business Logic) - Set by server-side code only via UpdateFriend</item>
    /// <item>agg_ (Aggregate) - Counters updated via UpdateAggregates with increment</item>
    /// <item>Identity columns - Auto-generated by database</item>
    /// <item>Timestamp/RowVersion - Concurrency control fields</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Calculated field (SQL computed column)
    /// var total = new Column
    /// {
    ///     Name = "clc_TotalAmount",
    ///     Prefix = ColumnPrefix.Calculated,
    ///     IsReadOnly = true,
    ///     IsComputed = true,
    ///     ComputedDefinition = "([Quantity] * [UnitPrice])"
    /// };
    /// // NOT in Update/Insert methods
    /// // Always calculated by SQL Server
    ///
    /// // Business logic field
    /// var score = new Column
    /// {
    ///     Name = "blg_CreditScore",
    ///     Prefix = ColumnPrefix.BusinessLogic,
    ///     IsReadOnly = true
    /// };
    /// // Only UpdateFriend method can modify
    /// // Client cannot change this value
    ///
    /// // Aggregate counter
    /// var orderCount = new Column
    /// {
    ///     Name = "agg_OrderCount",
    ///     Prefix = ColumnPrefix.Aggregate,
    ///     IsReadOnly = true,
    ///     DataType = "int"
    /// };
    /// // Updated via: UpdateAggregates(orderCount: +1)
    /// </code>
    /// </example>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to exclude this column from the audit trail.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When true, changes to this column are not recorded in the audit tables.
    /// Set via the ccDNA extended property (value: "1").
    /// </para>
    /// <para>
    /// <strong>Use Cases for DoNotAudit:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>Frequently changing fields with low audit value (e.g., LastViewedDate)</item>
    /// <item>Calculated fields that would clutter audit logs</item>
    /// <item>Internal system fields (e.g., sync timestamps)</item>
    /// <item>Large binary data (images, documents) - performance optimization</item>
    /// <item>Sensitive data where audit trail itself is a security risk</item>
    /// </list>
    /// <para>
    /// <strong>Warning:</strong> Use sparingly! Most fields should be audited for compliance.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Last viewed timestamp - changes too frequently to audit
    /// var lastViewed = new Column
    /// {
    ///     Name = "LastViewedDate",
    ///     DataType = "datetime",
    ///     DoNotAudit = true,
    ///     ExtendedProperties = new() { { "ccDNA", "1" } }
    /// };
    ///
    /// /// // Profile image - too large for audit table
    /// var avatar = new Column
    /// {
    ///     Name = "ProfileImage",
    ///     DataType = "varbinary",
    ///     MaxLength = -1,  // MAX
    ///     DoNotAudit = true,
    ///     ExtendedProperties = new() { { "ccDNA", "1" } }
    /// };
    ///
    /// // Internal sync field
    /// var syncedAt = new Column
    /// {
    ///     Name = "LastSyncTimestamp",
    ///     DataType = "datetime2",
    ///     DoNotAudit = true,
    ///     ExtendedProperties = new() { { "ccDNA", "1" } }
    /// };
    ///
    /// // Counter example: AUDIT MOST FIELDS!
    /// var salary = new Column
    /// {
    ///     Name = "Salary",
    ///     DataType = "decimal",
    ///     DoNotAudit = false  // MUST be audited for compliance!
    /// };
    /// </code>
    /// </example>
    public bool DoNotAudit { get; set; }
}
