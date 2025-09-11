using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Examples.Mistake4_CargoCult.Bad;

/// <summary>
/// ❌ BAD EXAMPLE: Cargo Cult Culture - Architecture Theatre
/// 
/// This demonstrates Mistake #4: Cargo Cult Culture
/// 
/// Problems:
/// - 30-minute planning sessions to send an email
/// - Architecture discussions become theatre
/// - Over-engineering "just in case"
/// - Ceremony without value
/// - Delivery paralysis
/// 
/// Result: Features take 30-40% longer to deliver
/// </summary>

#region Over-Engineered Email System (Result of 30-minute planning session)

/// <summary>
/// ❌ BAD: Abstract factory for email providers (will never be swapped)
/// </summary>
public interface IEmailProviderFactory
{
    IEmailProvider CreateEmailProvider(EmailProviderType providerType);
    IEmailProvider CreateEmailProvider(EmailProviderConfiguration configuration);
    Task<IEmailProvider> CreateEmailProviderAsync(string providerName);
}

/// <summary>
/// ❌ BAD: Strategy pattern for sending emails (only one implementation)
/// </summary>
public interface IEmailSendingStrategy
{
    Task<EmailSendResult> SendAsync(EmailMessage message, EmailSendOptions options);
    bool CanHandle(EmailProviderType providerType);
    EmailProviderCapabilities GetCapabilities();
}

/// <summary>
/// ❌ BAD: Repository pattern for email templates (stored in memory)
/// </summary>
public interface IEmailTemplateRepository
{
    Task<EmailTemplate> GetTemplateAsync(string templateId);
    Task<EmailTemplate> GetTemplateByNameAsync(string name, string culture);
    Task<IEnumerable<EmailTemplate>> GetTemplatesAsync(EmailTemplateFilter filter);
    Task SaveTemplateAsync(EmailTemplate template);
}

/// <summary>
/// ❌ BAD: Observer pattern for email events (no one subscribes)
/// </summary>
public interface IEmailEventPublisher
{
    Task PublishEmailSentAsync(EmailSentEvent emailEvent);
    Task PublishEmailFailedAsync(EmailFailedEvent emailEvent);
    void Subscribe<T>(IEmailEventHandler<T> handler) where T : IEmailEvent;
}

/// <summary>
/// ❌ BAD: Chain of responsibility for email validation (one validator)
/// </summary>
public interface IEmailValidationChain
{
    Task<EmailValidationResult> ValidateAsync(EmailMessage message);
    void AddValidator(IEmailValidator validator);
    void RemoveValidator(IEmailValidator validator);
}

/// <summary>
/// ❌ BAD: Decorator pattern for email metrics (metrics never used)
/// </summary>
public interface IEmailMetricsDecorator : IEmailService
{
    EmailMetrics GetMetrics();
    void ResetMetrics();
}

#endregion

#region Complex Configuration System

/// <summary>
/// ❌ BAD: Overcomplicated configuration for simple email
/// </summary>
public class EmailProviderConfiguration
{
    public EmailProviderType ProviderType { get; set; }
    public Dictionary<string, object> Settings { get; set; } = new();
    public EmailRetryPolicy RetryPolicy { get; set; } = new();
    public EmailRateLimitingOptions RateLimiting { get; set; } = new();
    public EmailCircuitBreakerOptions CircuitBreaker { get; set; } = new();
    public EmailBulkSendingOptions BulkSending { get; set; } = new();
}

public class EmailRetryPolicy
{
    public int MaxRetries { get; set; } = 3;
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromMinutes(5);
    public double BackoffMultiplier { get; set; } = 2.0;
    public List<Type> RetriableExceptions { get; set; } = new();
}

public class EmailRateLimitingOptions
{
    public int MaxEmailsPerMinute { get; set; } = 100;
    public int MaxEmailsPerHour { get; set; } = 1000;
    public int MaxEmailsPerDay { get; set; } = 10000;
    public bool EnablePerRecipientLimiting { get; set; }
}

#endregion

#region Excessive Value Objects

/// <summary>
/// ❌ BAD: Value objects for everything (even simple strings)
/// </summary>
public record EmailAddress(string Value)
{
    public static EmailAddress Create(string email) => new(email);
    public bool IsValid() => Value.Contains("@");
    public string Domain => Value.Split('@').LastOrDefault() ?? "";
    public string LocalPart => Value.Split('@').FirstOrDefault() ?? "";
}

public record EmailSubject(string Value)
{
    public static EmailSubject Create(string subject) => new(subject);
    public bool IsEmpty() => string.IsNullOrWhiteSpace(Value);
    public int Length => Value?.Length ?? 0;
}

public record EmailBody(string Content, EmailBodyType Type)
{
    public static EmailBody CreateText(string content) => new(content, EmailBodyType.Text);
    public static EmailBody CreateHtml(string content) => new(content, EmailBodyType.Html);
    public bool IsHtml => Type == EmailBodyType.Html;
}

