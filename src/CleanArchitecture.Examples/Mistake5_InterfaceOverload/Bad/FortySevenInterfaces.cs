using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Examples.Mistake5_InterfaceOverload.Bad;

/// <summary>
/// ❌ BAD EXAMPLE: 47 Interfaces, 47 Single Implementations
/// 
/// This demonstrates Mistake #5: Interface Overload
/// 
/// Problems:
/// - Interface for every single method
/// - Runtime overhead (virtual calls)
/// - Cognitive overhead (mental mapping)
/// - DI container overhead
/// - No actual flexibility gained
/// 
/// Result: Indirection everywhere, flexibility nowhere
/// </summary>

#region User Management - 12 Interfaces for Simple CRUD

/// <summary>
/// ❌ BAD: Interface for user creation (will never have another implementation)
/// </summary>
public interface IUserCreator
{
    Task<int> CreateUserAsync(string name, string email);
}

public class UserCreator : IUserCreator
{
    public async Task<int> CreateUserAsync(string name, string email)
    {
        // Only implementation, will never be swapped
        await Task.Delay(10); // Simulate database
        return Random.Shared.Next(1000);
    }
}

/// <summary>
/// ❌ BAD: Interface for user updating (will never have another implementation)
/// </summary>
public interface IUserUpdater
{
    Task UpdateUserAsync(int id, string name, string email);
}

public class UserUpdater : IUserUpdater
{
    public async Task UpdateUserAsync(int id, string name, string email)
    {
        await Task.Delay(10); // Only implementation
    }
}

/// <summary>
/// ❌ BAD: Interface for user deletion (will never have another implementation)
/// </summary>
public interface IUserDeleter
{
    Task DeleteUserAsync(int id);
}

public class UserDeleter : IUserDeleter
{
    public async Task DeleteUserAsync(int id)
    {
        await Task.Delay(10); // Only implementation
    }
}

/// <summary>
/// ❌ BAD: Interface for user retrieval (will never have another implementation)
/// </summary>
public interface IUserRetriever
{
    Task<UserData> GetUserAsync(int id);
}

public class UserRetriever : IUserRetriever
{
    public async Task<UserData> GetUserAsync(int id)
    {
        await Task.Delay(10);
        return new UserData { Id = id, Name = "User", Email = "user@example.com" };
    }
}

/// <summary>
/// ❌ BAD: Interface for email updating (will never have another implementation)
/// </summary>
public interface IUserEmailUpdater
{
    Task UpdateEmailAsync(int userId, string newEmail);
}

public class UserEmailUpdater : IUserEmailUpdater
{
    public async Task UpdateEmailAsync(int userId, string newEmail)
    {
        await Task.Delay(10); // Only implementation
    }
}

/// <summary>
/// ❌ BAD: Interface for password hashing (will never have another implementation)
/// </summary>
public interface IUserPasswordHasher
{
    string HashPassword(string password);
}

public class UserPasswordHasher : IUserPasswordHasher
{
    public string HashPassword(string password)
    {
        return $"hashed_{password}"; // Only implementation
    }
}

/// <summary>
/// ❌ BAD: Interface for user validation (will never have another implementation)
/// </summary>
public interface IUserValidator
{
    bool ValidateUser(UserData user);
}

public class UserValidator : IUserValidator
{
    public bool ValidateUser(UserData user)
    {
        return !string.IsNullOrEmpty(user.Name); // Only implementation
    }
}

/// <summary>
/// ❌ BAD: Interface for user notification (will never have another implementation)
/// </summary>
public interface IUserNotifier
{
    Task NotifyUserCreatedAsync(int userId);
}

public class UserNotifier : IUserNotifier
{
    public async Task NotifyUserCreatedAsync(int userId)
    {
        await Task.Delay(5); // Only implementation
    }
}

/// <summary>
/// ❌ BAD: Interface for user auditing (will never have another implementation)
/// </summary>
public interface IUserAuditor
{
    Task LogUserActionAsync(int userId, string action);
}

public class UserAuditor : IUserAuditor
{
    public async Task LogUserActionAsync(int userId, string action)
    {
        await Task.Delay(5); // Only implementation
    }
}

#endregion

#region Order Management - 15 More Interfaces

public interface IOrderCreator
{
    Task<int> CreateOrderAsync(int userId, decimal amount);
}

