namespace TargCC.Core.Interfaces.Models;

/// <summary>
/// Column prefix types that determine special behavior in TargCC code generation.
/// </summary>
/// <remarks>
/// <para>
/// Prefixes are detected from column names (e.g., "eno_Password") or set via ccType extended property.
/// Each prefix type generates different code patterns, methods, and UI controls.
/// </para>
/// <para>
/// <strong>Prefix Detection Examples:</strong>
/// </para>
/// <code>
/// // From column name:
/// "eno_Password" → OneWayEncryption
/// "clc_TotalAmount" → Calculated
/// "lkp_Status" → Lookup
/// /// // From extended property (no name change):
/// ExtendedProperties: { "ccType", "blg" } → BusinessLogic
/// ExtendedProperties: { "ccType", "clc,blg" } → Calculated + BusinessLogic
/// </code>
/// </remarks>
public enum ColumnPrefix
{
    /// <summary>
    /// No special prefix - standard column behavior.
    /// </summary>
    /// <remarks>
    /// Standard CRUD operations, no special handling.
    /// </remarks>
    None = 0,

    /// <summary>
    /// eno - One-way encryption using SHA256 hashing (cannot be decrypted).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Use Case:</strong> Passwords, authentication tokens, any data that should never be readable.
    /// </para>
    /// <para>
    /// <strong>Generated Code:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>Property name suffixed with "Hashed" (e.g., PasswordHashed)</item>
    /// <item>Client prefixes value with "[PleaseHash]" marker</item>
    /// <item>DBController hashes with SHA256 before saving</item>
    /// <item>UI shows password masked textbox</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <para><strong>SQL Definition:</strong></para>
    /// <code>
    /// CREATE TABLE [User] (
    ///     ID int IDENTITY PRIMARY KEY,
    ///     UserName nvarchar(100) NOT NULL,
    ///     eno_Password varchar(64) NOT NULL  -- SHA256 = 64 chars
    /// )
    /// </code>
    /// <para><strong>Generated Property:</strong></para>
    /// <code>
    /// public string PasswordHashed { get; set; }
    ///
    /// // Usage:
    /// user.PasswordHashed = "[PleaseHash]MyPassword123";
    /// // Saved as: "5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8"
    /// </code>
    /// </example>
    OneWayEncryption = 1,

    /// <summary>
    /// ent - Two-way encryption using AES-256 (can be decrypted).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Use Case:</strong> Credit cards, SSN, sensitive data that needs to be readable when authorized.
    /// </para>
    /// <para>
    /// <strong>Generated Code:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>Property stored as clear text in .NET object</item>
    /// <item>Encrypted before saving to database (AES-256)</item>
    /// <item>Decrypted when read from database</item>
    /// <item>UI shows standard textbox (clear text)</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <para><strong>SQL Definition:</strong></para>
    /// <code>
    /// CREATE TABLE [Customer] (
    ///     ID int IDENTITY PRIMARY KEY,
    ///     FullName nvarchar(200) NOT NULL,
    ///     ent_SSN varchar(MAX) NULL  -- Encrypted data needs more space
    /// )
    /// </code>
    /// <para><strong>Usage:</strong></para>
    /// <code>
    /// customer.SSN = "123-45-6789";  // Clear text in memory
    /// customer.Update();  // Encrypted in DB: "Ae8xK2p..." (AES-256)
    /// // When read back: automatically decrypted to "123-45-6789"
    /// </code>
    /// </example>
    TwoWayEncryption = 2,

    /// <summary>
    /// enm - Enumeration field with values from c_Enumeration system table.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Use Case:</strong> Fixed list of values you want to reference in code (e.g., Status, Priority).
    /// </para>
    /// <para>
    /// <strong>Generated Code:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>Enum type generated in code (e.g., enmStatuses)</item>
    /// <item>ComboBox in UI with enum values</item>
    /// <item>Localized text from c_Enumeration</item>
    /// <item>Type-safe code: if (order.Status == enmStatuses.Pending)</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <para><strong>SQL Definition:</strong></para>
    /// <code>
    /// CREATE TABLE [Order] (
    ///     ID int IDENTITY PRIMARY KEY,
    ///     enmStatus varchar(50) NOT NULL  -- or enmOrderStatus_Status
    /// )
    ///
    /// -- c_Enumeration table:
    /// -- Type='Status', Value='Pending', Text_en='Pending', Text_he='ממתין'
    /// -- Type='Status', Value='Approved', Text_en='Approved', Text_he='אושר'
    /// </code>
    /// <para><strong>Generated Code:</strong></para>
    /// <code>
    /// public enum enmStatuses { Pending, Approved, Rejected }
    /// public enmStatuses Status { get; set; }
    ///
    /// // Usage:
    /// if (order.Status == enmStatuses.Approved) { ... }
    /// </code>
    /// </example>
    Enumeration = 3,

