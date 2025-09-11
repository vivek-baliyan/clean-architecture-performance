namespace CleanArchitecture.Examples.Mistake1_FolderIllusion.Bad;

/// <summary>
/// ❌ WRONG LOCATION: Interface in Infrastructure layer
/// 
/// Problems:
/// 1. Creates circular dependency (Domain → Infrastructure)
/// 2. Violates Dependency Inversion Principle
/// 3. Makes unit testing harder
/// 4. Couples domain to infrastructure concerns
/// 
/// This is the classic "Folder Illusion" - 
/// Clean folders don't guarantee clean architecture!
/// </summary>
public interface IUserRepository
{
    Task<User> GetByIdAsync(int id);
    Task SaveAsync(User user);
}