public class OrderCreator : IOrderCreator
{
    public async Task<int> CreateOrderAsync(int userId, decimal amount)
    {
        await Task.Delay(10);
        return Random.Shared.Next(1000);
    }
}

public interface IOrderProcessor
{
    Task ProcessOrderAsync(int orderId);
}

public class OrderProcessor : IOrderProcessor
{
    public async Task ProcessOrderAsync(int orderId)
    {
        await Task.Delay(15); // Only implementation
    }
}

public interface IOrderValidator
{
    bool ValidateOrder(OrderData order);
}

public class OrderValidator : IOrderValidator
{
    public bool ValidateOrder(OrderData order)
    {
        return order.Amount > 0; // Only implementation
    }
}

public interface IOrderNotifier
{
    Task NotifyOrderCreatedAsync(int orderId);
}

public class OrderNotifier : IOrderNotifier
{
    public async Task NotifyOrderCreatedAsync(int orderId)
    {
        await Task.Delay(5); // Only implementation
    }
}

public interface IOrderAuditor
{
    Task LogOrderActionAsync(int orderId, string action);
}

public class OrderAuditor : IOrderAuditor
{
    public async Task LogOrderActionAsync(int orderId, string action)
    {
        await Task.Delay(5); // Only implementation
    }
}

// ... 10 more similar interfaces for orders

#endregion

#region Payment Management - 10 More Interfaces

public interface IPaymentProcessor
{
    Task<bool> ProcessPaymentAsync(int orderId, decimal amount);
}

public class PaymentProcessor : IPaymentProcessor
{
    public async Task<bool> ProcessPaymentAsync(int orderId, decimal amount)
    {
        await Task.Delay(20);
        return true; // Only implementation
    }
}

public interface IPaymentValidator
{
    bool ValidatePayment(decimal amount);
}

public class PaymentValidator : IPaymentValidator
{
    public bool ValidatePayment(decimal amount)
    {
        return amount > 0; // Only implementation
    }
}

public interface IPaymentNotifier
{
    Task NotifyPaymentProcessedAsync(int orderId);
}

public class PaymentNotifier : IPaymentNotifier
{
    public async Task NotifyPaymentProcessedAsync(int orderId)
    {
        await Task.Delay(5); // Only implementation
    }
}

// ... 7 more payment interfaces

#endregion

#region The Resulting Service - Dependency Injection Hell

/// <summary>
/// ❌ BAD: Service that requires 47 dependencies
/// Each interface has exactly one implementation
/// </summary>
public class UserOrderService
{
    // User dependencies (12 interfaces)
    private readonly IUserCreator _userCreator;
    private readonly IUserUpdater _userUpdater;
    private readonly IUserDeleter _userDeleter;
    private readonly IUserRetriever _userRetriever;
    private readonly IUserEmailUpdater _userEmailUpdater;
    private readonly IUserPasswordHasher _userPasswordHasher;
    private readonly IUserValidator _userValidator;
    private readonly IUserNotifier _userNotifier;
    private readonly IUserAuditor _userAuditor;

    // Order dependencies (15 interfaces)
    private readonly IOrderCreator _orderCreator;
    private readonly IOrderProcessor _orderProcessor;
    private readonly IOrderValidator _orderValidator;
    private readonly IOrderNotifier _orderNotifier;
    private readonly IOrderAuditor _orderAuditor;

    // Payment dependencies (10 interfaces)
    private readonly IPaymentProcessor _paymentProcessor;
    private readonly IPaymentValidator _paymentValidator;
    private readonly IPaymentNotifier _paymentNotifier;

    // ... 20 more interfaces not shown for brevity

    /// <summary>
    /// ❌ BAD: Constructor with 47 dependencies
    /// </summary>
    public UserOrderService(
        IUserCreator userCreator,
        IUserUpdater userUpdater,
        IUserDeleter userDeleter,
        IUserRetriever userRetriever,
        IUserEmailUpdater userEmailUpdater,
        IUserPasswordHasher userPasswordHasher,
        IUserValidator userValidator,
        IUserNotifier userNotifier,
        IUserAuditor userAuditor,
        IOrderCreator orderCreator,
        IOrderProcessor orderProcessor,
        IOrderValidator orderValidator,
        IOrderNotifier orderNotifier,
        IOrderAuditor orderAuditor,
        IPaymentProcessor paymentProcessor,
        IPaymentValidator paymentValidator,
        IPaymentNotifier paymentNotifier
    // ... 30 more parameters
    )
    {
        _userCreator = userCreator;
        _userUpdater = userUpdater;
        _userDeleter = userDeleter;
        _userRetriever = userRetriever;
        _userEmailUpdater = userEmailUpdater;
        _userPasswordHasher = userPasswordHasher;
        _userValidator = userValidator;
        _userNotifier = userNotifier;
        _userAuditor = userAuditor;
        _orderCreator = orderCreator;
        _orderProcessor = orderProcessor;
        _orderValidator = orderValidator;
        _orderNotifier = orderNotifier;
        _orderAuditor = orderAuditor;
        _paymentProcessor = paymentProcessor;
        _paymentValidator = paymentValidator;
        _paymentNotifier = paymentNotifier;
        // ... 30 more assignments
    }