    /// <summary>
    /// lkp - Lookup field with values from c_Lookup system table.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Use Case:</strong> Dynamic list of values that change at runtime (e.g., Countries, Departments).
    /// </para>
    /// <para>
    /// <strong>Difference from Enumeration:</strong> Lookups are dynamic (added via UI), enums are static (require code regeneration).
    /// </para>
    /// <para>
    /// <strong>Generated Code:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>Property stored as string</item>
    /// <item>ComboBox in UI populated from c_Lookup</item>
    /// <item>Localized text supported</item>
    /// <item>Validates against c_Lookup table</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <para><strong>SQL Definition:</strong></para>
    /// <code>
    /// CREATE TABLE [Employee] (
    ///     ID int IDENTITY PRIMARY KEY,
    ///     lkp_Department varchar(50) NULL
    /// )
    ///
    /// -- c_Lookup table:
    /// -- Type='Department', Code='IT', Text_en='IT', Text_he='מחשוב'
    /// -- Type='Department', Code='HR', Text_en='HR', Text_he='משאבי אנוש'
    /// </code>
    /// <para><strong>Usage:</strong></para>
    /// <code>
    /// employee.Department = "IT";  // String value
    /// // UI shows ComboBox with all departments from c_Lookup
    /// </code>
    /// </example>
    Lookup = 4,

    /// <summary>
    /// loc - Localization field stored in c_ObjectToTranslate system table.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Use Case:</strong> Multi-language content (product descriptions, help text).
    /// </para>
    /// <para>
    /// <strong>Generated Code:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>Two properties: Description (base) and DescriptionLocalized (current language)</item>
    /// <item>Translations stored in c_ObjectToTranslate</item>
    /// <item>Automatic language switching</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <para><strong>SQL Definition:</strong></para>
    /// <code>
    /// CREATE TABLE [Product] (
    ///     ID int IDENTITY PRIMARY KEY,
    ///     loc_Description nvarchar(MAX) NULL
    /// )
    /// </code>
    /// <para><strong>Generated Properties:</strong></para>
    /// <code>
    /// public string Description { get; set; }  // Base language
    /// public string DescriptionLocalized { get; }  // Current user language
    ///
    /// // Usage:
    /// product.Description = "Great product!";  // English
    /// // c_ObjectToTranslate: he → "מוצר מעולה!"
    /// // When user language = Hebrew:
    /// Console.WriteLine(product.DescriptionLocalized);  // "מוצר מעולה!"
    /// </code>
    /// </example>
    Localization = 5,

    /// <summary>
    /// clc_ - Calculated field computed by SQL Server (read-only, not in Update/Insert).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Use Case:</strong> Derived values, formulas, concatenations (e.g., FullName, TotalPrice).
    /// </para>
    /// <para>
    /// <strong>Generated Code:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>Read-only property</item>
    /// <item>Excluded from Update/Insert methods</item>
    /// <item>Always calculated by SQL Server</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <para><strong>SQL Definition:</strong></para>
    /// <code>
    /// CREATE TABLE [OrderLine] (
    ///     ID int IDENTITY PRIMARY KEY,
    ///     Quantity int NOT NULL,
    ///     UnitPrice decimal(18,2) NOT NULL,
    ///     clc_TotalPrice AS ([Quantity] * [UnitPrice])  -- Computed column
    /// )
    /// </code>
    /// <para><strong>Generated Property:</strong></para>
    /// <code>
    /// public decimal TotalPrice { get; }  // Read-only!
    ///
    /// // Usage:
    /// line.Quantity = 5;
    /// line.UnitPrice = 29.99m;
    /// line.Update();
    /// // line.TotalPrice automatically = 149.95 (calculated by SQL)
    /// </code>
    /// </example>
    Calculated = 6,

