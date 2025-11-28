//// <copyright file="SecurityAnalysisResult.cs" company="Doron Vaida">
//// Copyright (c) Doron Vaida. All rights reserved.
//// </copyright>

//namespace TargCC.CLI.Models.Analysis;

///// <summary>
///// Result of security analysis.
///// </summary>
//public class SecurityAnalysisResult
//{
//    /// <summary>
//    /// Gets or sets the list of security issues.
//    /// </summary>
//    public List<SecurityIssue> Issues { get; set; } = new();

//    /// <summary>
//    /// Gets or sets the total number of fields checked.
//    /// </summary>
//    public int TotalFieldsChecked { get; set; }

//    /// <summary>
//    /// Gets or sets the number of compliant fields.
//    /// </summary>
//    public int CompliantFields { get; set; }

//    /// <summary>
//    /// Gets the compliance percentage.
//    /// </summary>
//    public double CompliancePercentage =>
//        TotalFieldsChecked > 0 ? (double)CompliantFields / TotalFieldsChecked * 100 : 0;
//}

///// <summary>
///// Security issue found during analysis.
///// </summary>
//public class SecurityIssue
//{
//    /// <summary>
//    /// Gets or sets the table name.
//    /// </summary>
//    public string TableName { get; set; } = string.Empty;

//    /// <summary>
//    /// Gets or sets the column name.
//    /// </summary>
//    public string ColumnName { get; set; } = string.Empty;

//    /// <summary>
//    /// Gets or sets the issue description.
//    /// </summary>
//    public string Description { get; set; } = string.Empty;

//    /// <summary>
//    /// Gets or sets the severity level.
//    /// </summary>
//    public SecuritySeverity Severity { get; set; }

//    /// <summary>
//    /// Gets or sets the recommendation.
//    /// </summary>
//    public string Recommendation { get; set; } = string.Empty;
//}

///// <summary>
///// Security issue severity levels.
///// </summary>
//public enum SecuritySeverity
//{
//    /// <summary>
//    /// High priority issue.
//    /// </summary>
//    High,

//    /// <summary>
//    /// Medium priority issue.
//    /// </summary>
//    Medium,

//    /// <summary>
//    /// Low priority issue.
//    /// </summary>
//    Low,
//}
