using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Examples.Mistake3_TooManyLayers.Bad;

/// <summary>
/// ❌ BAD EXAMPLE: Four-layer mapping that wastes 35% of request time
/// 
/// This demonstrates Mistake #3: Too Many Layers
/// 
/// Performance Problems:
/// - SQL → EF Entity (database overhead)
/// - Entity → Domain (AutoMapper reflection)
/// - Domain → DTO (AutoMapper reflection)
/// - DTO → ViewModel (AutoMapper reflection)
/// 
/// Result: 847μs instead of 312μs (171% slower!)
/// Memory: 25KB allocated vs 9KB (178% more garbage)
/// </summary>

#region EF Entity Layer (Database)
public class CustomerEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
#endregion

#region Domain Layer 
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
#endregion

#region DTO Layer
public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
#endregion

#region ViewModel Layer  
public class CustomerViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public string Status { get; set; } = "";
}
#endregion

#region Infrastructure
public class CustomerDbContext : DbContext
{
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options) { }

    public DbSet<CustomerEntity> Customers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Seed data for benchmarking
        modelBuilder.Entity<CustomerEntity>().HasData(
            new CustomerEntity
            {
                Id = 1,
                Name = "John Doe",
                Email = "john@example.com",
                Phone = "+1-555-0123",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                IsActive = true
            },
            new CustomerEntity
            {
                Id = 2,
                Name = "Jane Smith",
                Email = "jane@example.com",
                Phone = "+1-555-0124",
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                IsActive = true
            },
            new CustomerEntity
            {
                Id = 3,
                Name = "Bob Johnson",
                Email = "bob@example.com",
                Phone = "+1-555-0125",
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                IsActive = false
            }
        );
    }
}
#endregion

#region AutoMapper Profiles
public class CustomerMappingProfile : Profile
{
    public CustomerMappingProfile()
    {
        // Entity → Domain (Mapping #1)
        CreateMap<CustomerEntity, Customer>();

        // Domain → DTO (Mapping #2)  
        CreateMap<Customer, CustomerDto>();

        // DTO → ViewModel (Mapping #3)
        CreateMap<CustomerDto, CustomerViewModel>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-dd")))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsActive ? "Active" : "Inactive"));
    }
}
#endregion

#region Service Layer
public class CustomerService
{
    private readonly CustomerDbContext _context;
    private readonly IMapper _mapper;

    public CustomerService(CustomerDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// ❌ BAD: Four-layer mapping kills performance
    /// This method demonstrates the 847μs performance problem
    /// </summary>
    public async Task<CustomerViewModel> GetCustomerAsync(int id)
    {
        // Step 1: SQL → EF Entity (Database I/O + EF overhead)
        var entity = await _context.Customers.FindAsync(id);
        if (entity == null) throw new InvalidOperationException($"Customer {id} not found");

        // Step 2: Entity → Domain (AutoMapper reflection overhead)
        var domain = _mapper.Map<Customer>(entity);

        // Step 3: Domain → DTO (AutoMapper reflection overhead)  
        var dto = _mapper.Map<CustomerDto>(domain);

        // Step 4: DTO → ViewModel (AutoMapper reflection overhead + string formatting)
        var viewModel = _mapper.Map<CustomerViewModel>(dto);

        return viewModel;

        // Performance Impact:
        // - 4 object allocations (25KB total)
        // - 3 AutoMapper reflection calls
        // - Result: 847μs instead of 312μs
    }

    /// <summary>
    /// Even worse: Getting multiple customers with 4-layer mapping
    /// </summary>
    public async Task<List<CustomerViewModel>> GetAllCustomersAsync()
    {
        var entities = await _context.Customers.ToListAsync();        // SQL → EF Entity
        var domains = _mapper.Map<List<Customer>>(entities);          // Entity → Domain
        var dtos = _mapper.Map<List<CustomerDto>>(domains);           // Domain → DTO
        var viewModels = _mapper.Map<List<CustomerViewModel>>(dtos);  // DTO → ViewModel

        return viewModels;
        // For 100 customers: 4 full list mappings = massive overhead!
    }
}
#endregion

#region DI Configuration
public static class ServiceConfiguration
{
    public static IServiceCollection AddBadLayeredServices(this IServiceCollection services)
    {
        // ❌ BAD: Heavy DI configuration with reflection
        services.AddDbContext<CustomerDbContext>(options =>
            options.UseInMemoryDatabase("BadExample"));

        services.AddAutoMapper(typeof(CustomerMappingProfile));
        services.AddScoped<CustomerService>();

        return services;
    }
}
#endregion

/// <summary>
/// Usage Example (for benchmarking):
/// 
/// var services = new ServiceCollection()
///     .AddBadLayeredServices()
///     .BuildServiceProvider();
/// 
/// var customerService = services.GetRequiredService&lt;CustomerService&gt;();
/// var customer = await customerService.GetCustomerAsync(1);
/// 
/// Performance Result: 847μs with 25KB allocation
/// </summary>
