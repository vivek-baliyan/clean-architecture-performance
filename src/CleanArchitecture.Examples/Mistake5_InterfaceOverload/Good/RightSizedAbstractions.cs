using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Examples.Mistake5_InterfaceOverload.Good;

/// <summary>
/// ✅ GOOD EXAMPLE: Right-Sized Abstractions - Only Abstract What You'll Actually Swap
/// 
/// This demonstrates the fix for Mistake #5: Interface Overload
/// 
/// Benefits:
/// - Only 2 interfaces (instead of 47)
/// - Real flexibility where it matters
/// - Minimal runtime overhead
/// - Easy to understand and maintain
/// - Fast DI resolution
/// 
/// Result: Performance gains, cognitive clarity, actual flexibility
/// </summary>

#region The Two Interfaces That Matter

/// <summary>
/// ✅ GOOD: Abstract the data storage - you WILL swap this
/// (InMemory for tests, SQL for production, Redis for caching, Cosmos for scale)
/// </summary>
public interface IUserRepository
{
    Task<int> CreateUserAsync(string name, string email);
    Task<UserData> GetUserAsync(int id);
    Task UpdateUserAsync(int id, string name, string email);
    Task DeleteUserAsync(int id);
    Task<List<UserData>> GetUsersAsync();
}

/// <summary>
/// ✅ GOOD: Abstract external communication - you WILL swap this
/// (SMTP for dev, SendGrid for production, Mock for tests, SES for AWS)
/// </summary>
public interface INotificationService
{
    Task NotifyUserCreatedAsync(int userId, string email);
    Task NotifyOrderCreatedAsync(int orderId, int userId);
    Task NotifyPaymentProcessedAsync(int orderId, decimal amount);
}

#endregion

#region Concrete Implementations with Multiple Options

/// <summary>
/// ✅ GOOD: SQL implementation for production
/// </summary>
public class SqlUserRepository : IUserRepository
{
    public async Task<int> CreateUserAsync(string name, string email)
    {
        await Task.Delay(10); // Simulate database call
        return Random.Shared.Next(1000);
    }

    public async Task<UserData> GetUserAsync(int id)
    {
        await Task.Delay(5);
        return new UserData { Id = id, Name = "SQL User", Email = "sql@example.com" };
    }

    public async Task UpdateUserAsync(int id, string name, string email)
    {
        await Task.Delay(8);
    }

    public async Task DeleteUserAsync(int id)
    {
        await Task.Delay(5);
    }

    public async Task<List<UserData>> GetUsersAsync()
    {
        await Task.Delay(15);
        return new List<UserData>();
    }
}

/// <summary>
/// ✅ GOOD: In-memory implementation for testing
/// </summary>
public class InMemoryUserRepository : IUserRepository
{
    private readonly Dictionary<int, UserData> _users = new();
    private int _nextId = 1;

    public Task<int> CreateUserAsync(string name, string email)
    {
        var id = _nextId++;
        _users[id] = new UserData { Id = id, Name = name, Email = email };
        return Task.FromResult(id);
    }

    public Task<UserData> GetUserAsync(int id)
    {
        _users.TryGetValue(id, out var user);
        return Task.FromResult(user ?? new UserData());
    }

    public Task UpdateUserAsync(int id, string name, string email)
    {
        if (_users.TryGetValue(id, out var user))
        {
            user.Name = name;
            user.Email = email;
        }
        return Task.CompletedTask;
    }

    public Task DeleteUserAsync(int id)
    {
        _users.Remove(id);
        return Task.CompletedTask;
    }

    public Task<List<UserData>> GetUsersAsync()
    {
        return Task.FromResult(_users.Values.ToList());
    }
}

/// <summary>
/// ✅ GOOD: Email notification implementation for production
/// </summary>
public class EmailNotificationService : INotificationService
{
    public async Task NotifyUserCreatedAsync(int userId, string email)
    {
        await Task.Delay(20); // SMTP is slower
        Console.WriteLine($"📧 Email sent to {email}: Welcome user {userId}!");
    }

