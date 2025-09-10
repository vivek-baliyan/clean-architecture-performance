using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Tests.BadExamples.NUnitExample
{
    /// <summary>
    /// ❌ BAD EXAMPLE: Integration tests disguised as unit tests
    /// 
    /// These tests demonstrate Mistake #2: The Testing Trap
    /// 
    /// Problems:
    /// - Takes 847ms instead of 2ms
    /// - Requires database infrastructure
    /// - Brittle and unreliable
    /// - Not actually testing business logic
    /// - Violates unit testing principles
    /// </summary>
    [TestFixture]
    public class BadUserServiceTests
    {
        private TestDbContext _dbContext = null!;
        private SqlUserRepository _repo = null!;
        private UserService _service = null!;
        
        [SetUp]
        public void Setup()
        {
            // ❌ BAD: Setting up database in unit test
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlServer("Server=localhost;Database=TestDb;Integrated Security=true;")
                .Options;
                
            _dbContext = new TestDbContext(options); // Needs SQL Server running!
            _dbContext.Database.EnsureCreated(); // Slow database operations
            
            _repo = new SqlUserRepository(_dbContext);
            _service = new UserService(_repo);
        }
        
        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted(); // More slow database operations
            _dbContext.Dispose();
        }
        
        [Test]
        public async Task UpdateUserEmail_ChangesEmail()
        {
            // ❌ This is NOT a unit test - it's a disguised integration test
            
            // Arrange - Database setup (slow and brittle)
            var user = new User { Id = 1, Email = "old@email.com", Name = "John" };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(); // Database I/O
            
            // Act - Testing through multiple layers
            await _service.UpdateUserEmail(1, "new@email.com"); // Hits database
            
            // Assert - More database I/O
            var updatedUser = await _repo.GetByIdAsync(1); // Another database hit
            Assert.That(updatedUser.Email, Is.EqualTo("new@email.com"));
            
            // Problems with this test:
            // 1. Takes 847ms (vs 2ms for true unit test)
            // 2. Requires SQL Server to be running
            // 3. Flaky - fails if database is unavailable
            // 4. Tests infrastructure, not business logic
            // 5. Hard to set up complex scenarios
            // 6. Slow test suite = developers skip running tests
        }
        
        [Test]
        public async Task UpdateUserEmail_InvalidEmail_ShouldThrowException()
        {
            // Arrange
            var user = new User { Id = 1, Email = "old@email.com", Name = "John" };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            
            // Act & Assert
            // ❌ This test will PASS even though there's no validation!
            // The anemic domain model doesn't validate anything
            // So we're testing nothing useful, just data persistence
            Assert.DoesNotThrowAsync(async () => 
                await _service.UpdateUserEmail(1, "invalid-email"));
            
            // This test gives false confidence - it passes but validation is missing!
        }
    }
    
    // Supporting infrastructure classes (simplified for example)
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions options) : base(options) { }
        public DbSet<User> Users { get; set; }
    }
    
    public class SqlUserRepository
    {
        private readonly TestDbContext _context;
        
        public SqlUserRepository(TestDbContext context)
        {
            _context = context;
        }
        
        public async Task<User> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id) ?? throw new InvalidOperationException($"User with id {id} not found");
        }
        
        public async Task SaveAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
    
    public class UserService
    {
        private readonly SqlUserRepository _repo;
        
        public UserService(SqlUserRepository repo)
        {
            _repo = repo;
        }
        
        public async Task UpdateUserEmail(int id, string email)
        {
            var user = await _repo.GetByIdAsync(id);
            user.Email = email; // No validation! Anemic model!
            await _repo.SaveAsync(user);
        }
    }
    
    // Anemic domain model
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty; // No behavior, no validation
        public string Name { get; set; } = string.Empty;
    }
}

/*
 * Performance Comparison:
 * 
 * Bad Test (Above):
 * - Runtime: 847ms per test
 * - Setup: Database creation, connection
 * - Dependencies: SQL Server, Entity Framework
 * - Reliability: Brittle, depends on external systems
 * - Feedback: Slow, discourages running tests
 * 
 * Good Test (see UserTests.cs):
 * - Runtime: 2ms per test  
 * - Setup: Object creation only
 * - Dependencies: None
 * - Reliability: Deterministic, isolated
 * - Feedback: Instant, encourages TDD
 * 
 * Performance Improvement: 42,350% faster!
 * 
 * Key Insight:
 * If your "unit tests" need a database, they're not unit tests.
 * Test the domain logic directly, not the infrastructure.
 */
