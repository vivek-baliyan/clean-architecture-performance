namespace Mistake1.FolderIllusion.Good
{
    /// <summary>
    /// ✅ GOOD EXAMPLE: Rich domain model with behavior
    /// Fixes Mistake #1: The Folder Illusion
    /// </summary>
    public class User
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        public UserId Id { get; private set; }
        public EmailAddress Email { get; private set; }
        public string Name { get; private set; }

        public User(UserId id, EmailAddress email, string name)
        {
            Id = id;
            Email = email;
            Name = name;
        }

        /// <summary>
        /// Business logic in the domain - NOT in application services
        /// </summary>
        public void ChangeEmail(EmailAddress newEmail)
        {
            if (!newEmail.IsValid())
                throw new InvalidEmailException("Invalid email format");

            Email = newEmail;
            AddDomainEvent(new EmailChangedEvent(Id, newEmail));
        }

        private void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public IReadOnlyList<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();
    }

    /// <summary>
    /// Strong typing prevents primitive obsession
    /// </summary>
    public record UserId(int Value);

    /// <summary>
    /// Value object with validation logic
    /// </summary>
    public record EmailAddress(string Value)
    {
        public bool IsValid() => Value.Contains("@") && Value.Length > 5 && !Value.StartsWith("@") && !Value.EndsWith("@") && Value.Count(c => c == '@') == 1;
    }

    /// <summary>
    /// Domain event for integration with other bounded contexts
    /// </summary>
    public record EmailChangedEvent(UserId UserId, EmailAddress NewEmail) : IDomainEvent;

    public interface IDomainEvent { }

    public class InvalidEmailException : Exception
    {
        public InvalidEmailException(string message) : base(message) { }
    }
}