    /// <summary>
    /// ❌ BAD: Simple operation requiring multiple interface calls
    /// </summary>
    public async Task CreateUserOrderAsync(string userName, string email, decimal amount)
    {
        // Virtual method calls everywhere - performance overhead
        var userId = await _userCreator.CreateUserAsync(userName, email);
        await _userNotifier.NotifyUserCreatedAsync(userId);
        await _userAuditor.LogUserActionAsync(userId, "Created");

        var orderId = await _orderCreator.CreateOrderAsync(userId, amount);
        await _orderNotifier.NotifyOrderCreatedAsync(orderId);
        await _orderAuditor.LogOrderActionAsync(orderId, "Created");

        await _paymentProcessor.ProcessPaymentAsync(orderId, amount);
        await _paymentNotifier.NotifyPaymentProcessedAsync(orderId);

        // 8 virtual method calls for a simple operation
        // Each call has runtime overhead
        // No actual flexibility - all implementations are fixed
    }
}

#endregion

#region DI Registration Hell

/// <summary>
/// ❌ BAD: 47 service registrations for single implementations
/// </summary>
public static class ServiceRegistration
{
    public static IServiceCollection AddOverEngineeredServices(this IServiceCollection services)
    {
        // User services (12 registrations)
        services.AddScoped<IUserCreator, UserCreator>();
        services.AddScoped<IUserUpdater, UserUpdater>();
        services.AddScoped<IUserDeleter, UserDeleter>();
        services.AddScoped<IUserRetriever, UserRetriever>();
        services.AddScoped<IUserEmailUpdater, UserEmailUpdater>();
        services.AddScoped<IUserPasswordHasher, UserPasswordHasher>();
        services.AddScoped<IUserValidator, UserValidator>();
        services.AddScoped<IUserNotifier, UserNotifier>();
        services.AddScoped<IUserAuditor, UserAuditor>();

        // Order services (15 registrations)
        services.AddScoped<IOrderCreator, OrderCreator>();
        services.AddScoped<IOrderProcessor, OrderProcessor>();
        services.AddScoped<IOrderValidator, OrderValidator>();
        services.AddScoped<IOrderNotifier, OrderNotifier>();
        services.AddScoped<IOrderAuditor, OrderAuditor>();

        // Payment services (10 registrations)
        services.AddScoped<IPaymentProcessor, PaymentProcessor>();
        services.AddScoped<IPaymentValidator, PaymentValidator>();
        services.AddScoped<IPaymentNotifier, PaymentNotifier>();

        // ... 20 more registrations

        // The main service
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

public class OrderData
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
}

#endregion

/// <summary>
/// Problems with this approach:
/// 
/// 1. ❌ 47 interfaces with exactly 1 implementation each
/// 2. ❌ No actual flexibility - implementations never swapped
/// 3. ❌ Runtime overhead - 47 virtual method calls
/// 4. ❌ Cognitive overhead - mental mapping of 47 abstractions
/// 5. ❌ DI container overhead - 47 service resolutions
/// 6. ❌ Testing complexity - 47 mocks needed
/// 7. ❌ Constructor bloat - 47-parameter constructor
/// 8. ❌ Maintenance burden - 94 files (47 interfaces + 47 implementations)
/// 9. ❌ No business value - ceremony without purpose
/// 10. ❌ Misleading abstractions - suggest flexibility that doesn't exist
/// 
/// Performance Impact:
/// - Virtual method call overhead: ~1-2ns per call
/// - DI container resolution: ~50-100ns per dependency
/// - Memory overhead: ~8-16 bytes per interface reference
/// - For this example: ~47 virtual calls = ~94ns overhead per operation
/// 
/// This adds up in high-throughput scenarios!
/// </summary>
