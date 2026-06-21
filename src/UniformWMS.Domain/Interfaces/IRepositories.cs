using System.Linq.Expressions;
using UniformWMS.Domain.Common;
using UniformWMS.Domain.Entities;

namespace UniformWMS.Domain.Interfaces;

// ─── Generic Repository ───────────────────────────────────────────────────────

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
    void SoftDelete(T entity);
    IQueryable<T> Query();
}

// ─── Specific Repositories ────────────────────────────────────────────────────

public interface IUserRepository : IRepository<AppUser>
{
    Task<AppUser?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<AppUser?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<AppUser?> GetWithRolesAsync(Guid id, CancellationToken ct = default);
}

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetActiveTokenAsync(string token, CancellationToken ct = default);
    Task RevokeAllForUserAsync(Guid userId, string reason, CancellationToken ct = default);
}

public interface IUniformItemRepository : IRepository<UniformItem>
{
    Task<IEnumerable<UniformItem>> GetLowStockItemsAsync(CancellationToken ct = default);
    Task<bool> IsItemCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default);
}

public interface IStockTransactionRepository : IRepository<StockTransaction>
{
    Task<IEnumerable<StockTransaction>> GetByItemAsync(Guid itemId, CancellationToken ct = default);
}

public interface IIssuanceOrderRepository : IRepository<IssuanceOrder>
{
    Task<IssuanceOrder?> GetWithLinesAsync(Guid id, CancellationToken ct = default);
}

public interface IReturnOrderRepository : IRepository<ReturnOrder>
{
    Task<ReturnOrder?> GetWithLinesAsync(Guid id, CancellationToken ct = default);
}

public interface IPurchaseOrderRepository : IRepository<PurchaseOrder>
{
    Task<PurchaseOrder?> GetWithLinesAsync(Guid id, CancellationToken ct = default);
}

// ─── Unit of Work ─────────────────────────────────────────────────────────────

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IRepository<AppRole> Roles { get; }
    IRepository<Permission> Permissions { get; }
    IRepository<UserRole> UserRoles { get; }
    IRepository<RolePermission> RolePermissions { get; }
    IRepository<Category> Categories { get; }
    IRepository<Supplier> Suppliers { get; }
    IRepository<Department> Departments { get; }
    IRepository<Employee> Employees { get; }
    IUniformItemRepository UniformItems { get; }
    IStockTransactionRepository StockTransactions { get; }
    IIssuanceOrderRepository IssuanceOrders { get; }
    IRepository<IssuanceOrderLine> IssuanceOrderLines { get; }
    IReturnOrderRepository ReturnOrders { get; }
    IRepository<ReturnOrderLine> ReturnOrderLines { get; }
    IPurchaseOrderRepository PurchaseOrders { get; }
    IRepository<PurchaseOrderLine> PurchaseOrderLines { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
