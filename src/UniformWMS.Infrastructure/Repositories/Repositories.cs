using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using UniformWMS.Domain.Common;
using UniformWMS.Domain.Entities;
using UniformWMS.Domain.Interfaces;
using UniformWMS.Infrastructure.Data;

namespace UniformWMS.Infrastructure.Repositories;

// ─── Generic Repository ───────────────────────────────────────────────────────

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _ctx;
    protected readonly DbSet<T> _set;

    public Repository(AppDbContext ctx)
    {
        _ctx = ctx;
        _set = ctx.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _set.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        => await _set.ToListAsync(ct);

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _set.Where(predicate).ToListAsync(ct);

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _set.FirstOrDefaultAsync(predicate, ct);

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _set.AnyAsync(predicate, ct);

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
        => predicate == null ? await _set.CountAsync(ct) : await _set.CountAsync(predicate, ct);

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await _set.AddAsync(entity, ct);

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
        => await _set.AddRangeAsync(entities, ct);

    public void Update(T entity) => _set.Update(entity);

    public void Remove(T entity) => _set.Remove(entity);

    public void SoftDelete(T entity)
    {
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        _set.Update(entity);
    }

    public IQueryable<T> Query() => _set;
}

// ─── Specific Repositories ────────────────────────────────────────────────────

public class UserRepository : Repository<AppUser>, IUserRepository
{
    public UserRepository(AppDbContext ctx) : base(ctx) { }

    public async Task<AppUser?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => await _set.FirstOrDefaultAsync(u => u.Username == username, ct);

    public async Task<AppUser?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _set.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<AppUser?> GetWithRolesAsync(Guid id, CancellationToken ct = default)
        => await _set
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
}

public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext ctx) : base(ctx) { }

    public async Task<RefreshToken?> GetActiveTokenAsync(string token, CancellationToken ct = default)
        => await _set.FirstOrDefaultAsync(t =>
            t.Token == token && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow, ct);

    public async Task RevokeAllForUserAsync(Guid userId, string reason, CancellationToken ct = default)
    {
        var tokens = await _set
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync(ct);
        foreach (var t in tokens)
        {
            t.IsRevoked = true;
            t.RevokedReason = reason;
        }
    }
}

public class UniformItemRepository : Repository<UniformItem>, IUniformItemRepository
{
    public UniformItemRepository(AppDbContext ctx) : base(ctx) { }

    public async Task<IEnumerable<UniformItem>> GetLowStockItemsAsync(CancellationToken ct = default)
        => await _set
            .Include(i => i.Category)
            .Where(i => i.CurrentStock <= i.MinStockLevel)
            .ToListAsync(ct);

    public async Task<bool> IsItemCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default)
        => await _set.AnyAsync(i =>
            i.ItemCode == code && (excludeId == null || i.Id != excludeId), ct);
}

public class StockTransactionRepository : Repository<StockTransaction>, IStockTransactionRepository
{
    public StockTransactionRepository(AppDbContext ctx) : base(ctx) { }

    public async Task<IEnumerable<StockTransaction>> GetByItemAsync(Guid itemId, CancellationToken ct = default)
        => await _set.Where(t => t.ItemId == itemId).OrderByDescending(t => t.CreatedAt).ToListAsync(ct);
}

public class IssuanceOrderRepository : Repository<IssuanceOrder>, IIssuanceOrderRepository
{
    public IssuanceOrderRepository(AppDbContext ctx) : base(ctx) { }

    public async Task<IssuanceOrder?> GetWithLinesAsync(Guid id, CancellationToken ct = default)
        => await _set
            .Include(o => o.Lines).ThenInclude(l => l.Employee)
            .Include(o => o.Lines).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
}

public class ReturnOrderRepository : Repository<ReturnOrder>, IReturnOrderRepository
{
    public ReturnOrderRepository(AppDbContext ctx) : base(ctx) { }

    public async Task<ReturnOrder?> GetWithLinesAsync(Guid id, CancellationToken ct = default)
        => await _set
            .Include(o => o.Lines).ThenInclude(l => l.Employee)
            .Include(o => o.Lines).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
}

