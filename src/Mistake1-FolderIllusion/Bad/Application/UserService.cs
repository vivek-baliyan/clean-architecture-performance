using System.Threading.Tasks;

namespace Bad.Application
{
    /// <summary>
    /// ❌ BAD EXAMPLE: Application service doing domain logic
    /// 
    /// Problems:
    /// 1. Business logic in application layer (wrong place)
    /// 2. Depends on Infrastructure interface (wrong direction)
    /// 3. No validation or business rules
    /// 4. Violates Single Responsibility Principle
    /// </summary>
    public class UserService
    {
        private readonly IUserRepository _repo; // Points to Infrastructure - WRONG!
        
        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }
        
        public async Task UpdateEmail(int id, string email)
        {
            var user = await _repo.GetByIdAsync(id);
            user.Email = email; // No validation! Business logic in wrong layer!
            await _repo.SaveAsync(user);
            
            // What about:
            // - Email format validation?
            // - Business rules?
            // - Domain events?
            // - Invariants?
            // All lost because of anemic domain model!
        }
    }
}
