# Contributing to Clean Architecture Performance

Thank you for your interest in improving this repository! 🚀

## Quick Start

1. **Fork the repository**
2. **Clone your fork**: `git clone https://github.com/YOUR-USERNAME/clean-architecture-performance.git`
3. **Create a branch**: `git checkout -b feature/your-improvement`
4. **Make your changes**
5. **Test your changes**: `dotnet test && dotnet run --project benchmarks --configuration Release`
6. **Submit a pull request**

## What We're Looking For

### 🎯 High-Priority Contributions

- **New Performance Anti-Patterns**: Documented with benchmarks
- **Real-World Examples**: Business scenarios beyond academic examples
- **Benchmark Improvements**: More accurate or realistic performance tests
- **Documentation**: Clear explanations with code examples
- **Architecture Tests**: Automated validation of Clean Architecture principles

### 📊 Performance Contributions

When adding performance-related content:

1. **Include Benchmarks**: Use BenchmarkDotNet with before/after comparisons
2. **Provide Evidence**: Screenshots or console output of benchmark results
3. **Explain Impact**: Business cost/benefit of the improvement
4. **Real Numbers**: Avoid exaggerated claims like "10,000x faster"

Example:
```csharp
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class YourBenchmark
{
    [Benchmark(Baseline = true)]
    public void SlowApproach() { /* your slow implementation */ }
    
    [Benchmark] 
    public void FastApproach() { /* your optimized implementation */ }
}
```

### 🏗️ Code Standards

- **Target Framework**: .NET 9.0
- **Language Features**: Use modern C# 13 features where appropriate
- **Nullable**: All projects have nullable reference types enabled
- **Testing**: xUnit + FluentAssertions for assertions
- **Architecture**: Follow the Clean Architecture principles demonstrated in the repository

### 📝 Documentation Standards

- **Clear Examples**: Working code that compiles and runs
- **Business Context**: Explain why the performance matters
- **Step-by-Step**: Include setup and execution instructions
- **Links**: Internal links should point to actual files

## Project Structure

```
├── src/                          # Implementation examples
│   ├── MistakeX-Name/
│   │   ├── Bad/                  # Anti-pattern examples
│   │   └── Good/                 # Optimized implementations
├── benchmarks/                   # Performance benchmarks
├── tests/
│   ├── Unit/                     # Fast unit tests (<5ms each)
│   └── BadExamples/              # Slow tests (for comparison)
├── docs/                         # Documentation and guides
└── tools/                        # Architecture validation scripts
```

## Types of Contributions

### 🐛 Bug Reports
- **Performance Issues**: Benchmarks don't run or produce unexpected results
- **Documentation Errors**: Broken links, unclear instructions
- **Code Issues**: Examples that don't compile or run

### 💡 Feature Requests
- **New Mistakes**: Additional Clean Architecture anti-patterns
- **Tools**: Architecture validation or performance monitoring
- **Examples**: Real-world scenarios or business cases

### 📈 Performance Improvements
- **Faster Implementations**: Better algorithms or optimizations
- **Memory Optimizations**: Reduced allocations
- **Architectural Improvements**: Better separation of concerns

## Code Review Process

1. **Automated Checks**: CI/CD validates that code compiles and tests pass
2. **Performance Validation**: Benchmarks must show measurable improvements
3. **Architecture Review**: Code follows Clean Architecture principles
4. **Documentation Review**: Examples are clear and well-explained

## Getting Help

- **Discussions**: Use [GitHub Discussions](https://github.com/vivek-baliyan/clean-architecture-performance/discussions) for questions
- **Issues**: Use [GitHub Issues](https://github.com/vivek-baliyan/clean-architecture-performance/issues) for bugs or feature requests
- **Performance Questions**: Tag issues with `performance` label

## Recognition

Contributors are recognized in:
- **README.md**: Contributors section
- **Release Notes**: Major contributions highlighted
- **Medium Articles**: Real contributions may be featured in follow-up articles

## Development Setup

### Prerequisites
- .NET 9.0 SDK
- Visual Studio 2022 17.8+ / VS Code / JetBrains Rider 2024.3+

### Build and Test
```bash
# Clone the repository
git clone https://github.com/vivek-baliyan/clean-architecture-performance.git
cd clean-architecture-performance

# Restore packages
dotnet restore

# Build solution
dotnet build

# Run fast tests (should complete in <5 seconds)
dotnet test tests/Unit

# Run performance benchmarks
dotnet run --project benchmarks --configuration Release

# Run architecture validation
dotnet test --filter "ArchitectureTests"
```

### Before Submitting

- [ ] Code compiles without warnings
- [ ] All tests pass
- [ ] Benchmarks run successfully
- [ ] Documentation is updated
- [ ] Architecture tests pass

## Code of Conduct

This project follows the [Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md). By participating, you agree to uphold this code.

## Questions?

Open a [GitHub Discussion](https://github.com/vivek-baliyan/clean-architecture-performance/discussions) or contact [@vivek-baliyan](https://github.com/vivek-baliyan).

---

**Happy Contributing!** 🎉