public class PurchaseOrderRepository : Repository<PurchaseOrder>, IIpurchaseOrderRepository
{
    public PurchaseOrderRepository(AppDbContext ctx) : base(ctx) { }

    public async Task<PurchaseOrder?> GetWithLinesAsync(Guid id, CancellationToken ct = default)
        => await _set
            .Include(o => o.Lines).ThenInclude(l => l.Item)
            .Include(o => o.Supplier)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
}

// Fix the interface name in IRepositories.cs is IPurchaseOrderRepository
public interface IIpurchaseOrderRepository : IPurchaseOrderRepository { }

// ─── Unit of Work ─────────────────────────────────────────────────────────────

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _ctx;
    private IDbContextTransaction? _transaction;

    private IUserRepository? _users;
    private IRefreshTokenRepository? _refreshTokens;
    private IRepository<AppRole>? _roles;
    private IRepository<Permission>? _permissions;
    private IRepository<UserRole>? _userRoles;
    private IRepository<RolePermission>? _rolePermissions;
    private IRepository<Category>? _categories;
    private IRepository<Supplier>? _suppliers;
    private IRepository<Department>? _departments;
    private IRepository<Employee>? _employees;
    private IUniformItemRepository? _uniformItems;
    private IStockTransactionRepository? _stockTransactions;
    private IIssuanceOrderRepository? _issuanceOrders;
    private IRepository<IssuanceOrderLine>? _issuanceOrderLines;
    private IReturnOrderRepository? _returnOrders;
    private IRepository<ReturnOrderLine>? _returnOrderLines;
    private IPurchaseOrderRepository? _purchaseOrders;
    private IRepository<PurchaseOrderLine>? _purchaseOrderLines;

    public UnitOfWork(AppDbContext ctx) => _ctx = ctx;

    public IUserRepository Users => _users ??= new UserRepository(_ctx);
    public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new RefreshTokenRepository(_ctx);
    public IRepository<AppRole> Roles => _roles ??= new Repository<AppRole>(_ctx);
    public IRepository<Permission> Permissions => _permissions ??= new Repository<Permission>(_ctx);
    public IRepository<UserRole> UserRoles => _userRoles ??= new Repository<UserRole>(_ctx);
    public IRepository<RolePermission> RolePermissions => _rolePermissions ??= new Repository<RolePermission>(_ctx);
    public IRepository<Category> Categories => _categories ??= new Repository<Category>(_ctx);
    public IRepository<Supplier> Suppliers => _suppliers ??= new Repository<Supplier>(_ctx);
    public IRepository<Department> Departments => _departments ??= new Repository<Department>(_ctx);
    public IRepository<Employee> Employees => _employees ??= new Repository<Employee>(_ctx);
    public IUniformItemRepository UniformItems => _uniformItems ??= new UniformItemRepository(_ctx);
    public IStockTransactionRepository StockTransactions => _stockTransactions ??= new StockTransactionRepository(_ctx);
    public IIssuanceOrderRepository IssuanceOrders => _issuanceOrders ??= new IssuanceOrderRepository(_ctx);
    public IRepository<IssuanceOrderLine> IssuanceOrderLines => _issuanceOrderLines ??= new Repository<IssuanceOrderLine>(_ctx);
    public IReturnOrderRepository ReturnOrders => _returnOrders ??= new ReturnOrderRepository(_ctx);
    public IRepository<ReturnOrderLine> ReturnOrderLines => _returnOrderLines ??= new Repository<ReturnOrderLine>(_ctx);
    public IPurchaseOrderRepository PurchaseOrders => _purchaseOrders ??= new PurchaseOrderRepository(_ctx);
    public IRepository<PurchaseOrderLine> PurchaseOrderLines => _purchaseOrderLines ??= new Repository<PurchaseOrderLine>(_ctx);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _ctx.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => _transaction = await _ctx.Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _ctx.Dispose();
    }
}
