# Migration Guide

This guide helps you fix Clean Architecture performance issues in your existing projects.

## 3-Week Fix Cycle

### Week 1: Move Interfaces to Correct Locations

**Problem**: Repository interfaces in Infrastructure layer
**Fix**: Move them to Domain layer

```bash
# 1. Move interface files
mv Infrastructure/IUserRepository.cs Domain/IUserRepository.cs

# 2. Update namespaces
# Change: namespace MyApp.Infrastructure
# To:     namespace MyApp.Domain

# 3. Update project references
# Remove Infrastructure reference from Domain
# Add Domain reference to Infrastructure
```

**Before**:
```
Infrastructure/
├── IUserRepository.cs  ❌ Wrong location
└── SqlUserRepository.cs
```

**After**:
```
Domain/
├── IUserRepository.cs  ✅ Correct location
└── User.cs

Infrastructure/
└── SqlUserRepository.cs  (implements Domain.IUserRepository)
```

### Week 2: Fix Unit Test Performance

**Problem**: Unit tests that secretly depend on databases
**Fix**: Test domain logic directly

**Before (847ms test)**:
```csharp
[Test]
public async Task UpdateUserEmail_ChangesEmail()
{
    var dbContext = new TestDbContext(); // Database dependency!
    var repo = new SqlUserRepository(dbContext);
    var service = new UserService(repo);
    
    await service.UpdateUserEmail(1, "new@email.com");
    // Takes 847ms, brittle, requires database
}
```

**After (2ms test)**:
```csharp
[Test]  
public void ChangeEmail_ValidEmail_UpdatesEmail()
{
    var user = new User(UserId.Create(1), "old@email.com");
    
    user.ChangeEmail(EmailAddress.Create("new@email.com"));
    
    Assert.That(user.Email.Value, Is.EqualTo("new@email.com"));
    // Runs in 2ms, reliable, no dependencies
}
```

### Week 3: Optimize Layer Performance

**Problem**: Excessive mapping between layers
**Fix**: Use direct projections for read scenarios

**Before (847ms with 4 mappings)**:
```csharp
[HttpGet("{id}")]
public async Task<CustomerViewModel> Get(int id)
{
    var entity = await _dbContext.Customers.FindAsync(id);    // SQL → EF Entity
    var domain = _mapper.Map<Customer>(entity);               // Entity → Domain  
    var dto = _mapper.Map<CustomerDto>(domain);               // Domain → DTO
    var view = _mapper.Map<CustomerViewModel>(dto);           // DTO → ViewModel
    return view; // 35% of request time wasted on mapping
}
```

**After (312ms with direct projection)**:
```csharp
[HttpGet("{id}")]  
public async Task<CustomerView> Get(int id)
{
    return await _dbContext.Customers
        .Where(c => c.Id == id)
        .Select(c => new CustomerView(c.Id, c.Name, c.Email))
        .FirstAsync(); // 65% faster
}
```

## Quick Wins

### 1. Architecture Audit
```bash
./tools/architecture-audit.ps1 -ProjectPath "src" -GenerateReport
```

### 2. Performance Benchmarks
```bash
dotnet run --project benchmarks --configuration Release
```

### 3. Test Suite Speed Check
```bash
# Time your test suite
Measure-Command { dotnet test }

# Target: Unit tests should complete in seconds, not minutes
```

## Validation Checklist

After implementing fixes, verify:

- [ ] Domain project has zero external references
- [ ] Repository interfaces are in Domain, not Infrastructure  
- [ ] Unit tests run without database/network dependencies
- [ ] Test suite completes in under 30 seconds
- [ ] API response times improved by 20-30%
- [ ] Cold start times reduced (for serverless apps)

## Common Pitfalls

### ❌ Don't Do This
```csharp
// Interface in wrong layer
namespace MyApp.Infrastructure
{
    public interface IUserRepository { } // Wrong!
}

// Anemic domain model
public class User 
{
    public string Email { get; set; } // No behavior
}

// Database in unit test
[Test]
public async Task Test_WithDatabase() 
{
    var context = new DbContext(); // Wrong!
}
```

### ✅ Do This Instead
```csharp
// Interface in consuming layer
namespace MyApp.Domain
{
    public interface IUserRepository { } // Correct!
}

// Rich domain model
public class User 
{
    public void ChangeEmail(EmailAddress email) // Behavior!
    {
        // Business logic here
    }
}

// Pure unit test
[Test]
public void Test_PureDomainLogic() 
{
    var user = new User(); // No dependencies!
    // Test business logic
}
```

## Success Metrics

Track these improvements:

| Metric | Before | Target |
|--------|--------|--------|
| Unit test speed | 847ms/test | 2ms/test |
| API response time | 847ms | 312ms |
| Test suite runtime | 15 minutes | 30 seconds |
| Cold start time | 600ms | 70ms |

## Next Steps

1. **Run the audit tool** on your current project
2. **Fix the highest-impact issues first** (interface locations)
3. **Measure before and after** using benchmarks
4. **Repeat the cycle** quarterly to prevent regression

Remember: Clean Architecture isn't about perfect folders—it's about dependencies pointing inward and keeping your domain logic pure and testable.