    /// <summary>
    /// blg_ - Business logic field set only by server-side code (via UpdateFriend).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Use Case:</strong> Server-calculated values, credit scores, risk levels - anything client shouldn't set.
    /// </para>
    /// <para>
    /// <strong>Generated Code:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>Read-only on client side</item>
    /// <item>Can be modified via UpdateFriend method (server-side only)</item>
    /// <item>Ensures business logic integrity</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <para><strong>SQL Definition:</strong></para>
    /// <code>
    /// CREATE TABLE [Customer] (
    ///     ID int IDENTITY PRIMARY KEY,
    ///     blg_CreditScore int NULL,
    ///     blg_RiskLevel varchar(20) NULL
    /// )
    /// </code>
    /// <para><strong>Usage (Server-side only):</strong></para>
    /// <code>
    /// // Client CANNOT do this:
    /// // customer.CreditScore = 800;  // Property is read-only
    ///
    /// // Server-side business logic:
    /// public static clsFault CalculateCreditScore(clsCustomer customer)
    /// {
    ///     var score = CalculateFromHistory(customer.ID);
    ///     customer.CreditScore = score;  // Set via friend property
    ///     customer.RiskLevel = score &gt; 700 ? "Low" : "High";
    ///     return customer.UpdateFriend();  // Special server-side update
    /// }
    /// </code>
    /// </example>
    BusinessLogic = 7,

    /// <summary>
    /// agg_ - Aggregate field (counters, sums) updated via UpdateAggregates with increment logic.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Use Case:</strong> Counters (OrderCount, TotalSpent), statistics that increment/decrement.
    /// </para>
    /// <para>
    /// <strong>Generated Code:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>Read-only property</item>
    /// <item>UpdateAggregates method with increment parameters</item>
    /// <item>Thread-safe increments in database</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <para><strong>SQL Definition:</strong></para>
    /// <code>
    /// CREATE TABLE [Customer] (
    ///     ID int IDENTITY PRIMARY KEY,
    ///     agg_OrderCount int NOT NULL DEFAULT 0,
    ///     agg_TotalSpent decimal(18,2) NOT NULL DEFAULT 0
    /// )
    /// </code>
    /// <para><strong>Generated Method:</strong></para>
    /// <code>
    /// // Generated: UpdateAggregates(int? orderCount = null, decimal? totalSpent = null)
    ///
    /// // Usage when new order placed:
    /// customer.UpdateAggregates(orderCount: +1, totalSpent: +129.99m);
    /// // SQL: UPDATE Customer SET OrderCount += 1, TotalSpent += 129.99
    ///
    /// // When order cancelled:
    /// customer.UpdateAggregates(orderCount: -1, totalSpent: -129.99m);
    /// </code>
    /// </example>
    Aggregate = 8,

    /// <summary>
    /// spt_ - Separately updated field with dedicated method (for different permissions).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Use Case:</strong> Fields needing different permissions than main object (e.g., Comments, ApprovalStatus).
    /// </para>
    /// <para>
    /// <strong>Generated Code:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>Excluded from main Update method</item>
    /// <item>Dedicated UpdateXXX method with separate permissions</item>
    /// <item>"Change" button in UI</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <para><strong>SQL Definition:</strong></para>
    /// <code>
    /// CREATE TABLE [SecretDocument] (
    ///     ID int IDENTITY PRIMARY KEY,
    ///     Title nvarchar(200) NOT NULL,  -- Only admin can update
    ///     spt_PublicComments nvarchar(MAX) NULL  -- Anyone can update
    /// )
    /// </code>
    /// <para><strong>Generated Methods:</strong></para>
    /// <code>
    /// // Main update (requires Admin permission):
    /// public clsFault Update();  // Updates Title only
    ///
    /// // Separate update (requires User permission):
    /// public clsFault UpdatePublicComments(string newComments);  // Updates Comments only
    ///
    /// // Permissions:
    /// // Process: tbl_SecretDocument.Update → Role: Admin
    /// // Process: tbl_SecretDocument.UpdatePublicComments → Role: User
    /// </code>
    /// </example>
    SeparateUpdate = 9,

