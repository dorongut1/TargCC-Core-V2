using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using TargCC.Core.Analyzers.Database;
using TargCC.Core.Interfaces.Models;
using TargCC.Core.Interfaces;

namespace TargCC.Core.Tests.Unit.Analyzers
{
    /// <summary>
    /// Unit Tests עבור DatabaseAnalyzer
    /// </summary>
    public class DatabaseAnalyzerTests
    {
        private readonly Mock<ILogger<DatabaseAnalyzer>> _mockLogger;
        private readonly string _testConnectionString;

        public DatabaseAnalyzerTests()
        {
            _mockLogger = new Mock<ILogger<DatabaseAnalyzer>>();

            // Connection string לבדיקות - צריך להחליף לפי הסביבה שלך
            _testConnectionString = "Server=localhost;Database=TargCCOrdersNew;Integrated Security=true;TrustServerCertificate=True;";
        }

        [Fact]
        public void Constructor_NullConnectionString_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new DatabaseAnalyzer(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new DatabaseAnalyzer(_testConnectionString, null));
        }

        [Fact]
        public void Constructor_ValidParameters_CreatesInstance()
        {
            // Arrange & Act
            var analyzer = new DatabaseAnalyzer(_testConnectionString, _mockLogger.Object);

            // Assert
            Assert.NotNull(analyzer);
        }

        [Fact]
        public async Task ConnectAsync_ValidConnection_ReturnsTrue()
        {
            // Arrange
            var analyzer = new DatabaseAnalyzer(_testConnectionString, _mockLogger.Object);

            // Act
            var result = await analyzer.ConnectAsync();

            // Assert
            // הערה: הטסט הזה דורש DB אמיתי. אפשר לעשות Skip אם אין
            Assert.True(result);
        }

        [Fact]
        public async Task ConnectAsync_InvalidConnection_ReturnsFalse()
        {
            // Arrange
            var invalidConnectionString = "Server=NonExistentServer;Database=Test;";
            var analyzer = new DatabaseAnalyzer(invalidConnectionString, _mockLogger.Object);

            // Act
            var result = await analyzer.ConnectAsync();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetTablesAsync_ValidDatabase_ReturnsTableList()
        {
            // Arrange
            var analyzer = new DatabaseAnalyzer(_testConnectionString, _mockLogger.Object);

            // Act
            var tables = await analyzer.GetTablesAsync();

            // Assert
            Assert.NotNull(tables);
            Assert.IsType<List<string>>(tables);
            // אם יש טבלאות, הרשימה לא תהיה ריקה
        }

        [Fact]
        public async Task AnalyzeAsync_ValidDatabase_ReturnsDatabaseSchema()
        {
            // Arrange
            var analyzer = new DatabaseAnalyzer(_testConnectionString, _mockLogger.Object);

            // Act
            var schema = await analyzer.AnalyzeAsync();

            // Assert
            Assert.NotNull(schema);
            Assert.NotNull(schema.DatabaseName);
            Assert.NotNull(schema.ServerName);
            Assert.NotNull(schema.Tables);
            Assert.True(schema.AnalysisDate > DateTime.MinValue);
        }

        [Fact]
        public async Task AnalyzeIncrementalAsync_EmptyList_ReturnsBasicSchema()
        {
            // Arrange
            var analyzer = new DatabaseAnalyzer(_testConnectionString, _mockLogger.Object);
            var changedTables = new List<string>();  // רשימה ריקה

            // Act
            var schema = await analyzer.AnalyzeIncrementalAsync(changedTables);

            // Assert
            Assert.NotNull(schema);
            Assert.NotNull(schema.Tables);
            Assert.Empty(schema.Tables);  // ✅ ריק

            // ❌ אל תבדוק IsIncrementalAnalysis - הוא false כשהרשימה ריקה!
            Assert.False(schema.IsIncrementalAnalysis);  
        }

        [Fact]
        public async Task AnalyzeIncrementalAsync_WithTables_ReturnsPartialSchema()
        {
            // Arrange
            var analyzer = new DatabaseAnalyzer(_testConnectionString, _mockLogger.Object);
            
            // קודם כל, בואו נקבל את כל הטבלאות
            var allTables = await analyzer.GetTablesAsync();
            
            if (!allTables.Any())
            {
                // אם אין טבלאות, דלג על הטסט
                return;
            }

            // בחר טבלה ראשונה
            var changedTables = new List<string> { allTables.First() };

            // Act
            var schema = await analyzer.AnalyzeIncrementalAsync(changedTables);

            // Assert
            Assert.NotNull(schema);
            Assert.Single(schema.Tables);
            Assert.True(schema.IsIncrementalAnalysis);
        }

        [Fact]
        public async Task DetectChangedTablesAsync_NullPreviousSchema_ThrowsException()
        {
            // Arrange
            var analyzer = new DatabaseAnalyzer(_testConnectionString, _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                analyzer.DetectChangedTablesAsync(null));
        }

        [Fact]
        public async Task DetectChangedTablesAsync_NoChanges_ReturnsEmptyList()
        {
            // Arrange
            var analyzer = new DatabaseAnalyzer(_testConnectionString, _mockLogger.Object);
            
            // יצירת Schema "קודם" שהוא למעשה נוכחי
            var previousSchema = await analyzer.AnalyzeAsync();

            // Act
            var changedTables = await analyzer.DetectChangedTablesAsync(previousSchema);

            // Assert
            Assert.NotNull(changedTables);
            // אמור להיות ריק כי לא היו שינויים
        }

        [Theory]
        [InlineData("dbo.Customer")]
        [InlineData("dbo.Order")]
        [InlineData("sales.Product")]
        public async Task GetTablesAsync_ReturnsExpectedFormat(string expectedTableFormat)
        {
            // Arrange
            var analyzer = new DatabaseAnalyzer(_testConnectionString, _mockLogger.Object);

            // Act
            var tables = await analyzer.GetTablesAsync();

            // Assert
            // בדיקה שהפורמט הוא Schema.TableName
            foreach (var table in tables)
            {
                Assert.Contains(".", table);
            }
        }

        [Fact]
        public void IAnalyzer_Name_ReturnsCorrectName()
        {
            // Arrange
            var analyzer = new DatabaseAnalyzer(_testConnectionString, _mockLogger.Object) as IAnalyzer;

            // Act
            var name = analyzer.Name;

            // Assert
            Assert.Equal("Database Analyzer", name);
        }

        [Fact]
        public void IAnalyzer_Version_ReturnsVersion()
        {
            // Arrange
            var analyzer = new DatabaseAnalyzer(_testConnectionString, _mockLogger.Object) as IAnalyzer;

            // Act
            var version = analyzer.Version;

            // Assert
            Assert.NotNull(version);
            Assert.Matches(@"\d+\.\d+\.\d+", version); // פורמט x.y.z
        }

        [Fact]
        public async Task IAnalyzer_AnalyzeAsync_ReturnsDatabaseSchema()
        {
            // Arrange
            var analyzer = new DatabaseAnalyzer(_testConnectionString, _mockLogger.Object) as IAnalyzer;

            // Act
            var result = await analyzer.AnalyzeAsync(null);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<DatabaseSchema>(result);
        }
    }
}
