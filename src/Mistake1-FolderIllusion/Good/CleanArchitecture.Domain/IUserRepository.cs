using System.Threading.Tasks;

namespace CleanArchitecture.Domain.Users
{
    /// <summary>
    /// ✅ CORRECT LOCATION: Interface in Domain layer
    /// 
    /// Key principle: Put interfaces where they're CONSUMED, not where they're IMPLEMENTED
    /// 
    /// The Domain layer defines what it needs from Infrastructure,
    /// but doesn't depend on Infrastructure implementations.
    /// 
    /// This is the Dependency Inversion Principle in action.
    /// </summary>
    public interface IUserRepository 
    {
        Task<User> GetByIdAsync(UserId id);
        Task SaveAsync(User user);
    }
}
