using TargCC.Core.Interfaces.Models;

namespace TargCC.Core.Tests.TestHelpers;

/// <summary>
/// Builder pattern for creating Column objects in tests.
/// Makes test setup clean and readable.
/// </summary>
public class ColumnBuilder
{
    private string _name = "TestColumn";
    private string _dataType = "nvarchar";
    private string _dotNetType = "string";
    private bool _isNullable = true;
    private bool _isPrimaryKey = false;
    private bool _isIdentity = false;
    private int? _maxLength = null;
    private int? _precision = null;
    private int? _scale = null;
    private string? _defaultValue = null;
    private ColumnPrefix _prefix = ColumnPrefix.None;
    private bool _isEncrypted = false;
    private bool _isReadOnly = false;
    private Dictionary<string, string> _extendedProperties = new();

    /// <summary>
    /// Sets the column name.
    /// </summary>
    public ColumnBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the SQL data type.
    /// </summary>
    public ColumnBuilder WithDataType(string dataType)
    {
        _dataType = dataType;
        return this;
    }

    /// <summary>
    /// Makes the column an INT type.
    /// </summary>
    public ColumnBuilder AsInt()
    {
        _dataType = "int";
        _dotNetType = "int";
        return this;
    }

    /// <summary>
    /// Makes the column a BIGINT type.
    /// </summary>
    public ColumnBuilder AsBigInt()
    {
        _dataType = "bigint";
        _dotNetType = "long";
        return this;
    }

    /// <summary>
    /// Makes the column a VARCHAR type with specified length.
    /// </summary>
    public ColumnBuilder AsVarchar(int length = 50)
    {
        _dataType = "varchar";
        _dotNetType = "string";
        _maxLength = length;
        return this;
    }

    /// <summary>
    /// Makes the column an NVARCHAR type with specified length.
    /// </summary>
    public ColumnBuilder AsNVarchar(int length = 50)
    {
        _dataType = "nvarchar";
        _dotNetType = "string";
        _maxLength = length;
        return this;
    }

    /// <summary>
    /// Makes the column an NVARCHAR type (alias for AsNVarchar).
    /// </summary>
    public ColumnBuilder AsNvarchar(int length = 50)
    {
        return AsNVarchar(length);
    }

    /// <summary>
    /// Makes the column a DATETIME type.
    /// </summary>
    public ColumnBuilder AsDateTime()
    {
        _dataType = "datetime";
        _dotNetType = "DateTime";
        return this;
    }

    /// <summary>
    /// Makes the column a BIT (boolean) type.
    /// </summary>
    public ColumnBuilder AsBit()
    {
        _dataType = "bit";
        _dotNetType = "bool";
        return this;
    }

    /// <summary>
    /// Makes the column a DECIMAL type.
    /// </summary>
    public ColumnBuilder AsDecimal(int precision = 18, int scale = 2)
    {
        _dataType = "decimal";
        _dotNetType = "decimal";
        _precision = precision;
        _scale = scale;
        return this;
    }

    /// <summary>
    /// Makes the column NOT NULL.
    /// </summary>
    public ColumnBuilder NotNullable()
    {
        _isNullable = false;
        return this;
    }

    /// <summary>
    /// Makes the column NULLABLE.
    /// </summary>
    public ColumnBuilder Nullable()
    {
        _isNullable = true;
        return this;
    }

    /// <summary>
    /// Makes the column a Primary Key.
    /// </summary>
    public ColumnBuilder AsPrimaryKey()
    {
        _isPrimaryKey = true;
        _isNullable = false;
        return this;
    }

    /// <summary>
    /// Makes the column an IDENTITY column.
    /// </summary>
    public ColumnBuilder AsIdentity()
    {
        _isIdentity = true;
        _isPrimaryKey = true;
        _isNullable = false;
        return this;
    }

    /// <summary>
    /// Sets a default value for the column.
    /// </summary>
    public ColumnBuilder WithDefaultValue(string defaultValue)
    {
        _defaultValue = defaultValue;
        return this;
    }

    /// <summary>
    /// Sets the column prefix (eno, ent, enm, etc.)
    /// </summary>
    public ColumnBuilder WithPrefix(ColumnPrefix prefix)
    {
        _prefix = prefix;
        return this;
    }

    /// <summary>
    /// Makes the column encrypted (one-way: eno prefix).
    /// </summary>
    public ColumnBuilder AsOneWayEncrypted()
    {
        _prefix = ColumnPrefix.OneWayEncryption;
        _isEncrypted = true;
        _dataType = "varchar";
        _dotNetType = "string";
        _maxLength = 64;
        return this;
    }

    /// <summary>
    /// Makes the column encrypted (one-way: eno prefix) - alias.
    /// </summary>
    public ColumnBuilder WithOneWayEncryption()
    {
        return AsOneWayEncrypted();
    }

    /// <summary>
    /// Makes the column encrypted (two-way: ent prefix).
    /// </summary>
    public ColumnBuilder AsTwoWayEncrypted()
    {
        _prefix = ColumnPrefix.TwoWayEncryption;
        _isEncrypted = true;
        _dataType = "varchar";
        _dotNetType = "string";
        _maxLength = -1; // MAX
        return this;
    }

