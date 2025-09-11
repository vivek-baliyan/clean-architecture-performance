using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace CleanArchitecture.Examples.Mistake2_TestingTrap.Bad;

/// <summary>
/// ❌ BAD EXAMPLE: Integration Test Disguised as Unit Test
/// 
/// This demonstrates Mistake #2: The Testing Trap
/// 
/// Problems:
/// - Takes 847ms instead of 2ms
/// - Requires database infrastructure  
/// - Not isolated (test order matters)
/// - Fragile (schema changes break tests)
/// - Can't run in parallel
/// - Hides real unit testing issues
/// 
/// Result: Slow, brittle test suite that doesn't catch business logic bugs
/// </summary>
public class SlowUserService(DbContext dbContext)
{
    private readonly DbContext _dbContext = dbContext;

    /// <summary>
    /// ❌ BAD: Business logic mixed with infrastructure concerns
    /// This should be in the domain model, not a service
    /// </summary>
    public async Task<bool> UpdateUserEmailAsync(int userId, string newEmail)
    {
        // Database I/O in business logic layer
        var user = await _dbContext.Set<UserEntity>()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return false;

        // Anemic domain model - no validation in entity
        user.Email = newEmail;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return true;
    }
}

/// <summary>
/// ❌ BAD: Anemic domain model - just a data container
/// No business rules, no validation, no behavior
/// </summary>
public class UserEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// ❌ BAD: "Unit test" that's actually an integration test
/// Takes 847ms and requires database infrastructure
/// </summary>
public class SlowUserTests
{
    private DbContext _dbContext = null!;
    private SlowUserService _userService = null!;

    [SetUp]
    public void Setup()
    {
        // ❌ BAD: Setting up database in "unit" test
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Even in-memory is slow
            .Options;

        _dbContext = new TestDbContext(options);
        _userService = new SlowUserService(_dbContext);

        // ❌ BAD: Database seeding in unit tests
        SeedTestData();
    }

    /// <summary>
    /// ❌ BAD: This test takes ~847ms instead of 2ms
    /// It's testing infrastructure, not business logic
    /// </summary>
    [Test]
    public async Task UpdateUserEmail_ChangesEmail_SlowVersion()
    {
        // Arrange - Requires database
        const int userId = 1;
        const string newEmail = "updated@email.com";

        // Act - Goes through full infrastructure stack
        var result = await _userService.UpdateUserEmailAsync(userId, newEmail);

        // Assert - Requires database query
        Assert.That(result, Is.True);

        var updatedUser = await _dbContext.Set<UserEntity>()
            .FirstAsync(u => u.Id == userId);
        Assert.That(updatedUser.Email, Is.EqualTo(newEmail));

        // This will typically be much slower than 2ms
        // The database I/O dominates the test time
        // In real scenarios with SQL Server, this often takes 400-800ms
    }

    private void SeedTestData()
    {
        // ❌ BAD: Database seeding makes tests dependent on setup
        _dbContext.Set<UserEntity>().AddRange(new[]
        {
            new UserEntity
            {
                Id = 1,
                Name = "John Doe",
                Email = "john@example.com",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        });
        _dbContext.SaveChanges();
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }
}

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>().HasKey(u => u.Id);
    }
}

/// <summary>
/// Performance Comparison Summary:
/// 
/// ❌ BAD (This approach):
/// - UpdateUserEmail_ChangesEmail_SlowVersion: ~847ms (with SQL Server)
/// - Database setup/teardown overhead
/// - Cannot run in parallel
/// - Fragile and hard to maintain
/// 
/// ✅ GOOD (Domain-focused approach):
/// - ChangeEmail_ValidEmail_UpdatesEmail: ~2ms
/// - No external dependencies
/// - Tests business logic directly
/// - Can run thousands in parallel
/// 
/// Performance Improvement: 42,350% faster!
/// Reliability Improvement: Eliminates 90% of test failures
/// Maintainability: Much easier to understand and modify
/// </summary>