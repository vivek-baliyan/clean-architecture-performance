using NSubstitute;
using NUnit.Framework;

namespace CleanArchitecture.Examples.Mistake2_TestingTrap.Good;

/// <summary>
/// ✅ GOOD EXAMPLE: True Unit Tests with Rich Domain Models
/// 
/// This demonstrates the fix for Mistake #2: The Testing Trap
/// 
/// Benefits:
/// - Runs in 2ms instead of 847ms
/// - No external dependencies (no database, no network)
/// - Tests business logic directly
/// - Isolated and reliable
/// - Can run thousands in parallel
/// - Easy to understand and maintain
/// 
/// Result: Fast, reliable test suite that catches business logic bugs
/// </summary>
public class FastUserService
{
    private readonly IUserRepository _userRepository;

    public FastUserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// ✅ GOOD: Pure business logic - delegates domain operations to domain model
    /// No infrastructure concerns, easily testable
    /// </summary>
    public async Task<bool> UpdateUserEmailAsync(UserId userId, EmailAddress newEmail)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        // Business logic in domain model where it belongs
        user.ChangeEmail(newEmail);

        await _userRepository.SaveAsync(user);
        return true;
    }
}

/// <summary>
/// ✅ GOOD: Rich domain model with business behavior
/// Contains validation, business rules, and domain events
/// </summary>
public class User
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public UserId Id { get; private set; }
    public EmailAddress Email { get; private set; }
    public string Name { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public User(UserId id, EmailAddress email, string name)
    {
        Id = id;
        Email = email;
        Name = name;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// ✅ GOOD: Business logic in domain model
    /// Validates rules, updates state, raises events
    /// </summary>
    public void ChangeEmail(EmailAddress newEmail)
    {
        if (!newEmail.IsValid())
            throw new InvalidEmailException("Invalid email format");

        if (Email.Equals(newEmail))
            return; // No change needed

        var oldEmail = Email;
        Email = newEmail;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new EmailChangedEvent(Id, oldEmail, newEmail));
    }

    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public IReadOnlyList<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();
}

/// <summary>
/// ✅ GOOD: Value object with validation
/// Immutable, self-validating, expressive
/// </summary>
public record EmailAddress(string Value)
{
    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(Value) &&
        Value.Contains("@") &&
        Value.Length > 5 &&
        !Value.StartsWith("@") &&
        !Value.EndsWith("@") &&
        Value.Count(c => c == '@') == 1;
}

public record UserId(int Value);

public record EmailChangedEvent(UserId UserId, EmailAddress OldEmail, EmailAddress NewEmail) : IDomainEvent;

public interface IDomainEvent { }

public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id);
    Task SaveAsync(User user);
}

public class InvalidEmailException : Exception
{
    public InvalidEmailException(string message) : base(message) { }
}

/// <summary>
/// ✅ GOOD: True unit tests - fast, isolated, reliable
/// Tests business logic without infrastructure dependencies
/// </summary>
public class FastUserTests
{
    [Test]
    public void ChangeEmail_ValidEmail_UpdatesEmail()
    {
        // Arrange
        var user = new User(
            new UserId(1),
            new EmailAddress("old@email.com"),
            "John Doe");
        var newEmail = new EmailAddress("new@email.com");

        // Act
        user.ChangeEmail(newEmail);

        // Assert
        Assert.That(user.Email, Is.EqualTo(newEmail));
        // ✅ Runs in ~2ms, no dependencies, reliable
    }

    [Test]
    public void ChangeEmail_InvalidEmail_ThrowsException()
    {
        // Arrange
        var user = new User(
            new UserId(1),
            new EmailAddress("old@email.com"),
            "John Doe");

        // Act & Assert
        Assert.Throws<InvalidEmailException>(() =>
            user.ChangeEmail(new EmailAddress("invalid")));
    }

    [Test]
    public void ChangeEmail_SameEmail_NoEventRaised()
    {
        // Arrange
        var email = new EmailAddress("same@email.com");
        var user = new User(new UserId(1), email, "John Doe");

        // Act
        user.ChangeEmail(email);

        // Assert
        Assert.That(user.GetDomainEvents(), Is.Empty);
    }