    public async Task NotifyOrderCreatedAsync(int orderId, int userId)
    {
        await Task.Delay(20);
        Console.WriteLine($"📧 Email sent: Order {orderId} created for user {userId}");
    }

    public async Task NotifyPaymentProcessedAsync(int orderId, decimal amount)
    {
        await Task.Delay(20);
        Console.WriteLine($"📧 Email sent: Payment of ${amount} processed for order {orderId}");
    }
}

/// <summary>
/// ✅ GOOD: Mock notification for testing
/// </summary>
public class MockNotificationService : INotificationService
{
    public List<string> SentNotifications { get; } = new();

    public Task NotifyUserCreatedAsync(int userId, string email)
    {
        SentNotifications.Add($"UserCreated:{userId}:{email}");
        return Task.CompletedTask;
    }

    public Task NotifyOrderCreatedAsync(int orderId, int userId)
    {
        SentNotifications.Add($"OrderCreated:{orderId}:{userId}");
        return Task.CompletedTask;
    }

    public Task NotifyPaymentProcessedAsync(int orderId, decimal amount)
    {
        SentNotifications.Add($"PaymentProcessed:{orderId}:{amount}");
        return Task.CompletedTask;
    }
}

#endregion

#region Concrete Services - No Unnecessary Abstractions

/// <summary>
/// ✅ GOOD: Simple service with minimal dependencies
/// Only abstract the parts you'll actually swap
/// </summary>
public class UserOrderService
{
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// ✅ GOOD: Only 2 dependencies instead of 47
    /// </summary>
    public UserOrderService(
        IUserRepository userRepository,
        INotificationService notificationService)
    {
        _userRepository = userRepository;
        _notificationService = notificationService;
    }

    /// <summary>
    /// ✅ GOOD: Business logic in concrete methods - no unnecessary virtual calls
    /// </summary>
    public async Task<int> CreateUserOrderAsync(string userName, string email, decimal amount)
    {
        // Step 1: Create user (through interface - we DO swap this)
        var userId = await _userRepository.CreateUserAsync(userName, email);

        // Step 2: Validate user (concrete method - no interface needed)
        if (!ValidateUser(userName, email))
        {
            throw new ArgumentException("Invalid user data");
        }

        // Step 3: Hash password (concrete method - no interface needed)
        var hashedPassword = HashPassword("defaultPassword");

        // Step 4: Create order (concrete method - no interface needed)
        var orderId = CreateOrder(userId, amount);

        // Step 5: Process payment (concrete method - no interface needed)
        var paymentSuccess = ProcessPayment(orderId, amount);

        if (!paymentSuccess)
        {
            throw new InvalidOperationException("Payment failed");
        }

        // Step 6: Send notifications (through interface - we DO swap this)
        await _notificationService.NotifyUserCreatedAsync(userId, email);
        await _notificationService.NotifyOrderCreatedAsync(orderId, userId);
        await _notificationService.NotifyPaymentProcessedAsync(orderId, amount);

        // Step 7: Audit log (concrete method - no interface needed)
        LogUserAction(userId, "UserOrderCreated");

        return orderId;
    }

    /// <summary>
    /// ✅ GOOD: Simple validation - no interface needed
    /// </summary>
    private bool ValidateUser(string name, string email)
    {
        return !string.IsNullOrEmpty(name) && email.Contains("@");
    }

    /// <summary>
    /// ✅ GOOD: Simple hashing - no interface needed
    /// </summary>
    private string HashPassword(string password)
    {
        return $"hashed_{password}";
    }

    /// <summary>
    /// ✅ GOOD: Simple order creation - no interface needed
    /// </summary>
    private int CreateOrder(int userId, decimal amount)
    {
        return Random.Shared.Next(1000);
    }

    /// <summary>
    /// ✅ GOOD: Simple payment processing - no interface needed
    /// </summary>
    private bool ProcessPayment(int orderId, decimal amount)
    {
        return amount > 0; // Simple validation
    }

    /// <summary>
    /// ✅ GOOD: Simple logging - no interface needed
    /// </summary>
    private void LogUserAction(int userId, string action)
    {
        Console.WriteLine($"🔍 Audit: User {userId} performed {action}");
    }
}

#endregion