    /// <summary>
    /// spl_ - Separate list field with NewLine delimited values (multiselect).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Use Case:</strong> Multi-select fields (applications, roles, tags).
    /// </para>
    /// <para>
    /// <strong>Generated Code:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>Property stores NewLine-delimited string</item>
    /// <item>UI shows multiline textbox</item>
    /// <item>Update popup shows ListBox with checkboxes</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <para><strong>SQL Definition:</strong></para>
    /// <code>
    /// CREATE TABLE [User] (
    ///     ID int IDENTITY PRIMARY KEY,
    ///     UserName nvarchar(100) NOT NULL,
    ///     spl_Applications nvarchar(MAX) NULL  -- Multi-select
    /// )
    /// </code>
    /// <para><strong>Usage:</strong></para>
    /// <code>
    /// // Stored as:
    /// user.Applications = "App1\r\nApp2\r\nApp3";
    ///
    /// // UI shows:
    /// // [✓] App1
    /// // [✓] App2
    /// // [✓] App3
    /// // [ ] App4
    /// </code>
    /// </example>
    SeparateList = 10,

    /// <summary>
    /// upl_ - Upload field for storing files on server with encrypted names.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Use Case:</strong> Document uploads (contracts, images, attachments).
    /// </para>
    /// <para>
    /// <strong>Generated Code:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>Column stores encrypted filename (varchar(69))</item>
    /// <item>UI shows Upload/View/Delete buttons</item>
    /// <item>Files stored on server with encrypted names</item>
    /// <item>Deleted files moved to archive folder</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <para><strong>SQL Definition:</strong></para>
    /// <code>
    /// CREATE TABLE [Lease] (
    ///     ID int IDENTITY PRIMARY KEY,
    ///     upl_Contract varchar(69) NULL  -- Encrypted filename
    /// )
    /// </code>
    /// <para><strong>Usage:</strong></para>
    /// <code>
    /// // User uploads "Lease_2024.pdf"
    /// // Stored as: "Ae8xK2p9ZmN1Y3RlZF9maWxl" (encrypted)
    /// // File saved: C:\Uploads\Ae8xK2p9ZmN1Y3RlZF9maWxl
    ///
    /// // When deleted:
    /// // Moved to: C:\Uploads\Archive\Ae8xK2p9ZmN1Y3RlZF_20241114_103045
    /// </code>
    /// </example>
    Upload = 11,

    /// <summary>
    /// fui_ - Fake Unique Index for creating unique constraints that allow NULLs.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Problem:</strong> SQL Server unique indexes don't ignore NULLs (multiple NULLs not allowed).
    /// </para>
    /// <para>
    /// <strong>Solution:</strong> Computed column with CASE that converts NULL to unique negative value.
    /// </para>
    /// <para>
    /// <strong>Use Case:</strong> Optional unique fields (InvoiceNo for completed transactions only).
    /// </para>
    /// <para>
    /// <strong>Note:</strong> Filtered indexes (SQL 2008+) are a better modern solution.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para><strong>SQL Definition:</strong></para>
    /// <code>
    /// CREATE TABLE [Transaction] (
    ///     ID int IDENTITY PRIMARY KEY,
    ///     InvoiceNo varchar(50) NULL,  -- Only successful transactions
    ///     FUI_InvoiceNo AS (
    ///         CASE
    ///             WHEN InvoiceNo IS NULL OR InvoiceNo = ''
    ///             THEN 'FUI' + CONVERT(varchar(50), -ID)
    ///             ELSE InvoiceNo
    ///         END
    ///     )
    /// )
    /// CREATE UNIQUE INDEX IX_FUI_InvoiceNo ON Transaction(FUI_InvoiceNo);
    /// </code>
    /// <para><strong>Result:</strong></para>
    /// <code>
    /// // Successful transaction:
    /// InvoiceNo = "INV-001" → FUI_InvoiceNo = "INV-001" (unique)
    ///
    /// // Failed transactions (NULL invoices):
    /// ID=1, InvoiceNo=NULL → FUI_InvoiceNo = "FUI-1" (unique)
    /// ID=2, InvoiceNo=NULL → FUI_InvoiceNo = "FUI-2" (unique)
    ///
    /// // GetByInvoiceNo("INV-001") still works - returns from InvoiceNo column
    /// </code>
    /// </example>
    FakeUniqueIndex = 12
}

/// <summary>
/// Relationship types between tables.
/// </summary>
public enum RelationshipType
{
    /// <summary>
    /// One-to-Many (1:N) - most common type.
    /// </summary>
    OneToMany = 1,

    /// <summary>
    /// One-to-One (1:1) - FK with Unique Index.
    /// </summary>
    OneToOne = 2,

    /// <summary>
    /// Many-to-Many (N:M) - via junction table.
    /// </summary>
    ManyToMany = 3,

    /// <summary>
    /// Many-to-one (N:1) - via junction table.
    /// </summary>
    ManyToOne = 4
}