#endregion

#region The "Architecture" Result

/// <summary>
/// ❌ BAD: The final "architected" email service
/// Result of 30-minute planning session to send a simple email
/// </summary>
public class EnterpriseEmailService : IEmailService
{
    private readonly IEmailProviderFactory _providerFactory;
    private readonly IEmailTemplateRepository _templateRepository;
    private readonly IEmailValidationChain _validationChain;
    private readonly IEmailEventPublisher _eventPublisher;
    private readonly IOptions<EmailProviderConfiguration> _configuration;

    public EnterpriseEmailService(
        IEmailProviderFactory providerFactory,
        IEmailTemplateRepository templateRepository,
        IEmailValidationChain validationChain,
        IEmailEventPublisher eventPublisher,
        IOptions<EmailProviderConfiguration> configuration)
    {
        _providerFactory = providerFactory;
        _templateRepository = templateRepository;
        _validationChain = validationChain;
        _eventPublisher = eventPublisher;
        _configuration = configuration;
    }

    /// <summary>
    /// ❌ BAD: 47 lines of code to send an email
    /// </summary>
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            // Step 1: Create value objects (unnecessary)
            var emailAddress = EmailAddress.Create(to);
            var emailSubject = EmailSubject.Create(subject);
            var emailBody = EmailBody.CreateText(body);

            // Step 2: Create message object (unnecessary wrapper)
            var message = new EmailMessage
            {
                To = emailAddress,
                Subject = emailSubject,
                Body = emailBody,
                Priority = EmailPriority.Normal,
                Category = EmailCategory.Transactional
            };

            // Step 3: Validate message (one simple validator)
            var validationResult = await _validationChain.ValidateAsync(message);
            if (!validationResult.IsValid)
            {
                throw new EmailValidationException(validationResult.Errors);
            }

            // Step 4: Get email provider (always the same one)
            var provider = await _providerFactory.CreateEmailProviderAsync("smtp");

            // Step 5: Send email (finally!)
            var result = await provider.SendAsync(message);

            // Step 6: Publish success event (no one listens)
            await _eventPublisher.PublishEmailSentAsync(new EmailSentEvent
            {
                MessageId = result.MessageId,
                Recipient = to,
                Subject = subject,
                SentAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            // Step 7: Publish failure event (no one listens)
            await _eventPublisher.PublishEmailFailedAsync(new EmailFailedEvent
            {
                Recipient = to,
                Subject = subject,
                Error = ex.Message,
                FailedAt = DateTime.UtcNow
            });
            throw;
        }
    }
}

#endregion

#region Supporting Interfaces & Enums

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}

public interface IEmailProvider
{
    Task<EmailSendResult> SendAsync(EmailMessage message);
}

public enum EmailProviderType { Smtp, SendGrid, Mailgun, AmazonSes }
public enum EmailBodyType { Text, Html }
public enum EmailPriority { Low, Normal, High }
public enum EmailCategory { Transactional, Marketing, System }

public class EmailMessage
{
    public EmailAddress To { get; set; } = null!;
    public EmailSubject Subject { get; set; } = null!;
    public EmailBody Body { get; set; } = null!;
    public EmailPriority Priority { get; set; }
    public EmailCategory Category { get; set; }
}

public class EmailSendResult
{
    public string MessageId { get; set; } = "";
    public bool IsSuccess { get; set; }
}

public class EmailValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class EmailTemplate { }
public class EmailTemplateFilter { }
public class EmailSendOptions { }
public class EmailProviderCapabilities { }
public class EmailMetrics { }
public class EmailCircuitBreakerOptions { }
public class EmailBulkSendingOptions { }

public interface IEmailEvent { }
public class EmailSentEvent : IEmailEvent
{
    public string MessageId { get; set; } = "";
    public string Recipient { get; set; } = "";
    public string Subject { get; set; } = "";
    public DateTime SentAt { get; set; }
}

public class EmailFailedEvent : IEmailEvent
{
    public string Recipient { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Error { get; set; } = "";
    public DateTime FailedAt { get; set; }
}

public interface IEmailEventHandler<T> where T : IEmailEvent { }
public interface IEmailValidator { }
public class EmailValidationException : Exception
{
    public EmailValidationException(List<string> errors) : base(string.Join(", ", errors)) { }
}

#endregion

#region DI Configuration Nightmare

/// <summary>
/// ❌ BAD: 20+ registrations for sending an email
/// </summary>
public static class EmailServiceRegistration
{
    public static IServiceCollection AddEnterpriseEmailServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register 20+ services to send a simple email
        services.Configure<EmailProviderConfiguration>(options => configuration.GetSection("Email").Bind(options));