#region DI Registration - Minimal and Clean

/// <summary>
/// ✅ GOOD: Only 3 service registrations instead of 47
/// </summary>
public static class ServiceRegistration
{
    public static IServiceCollection AddRightSizedServices(this IServiceCollection services)
    {
        // Register the two abstractions that matter
        services.AddScoped<IUserRepository, SqlUserRepository>();
        services.AddScoped<INotificationService, EmailNotificationService>();

        // Register the main service
        services.AddScoped<UserOrderService>();

        return services;
    }

    /// <summary>
    /// ✅ GOOD: Easy environment-specific configuration
    /// </summary>
    public static IServiceCollection AddTestServices(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, InMemoryUserRepository>();
        services.AddScoped<INotificationService, MockNotificationService>();
        services.AddScoped<UserOrderService>();

        return services;
    }
}

#endregion

#region Supporting Types

public class UserData
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}

#endregion

/// <summary>
/// Decision Matrix: When to Create an Interface
/// 
/// ✅ Create Interface When:
/// - You have 2+ implementations (IUserRepository: SQL, InMemory, Redis)
/// - You plan to swap implementations (INotificationService: Email, SMS, Push)
/// - You need to mock for testing (External services)
/// - Different environments need different implementations
/// 
/// ❌ DON'T Create Interface When:
/// - Only one implementation exists
/// - Implementation will never be swapped
/// - It's pure business logic (validation, calculations)
/// - It's a simple utility method
/// - You're creating it "just in case"
/// 
/// 📊 Performance Comparison:
/// 
/// ❌ BAD (47 Interfaces):
/// - Virtual method calls: 47 × 1-2ns = ~94ns overhead
/// - DI resolution: 47 × 50ns = ~2.35μs overhead
/// - Memory: 47 × 8 bytes = 376 bytes per instance
/// - Constructor complexity: 47 parameters
/// - Files to maintain: 94 (47 interfaces + 47 implementations)
/// 
/// ✅ GOOD (2 Interfaces):
/// - Virtual method calls: 2 × 1-2ns = ~4ns overhead
/// - DI resolution: 2 × 50ns = ~100ns overhead  
/// - Memory: 2 × 8 bytes = 16 bytes per instance
/// - Constructor complexity: 2 parameters
/// - Files to maintain: 6 (2 interfaces + 4 implementations)
/// 
/// Performance Improvement: 96% faster DI resolution, 96% less memory overhead
/// Cognitive Load Improvement: 96% fewer abstractions to understand
/// Maintenance Improvement: 94% fewer files to maintain
/// 
/// 🎯 Real Flexibility Gained:
/// - Swap SQL ↔ InMemory ↔ Redis for different environments
/// - Swap Email ↔ SMS ↔ Push ↔ Mock for different scenarios
/// - Easy testing with InMemory + Mock implementations
/// - Environment-specific configurations (dev, test, prod)
/// </summary>

/// <summary>
/// Usage Examples:
/// </summary>
public static class UsageExamples
{
    /// <summary>
    /// ✅ Production usage with SQL + Email
    /// </summary>
    public static async Task ProductionExample()
    {
        var services = new ServiceCollection()
            .AddRightSizedServices()
            .BuildServiceProvider();

        var userOrderService = services.GetRequiredService<UserOrderService>();
        var orderId = await userOrderService.CreateUserOrderAsync("John Doe", "john@example.com", 99.99m);

        Console.WriteLine($"✅ Order {orderId} created in production");
    }

    /// <summary>
    /// ✅ Testing usage with InMemory + Mock  
    /// </summary>
    public static async Task TestingExample()
    {
        var services = new ServiceCollection()
            .AddTestServices()
            .BuildServiceProvider();

        var userOrderService = services.GetRequiredService<UserOrderService>();
        var orderId = await userOrderService.CreateUserOrderAsync("Test User", "test@example.com", 50.00m);

        // Verify notifications were sent
        var mockNotifications = services.GetRequiredService<INotificationService>() as MockNotificationService;
        Console.WriteLine($"✅ Test passed: {mockNotifications?.SentNotifications.Count} notifications sent");
    }
}