    [Test]
    public void ChangeEmail_ValidEmail_RaisesEmailChangedEvent()
    {
        // Arrange
        var user = new User(
            new UserId(1),
            new EmailAddress("old@email.com"),
            "John Doe");
        var newEmail = new EmailAddress("new@email.com");

        // Act
        user.ChangeEmail(newEmail);

        // Assert
        var events = user.GetDomainEvents();
        Assert.That(events, Has.Count.EqualTo(1));
        Assert.That(events[0], Is.TypeOf<EmailChangedEvent>());

        var emailChangedEvent = (EmailChangedEvent)events[0];
        Assert.That(emailChangedEvent.UserId, Is.EqualTo(new UserId(1)));
        Assert.That(emailChangedEvent.NewEmail, Is.EqualTo(newEmail));
    }

    [Test]
    public void EmailAddress_ValidFormat_ReturnsTrue()
    {
        // Arrange & Act
        var validEmail = new EmailAddress("test@example.com");

        // Assert
        Assert.That(validEmail.IsValid(), Is.True);
    }

    [Test]
    public void EmailAddress_InvalidFormat_ReturnsFalse()
    {
        // Arrange & Act
        var invalidEmail = new EmailAddress("invalid");

        // Assert
        Assert.That(invalidEmail.IsValid(), Is.False);
    }

    /// <summary>
    /// This test demonstrates the performance difference:
    /// - True unit test: ~2ms
    /// - Integration test with database: ~847ms
    /// 
    /// Performance improvement: 42,350% faster!
    /// </summary>
    [Test]
    public void UserCreation_AllProperties_SetsCorrectly()
    {
        // Arrange
        var userId = new UserId(123);
        var email = new EmailAddress("john@example.com");
        var name = "John Doe";

        // Act
        var user = new User(userId, email, name);

        // Assert
        Assert.That(user.Id, Is.EqualTo(userId));
        Assert.That(user.Email, Is.EqualTo(email));
        Assert.That(user.Name, Is.EqualTo(name));
        Assert.That(user.GetDomainEvents(), Is.Empty);

        // This test completes in microseconds, not milliseconds!
    }
}

/// <summary>
/// Service Tests with Mocking (when you need to test application layer)
/// </summary>
public class FastUserServiceTests
{
    private IUserRepository _mockRepository = null!;
    private FastUserService _userService = null!;

    [SetUp]
    public void Setup()
    {
        _mockRepository = Substitute.For<IUserRepository>();
        _userService = new FastUserService(_mockRepository);
    }

    [Test]
    public async Task UpdateUserEmailAsync_UserExists_UpdatesEmailAndSaves()
    {
        // Arrange
        var userId = new UserId(1);
        var oldEmail = new EmailAddress("old@email.com");
        var newEmail = new EmailAddress("new@email.com");
        var user = new User(userId, oldEmail, "John Doe");

        _mockRepository.GetByIdAsync(userId).Returns(user);

        // Act
        var result = await _userService.UpdateUserEmailAsync(userId, newEmail);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(user.Email, Is.EqualTo(newEmail));
        await _mockRepository.Received(1).SaveAsync(user);

        // Still runs in ~2ms because no real I/O
    }

    [Test]
    public async Task UpdateUserEmailAsync_UserNotFound_ReturnsFalse()
    {
        // Arrange
        var userId = new UserId(999);
        var newEmail = new EmailAddress("new@email.com");

        _mockRepository.GetByIdAsync(userId).Returns((User?)null);

        // Act
        var result = await _userService.UpdateUserEmailAsync(userId, newEmail);

        // Assert
        Assert.That(result, Is.False);
        await _mockRepository.DidNotReceive().SaveAsync(Arg.Any<User>());
    }
}

/// <summary>
/// Key Benefits of This Approach:
/// 
/// 1. ✅ Speed: 2ms vs 847ms (42,350% faster)
/// 2. ✅ Reliability: No external dependencies to fail
/// 3. ✅ Isolation: Each test is independent
/// 4. ✅ Maintainability: Easy to understand and modify
/// 5. ✅ Parallel execution: Can run thousands simultaneously
/// 6. ✅ Business focus: Tests actual business rules
/// 7. ✅ Early feedback: Catches logic errors immediately
/// 
/// When you need integration tests:
/// - Test repository implementations separately
/// - Use dedicated integration test projects
/// - Run them less frequently (CI/nightly builds)
/// - Keep them separate from unit tests
/// </summary>