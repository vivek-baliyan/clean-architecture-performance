# Clean Architecture Performance Mistakes

> 5 Clean Architecture Mistakes That Kill .NET Performance (and How to Fix Them)

This repository demonstrates common Clean Architecture implementation mistakes that hurt performance in .NET applications, along with practical fixes and benchmarks.

## 📊 Performance Impact Summary

| Mistake | Performance Impact | Fix Impact |
|---------|-------------------|------------|
| Folder Illusion | 30-40% slower delivery | Proper dependency direction |
| Testing Trap | 847ms → 2ms test runs | True unit testing |
| Too Many Layers | 35% request time wasted | Strategic layer collapse |
| Cargo Cult | Delivery paralysis | Value-driven decisions |
| Interface Overload | Runtime + cognitive overhead | Right-sized abstractions |

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 / VS Code

### Clone and Run

```bash
git clone https://github.com/vivek-baliyan/clean-architecture-performance.git
cd clean-architecture-performance

# Run benchmarks
dotnet run --project benchmarks --configuration Release

# Run all tests
dotnet test

# Architecture audit
./tools/architecture-audit.ps1 -ProjectPath "src"
```

## 📁 Mistake Examples

### Mistake 1: The Folder Illusion

**Problem**: Putting interfaces in Infrastructure instead of Domain

```csharp
// ❌ BAD - Infrastructure/IUserRepository.cs
public interface IUserRepository { ... }

// ✅ GOOD - Domain/IUserRepository.cs  
public interface IUserRepository { ... }
```

### Mistake 2: The Testing Trap

**Problem**: Unit tests that secretly depend on databases

```csharp
// ❌ BAD - 847ms test with database dependency
[Test]
public async Task UpdateUserEmail_ChangesEmail()
{
    var dbContext = new TestDbContext(); // Needs SQL Server
    // ...
}

// ✅ GOOD - 2ms test with no dependencies
[Test]  
public void ChangeEmail_ValidEmail_UpdatesEmail()
{
    var user = new User(UserId.Create(1), "old@email.com");
    user.ChangeEmail(EmailAddress.Create("new@email.com"));
    Assert.That(user.Email.Value, Is.EqualTo("new@email.com"));
}
```

### Mistake 3: Too Many Layers

**Problem**: Excessive mapping killing performance

```csharp
// ❌ BAD - 847ms with 4 mappings
var entity = await _dbContext.Customers.FindAsync(id);    // SQL → EF Entity
var domain = _mapper.Map<Customer>(entity);               // Entity → Domain  
var dto = _mapper.Map<CustomerDto>(domain);               // Domain → DTO
var view = _mapper.Map<CustomerViewModel>(dto);           // DTO → ViewModel

// ✅ GOOD - 312ms with direct projection
return await _dbContext.Customers
    .Where(c => c.Id == id)
    .Select(c => new CustomerView(c.Id, c.Name, c.Email))
    .FirstAsync();
```

## 🔧 Tools & Benchmarks

This repository includes:

- **🔍 Architecture Audit Tool** - Validates clean architecture principles
- **⚡ Performance Benchmarks** - Proves the performance claims
- **🧪 Test Examples** - Shows proper vs improper testing
- **📊 Migration Guide** - Step-by-step fixes

## 📚 Resources

- [Original Medium Article](https://medium.com/@vivekbaliyan/5-clean-architecture-mistakes-that-kill-net-performance)
- [Clean Architecture by Uncle Bob](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

## ⭐ Star History

If this repository helped you improve your Clean Architecture implementation, please give it a star! 

---

**Follow [@vivekbaliyan](https://medium.com/@vivekbaliyan) for more real-world .NET architecture and performance lessons.**