    /// <summary>
    /// Makes the column encrypted (two-way: ent prefix) - alias.
    /// </summary>
    public ColumnBuilder WithTwoWayEncryption()
    {
        return AsTwoWayEncrypted();
    }

    /// <summary>
    /// Makes the column an enum field (enm prefix).
    /// </summary>
    public ColumnBuilder AsEnumeration()
    {
        _prefix = ColumnPrefix.Enumeration;
        _dataType = "int";
        _dotNetType = "int";
        return this;
    }

    /// <summary>
    /// Makes the column a lookup field (lkp prefix).
    /// </summary>
    public ColumnBuilder AsLookup()
    {
        _prefix = ColumnPrefix.Lookup;
        _dataType = "nvarchar";
        _dotNetType = "string";
        _maxLength = 50;
        return this;
    }

    /// <summary>
    /// Makes the column localizable (loc prefix).
    /// </summary>
    public ColumnBuilder AsLocalization()
    {
        _prefix = ColumnPrefix.Localization;
        return this;
    }

    /// <summary>
    /// Makes the column localizable (loc prefix) - alias.
    /// </summary>
    public ColumnBuilder AsLocalizable()
    {
        return AsLocalization();
    }

    /// <summary>
    /// Makes the column calculated (clc_ prefix).
    /// </summary>
    public ColumnBuilder AsCalculated()
    {
        _prefix = ColumnPrefix.Calculated;
        _isReadOnly = true;
        return this;
    }

    /// <summary>
    /// Makes the column business logic field (blg_ prefix).
    /// </summary>
    public ColumnBuilder AsBusinessLogic()
    {
        _prefix = ColumnPrefix.BusinessLogic;
        _isReadOnly = true;
        return this;
    }

    /// <summary>
    /// Makes the column aggregate field (agg_ prefix).
    /// </summary>
    public ColumnBuilder AsAggregate()
    {
        _prefix = ColumnPrefix.Aggregate;
        _isReadOnly = true;
        return this;
    }

    /// <summary>
    /// Makes the column separate update field (spt_ prefix).
    /// </summary>
    public ColumnBuilder AsSeparateUpdate()
    {
        _prefix = ColumnPrefix.SeparateUpdate;
        return this;
    }

    /// <summary>
    /// Adds an extended property.
    /// </summary>
    public ColumnBuilder WithExtendedProperty(string key, string value)
    {
        _extendedProperties[key] = value;
        return this;
    }

    /// <summary>
    /// Adds ccType extended property.
    /// </summary>
    public ColumnBuilder WithCcType(string ccType)
    {
        _extendedProperties["ccType"] = ccType;
        return this;
    }

    /// <summary>
    /// Builds the Column object.
    /// </summary>
    public Column Build()
    {
        if (string.IsNullOrWhiteSpace(_name))
        {
            throw new ArgumentNullException(nameof(_name), "Column name cannot be null or empty");
        }

        return new Column
        {
            Name = _name,
            DataType = _dataType,
            DotNetType = _dotNetType,
            IsNullable = _isNullable,
            IsPrimaryKey = _isPrimaryKey,
            IsIdentity = _isIdentity,
            MaxLength = _maxLength,
            Precision = _precision,
            Scale = _scale,
            DefaultValue = _defaultValue,
            Prefix = _prefix,
            IsEncrypted = _isEncrypted,
            IsReadOnly = _isReadOnly,
            ExtendedProperties = _extendedProperties
        };
    }

    /// <summary>
    /// Creates a new ColumnBuilder instance.
    /// </summary>
    public static ColumnBuilder New() => new();

    /// <summary>
    /// Creates a typical ID column (Primary Key, IDENTITY, INT).
    /// </summary>
    public static Column IdColumn(string name = "ID") =>
        new ColumnBuilder()
            .WithName(name)
            .AsInt()
            .AsIdentity()
            .Build();

    /// <summary>
    /// Creates a typical ID column (alias for IdColumn).
    /// </summary>
    public static Column CreateIdColumn(string name = "ID") => IdColumn(name);

    /// <summary>
    /// Creates a typical Name column (NVARCHAR(100), NOT NULL).
    /// </summary>
    public static Column NameColumn(string name = "Name") =>
        new ColumnBuilder()
            .WithName(name)
            .AsNVarchar(100)
            .NotNullable()
            .Build();

    /// <summary>
    /// Creates a typical Name column (alias for NameColumn).
    /// </summary>
    public static Column CreateNameColumn(string name = "Name") => NameColumn(name);

    /// <summary>
    /// Creates a typical Foreign Key column (INT, NOT NULL).
    /// </summary>
    public static Column ForeignKeyColumn(string name)
    {
        var column = new ColumnBuilder()
            .WithName(name)
            .AsInt()
            .NotNullable()
            .Build();
        column.IsForeignKey = true;
        return column;
    }

    /// <summary>
    /// Creates a typical Foreign Key column (alias for ForeignKeyColumn).
    /// </summary>
    public static Column CreateForeignKeyColumn(string name) => ForeignKeyColumn(name);

    /// <summary>
    /// Creates a typical DateTime column with default GETDATE().
    /// </summary>
    public static Column CreatedDateColumn(string name = "CreatedDate") =>
        new ColumnBuilder()
            .WithName(name)
            .AsDateTime()
            .NotNullable()
            .WithDefaultValue("getdate()")
            .Build();
}