        services.AddScoped<IEmailProviderFactory, EmailProviderFactory>();
        services.AddScoped<IEmailSendingStrategy, SmtpEmailSendingStrategy>();
        services.AddScoped<IEmailTemplateRepository, InMemoryEmailTemplateRepository>();
        services.AddScoped<IEmailEventPublisher, EmailEventPublisher>();
        services.AddScoped<IEmailValidationChain, EmailValidationChain>();
        services.AddScoped<IEmailMetricsDecorator, EmailMetricsDecorator>();
        services.AddScoped<IEmailService, EnterpriseEmailService>();

        // Register validators (only one actually exists)
        services.AddScoped<IEmailValidator, EmailAddressValidator>();

        // Register event handlers (none actually exist)
        services.AddScoped<IEmailEventHandler<EmailSentEvent>, EmailSentEventHandler>();
        services.AddScoped<IEmailEventHandler<EmailFailedEvent>, EmailFailedEventHandler>();

        return services;
    }
}

// Stub implementations to make it compile
public class EmailProviderFactory : IEmailProviderFactory
{
    public IEmailProvider CreateEmailProvider(EmailProviderType providerType) => new SmtpEmailProvider();
    public IEmailProvider CreateEmailProvider(EmailProviderConfiguration configuration) => new SmtpEmailProvider();
    public Task<IEmailProvider> CreateEmailProviderAsync(string providerName) => Task.FromResult<IEmailProvider>(new SmtpEmailProvider());
}

public class SmtpEmailSendingStrategy : IEmailSendingStrategy
{
    public Task<EmailSendResult> SendAsync(EmailMessage message, EmailSendOptions options) => throw new NotImplementedException();
    public bool CanHandle(EmailProviderType providerType) => true;
    public EmailProviderCapabilities GetCapabilities() => new();
}

public class InMemoryEmailTemplateRepository : IEmailTemplateRepository
{
    public Task<EmailTemplate> GetTemplateAsync(string templateId) => throw new NotImplementedException();
    public Task<EmailTemplate> GetTemplateByNameAsync(string name, string culture) => throw new NotImplementedException();
    public Task<IEnumerable<EmailTemplate>> GetTemplatesAsync(EmailTemplateFilter filter) => throw new NotImplementedException();
    public Task SaveTemplateAsync(EmailTemplate template) => throw new NotImplementedException();
}

public class EmailEventPublisher : IEmailEventPublisher
{
    public Task PublishEmailSentAsync(EmailSentEvent emailEvent) => Task.CompletedTask;
    public Task PublishEmailFailedAsync(EmailFailedEvent emailEvent) => Task.CompletedTask;
    public void Subscribe<T>(IEmailEventHandler<T> handler) where T : IEmailEvent { }
}

public class EmailValidationChain : IEmailValidationChain
{
    public Task<EmailValidationResult> ValidateAsync(EmailMessage message) => Task.FromResult(new EmailValidationResult { IsValid = true });
    public void AddValidator(IEmailValidator validator) { }
    public void RemoveValidator(IEmailValidator validator) { }
}

public class EmailMetricsDecorator : IEmailMetricsDecorator
{
    private readonly IEmailService _inner;
    public EmailMetricsDecorator(IEmailService inner) => _inner = inner;
    public Task SendEmailAsync(string to, string subject, string body) => _inner.SendEmailAsync(to, subject, body);
    public EmailMetrics GetMetrics() => new();
    public void ResetMetrics() { }
}

public class SmtpEmailProvider : IEmailProvider
{
    public Task<EmailSendResult> SendAsync(EmailMessage message) => Task.FromResult(new EmailSendResult { IsSuccess = true, MessageId = Guid.NewGuid().ToString() });
}

public class EmailAddressValidator : IEmailValidator { }
public class EmailSentEventHandler : IEmailEventHandler<EmailSentEvent> { }
public class EmailFailedEventHandler : IEmailEventHandler<EmailFailedEvent> { }

#endregion

/// <summary>
/// Problems with this approach:
/// 
/// 1. ❌ 30-minute planning session to send an email
/// 2. ❌ 47 lines of code for a 3-line operation
/// 3. ❌ 20+ DI registrations for simple functionality
/// 4. ❌ 15+ interfaces for one simple use case
/// 5. ❌ Patterns applied "just in case" (never needed)
/// 6. ❌ Architecture discussions become theatre
/// 7. ❌ Delivery paralysis - features take 30-40% longer
/// 8. ❌ Cognitive overhead for new team members
/// 9. ❌ Testing becomes complex (20+ mocks needed)
/// 10. ❌ No actual business value delivered
/// 
/// Time to Deliver:
/// - Planning: 30 minutes
/// - Implementation: 2 hours
/// - Testing: 1 hour (mocking 20+ dependencies)
/// - Total: 3.5 hours to send an email
/// </summary>
