namespace TargCC.Core.Interfaces.Models;

/// <summary>
/// Represents a database index that determines which query methods are generated.
/// </summary>
/// <remarks>
/// <para>
/// Indexes are one of the most important factors in code generation. They determine which
/// query methods (GetByXXX, FillByXXX) are created for each table.
/// </para>
/// <para>
/// <strong>Index Type → Generated Method Pattern:</strong>
/// </para>
/// <list type="table">
/// <listheader><term>Index Type</term><description>Generated Method</description></listheader>
/// <item><term>Primary Key (Unique, Clustered)</term><description>GetByID(primaryKeyType id)</description></item>
/// <item><term>Unique Index (single column)</term><description>GetByColumnName(type value)</description></item>
/// <item><term>Unique Index (composite)</term><description>GetByCol1AndCol2(type1 val1, type2 val2)</description></item>
/// <item><term>Non-Unique Index (single)</term><description>FillByColumnName(type value)</description></item>
/// <item><term>Non-Unique Index (composite)</term><description>FillByCol1AndCol2(type1 val1, type2 val2)</description></item>
/// </list>
/// <para>
/// <strong>Index Types:</strong>
/// </para>
/// <list type="bullet">
/// <item>Clustered - Physical order of data, one per table, typically the primary key</item>
/// <item>Non-Clustered - Separate structure with pointer to data, multiple allowed per table</item>
/// <item>Unique - Ensures no duplicate values, generates GetBy methods</item>
/// <item>Non-Unique - Allows duplicates, generates FillBy methods (returns collections)</item>
/// </list>
/// </remarks>
/// <example>
/// <para><strong>Example: Indexes and their generated methods</strong></para>
/// <code>
/// var indexes = new List&lt;Index&gt;
/// {
///     // Primary key → GetByID(int id)
///     new()
///     {
///         Name = "PK_Customer",
///         IsUnique = true,
///         IsClustered = true,
///         IsPrimaryKey = true,
///         ColumnNames = new() { "ID" }
///     },
///
///     // Unique email → GetByEmail(string email)
///     new()
///     {
///         Name = "IX_Customer_Email",
///         IsUnique = true,
///         IsClustered = false,
///         ColumnNames = new() { "Email" }
///     },
///
///     // Non-unique country → FillByCountry(string country)
///     new()
///     {
///         Name = "IX_Customer_Country",
///         IsUnique = false,
///         ColumnNames = new() { "Country" }
///     },
///
///     // Composite unique → GetByLastNameAndFirstName(string lastName, string firstName)
///     new()
///     {
///         Name = "IX_Customer_Name",
///         IsUnique = true,
///         ColumnNames = new() { "LastName", "FirstName" }
///     }
/// };
///
/// // Result: clsCustomer class with 4 query methods
/// </code>
/// </example>
public class Index
{
    /// <summary>
    /// Gets or sets the index name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this is a unique index.
    /// </summary>
    public bool IsUnique { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a clustered index.
    /// </summary>
    public bool IsClustered { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a primary key.
    /// </summary>
    public bool IsPrimaryKey { get; set; }

    /// <summary>
    /// Gets or sets the list of column names in the index.
    /// </summary>
    public List<string> ColumnNames { get; set; } = new ();

    /// <summary>
    /// Gets or sets the table name.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the index type description (CLUSTERED, NONCLUSTERED, etc.).
    /// </summary>
    public string TypeDescription { get; set; } = string.Empty;
}
