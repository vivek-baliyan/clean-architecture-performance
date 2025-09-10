namespace Bad.Domain
{
    /// <summary>
    /// ❌ BAD EXAMPLE: Anemic domain model
    /// 
    /// Problems:
    /// 1. No behavior - just a data container
    /// 2. Business logic will leak into application services
    /// 3. Violates Tell Don't Ask principle
    /// 4. Hard to maintain invariants
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } // No validation, no encapsulation
        public string Name { get; set; }
        
        // No methods = no behavior = anemic model
        // This forces business logic into application services
    }
}
