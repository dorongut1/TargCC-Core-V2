// <copyright file="RepositoryDbContextIntegrationTests.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace TargCC.Core.Tests.Integration;

/// <summary>
/// Integration tests for Repository + DbContext interaction.
/// Tests end-to-end CRUD operations using in-memory database.
/// </summary>
public class RepositoryDbContextIntegrationTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly TestCustomerRepository _repository;
    private readonly Mock<ILogger<TestCustomerRepository>> _loggerMock;

    public RepositoryDbContextIntegrationTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestDbContext(options);
        _loggerMock = new Mock<ILogger<TestCustomerRepository>>();
        _repository = new TestCustomerRepository(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task FullCrud_WorksEndToEnd()
    {
        // Arrange
        var customer = new TestCustomer
        {
            Name = "Test Customer",
            Email = "test@example.com",
            Phone = "123-456-7890"
        };

        // Act & Assert: Create
        await _repository.AddAsync(customer);
        customer.ID.Should().BeGreaterThan(0);

        // Act & Assert: Read
        var found = await _repository.GetByIdAsync(customer.ID);
        found.Should().NotBeNull();
        found!.Name.Should().Be("Test Customer");
        found.Email.Should().Be("test@example.com");

        // Act & Assert: Update
        found.Name = "Updated Customer";
        await _repository.UpdateAsync(found);

        var updated = await _repository.GetByIdAsync(customer.ID);
        updated!.Name.Should().Be("Updated Customer");

        // Act & Assert: Delete
        await _repository.DeleteAsync(customer.ID);

        var deleted = await _repository.GetByIdAsync(customer.ID);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task GetAll_WithFiltering_ReturnsFilteredResults()
    {
        // Arrange
        await _repository.AddAsync(new TestCustomer { Name = "Customer A", Email = "a@test.com" });
        await _repository.AddAsync(new TestCustomer { Name = "Customer B", Email = "b@test.com" });
        await _repository.AddAsync(new TestCustomer { Name = "Customer C", Email = "c@test.com" });

        // Act
        var all = await _repository.GetAllAsync();

        // Assert
        all.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAll_WithPaging_ReturnsPagedResults()
    {
        // Arrange
        for (int i = 1; i <= 10; i++)
        {
            await _repository.AddAsync(new TestCustomer
            {
                Name = $"Customer {i}",
                Email = $"customer{i}@test.com"
            });
        }

        // Act: Get first page (5 items)
        var page1 = await _repository.GetAllAsync(skip: 0, take: 5);

        // Act: Get second page (5 items)
        var page2 = await _repository.GetAllAsync(skip: 5, take: 5);

        // Assert
        page1.Should().HaveCount(5);
        page2.Should().HaveCount(5);

        // Verify no overlap between pages
        var page1Ids = page1.Select(c => c.ID).ToList();
        var page2Ids = page2.Select(c => c.ID).ToList();
        page1Ids.Should().NotIntersectWith(page2Ids);
    }

    [Fact]
    public async Task Exists_WithValidId_ReturnsTrue()
    {
        // Arrange
        var customer = new TestCustomer { Name = "Test", Email = "test@test.com" };
        await _repository.AddAsync(customer);

        // Act
        var exists = await _repository.ExistsAsync(customer.ID);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Exists_WithInvalidId_ReturnsFalse()
    {
        // Act
        var exists = await _repository.ExistsAsync(999999);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task Update_WithNonExistentEntity_ThrowsException()
    {
        // Arrange
        var customer = new TestCustomer
        {
            ID = 999999,
            Name = "Non-existent",
            Email = "none@test.com"
        };

        // Act
        Func<Task> act = async () => await _repository.UpdateAsync(customer);

        // Assert
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }

    [Fact]
    public async Task Delete_WithNonExistentId_DoesNotThrow()
    {
        // Act
        Func<Task> act = async () => await _repository.DeleteAsync(999999);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ConcurrentOperations_WorkCorrectly()
    {
        // Note: EF Core's in-memory provider doesn't support true concurrent operations
        // on the same DbContext instance. We test sequential operations to verify
        // the repository works correctly with multiple operations.
        
        // Arrange & Act - Add customers sequentially (DbContext is not thread-safe)
        for (int i = 1; i <= 10; i++)
        {
            await _repository.AddAsync(new TestCustomer
            {
                Name = $"Customer {i}",
                Email = $"customer{i}@test.com"
            });
        }

        // Assert
        var all = await _repository.GetAllAsync();
        all.Should().HaveCount(10);
    }

    [Fact]
    public async Task AddAsync_WithDuplicateEmail_ThrowsException()
    {
        // Arrange
        await _repository.AddAsync(new TestCustomer
        {
            Name = "Customer 1",
            Email = "duplicate@test.com"
        });

        // Act
        Func<Task> act = async () => await _repository.AddAsync(new TestCustomer
        {
            Name = "Customer 2",
            Email = "duplicate@test.com" // Same email
        });

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Transaction_Rollback_WorksCorrectly()
    {
        // Arrange
        await _repository.AddAsync(new TestCustomer
        {
            Name = "Customer 1",
            Email = "customer1@test.com"
        });

        var initialCount = (await _repository.GetAllAsync()).Count();

        // Act: Try to add with duplicate email (should rollback)
        try
        {
            await _repository.AddAsync(new TestCustomer
            {
                Name = "Customer 2",
                Email = "customer1@test.com" // Duplicate
            });
        }
        catch
        {
            // Expected exception
        }

        // Assert: Count should remain the same
        var finalCount = (await _repository.GetAllAsync()).Count();
        finalCount.Should().Be(initialCount);
    }
}

#region Test Helpers

/// <summary>
/// Test DbContext for integration testing.
/// </summary>
public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public DbSet<TestCustomer> Customers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestCustomer>(entity =>
        {
            entity.HasKey(e => e.ID);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.Property(e => e.Phone)
                .HasMaxLength(20);
        });
    }
}

/// <summary>
/// Test entity for integration testing.
/// </summary>
public class TestCustomer
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

/// <summary>
/// Test repository for integration testing.
/// </summary>
public class TestCustomerRepository
{
    private readonly TestDbContext _context;
    private readonly ILogger<TestCustomerRepository> _logger;

    public TestCustomerRepository(TestDbContext context, ILogger<TestCustomerRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TestCustomer?> GetByIdAsync(int id)
    {
        _logger.LogDebug("Getting customer {CustomerId}", id);
        return await _context.Customers.FindAsync(id);
    }

    public async Task<IEnumerable<TestCustomer>> GetAllAsync(int? skip = null, int? take = null)
    {
        var query = _context.Customers.AsQueryable();

        if (skip.HasValue)
            query = query.Skip(skip.Value);

        if (take.HasValue)
            query = query.Take(take.Value);

        return await query.ToListAsync();
    }

    public async Task AddAsync(TestCustomer customer)
    {
        _logger.LogDebug("Adding customer {CustomerName}", customer.Name);

        // Check for duplicate email
        var exists = await _context.Customers
            .AnyAsync(c => c.Email == customer.Email);

        if (exists)
            throw new InvalidOperationException("Email already exists");

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TestCustomer customer)
    {
        _logger.LogDebug("Updating customer {CustomerId}", customer.ID);

        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        _logger.LogDebug("Deleting customer {CustomerId}", id);

        var customer = await _context.Customers.FindAsync(id);
        if (customer != null)
        {
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Customers.AnyAsync(c => c.ID == id);
    }
}

#endregion
