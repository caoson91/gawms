using UniformWMS.Domain.Common;
using UniformWMS.Domain.Enums;

namespace UniformWMS.Domain.Entities;

// ─── Identity ────────────────────────────────────────────────────────────────

public class AppUser : BaseEntity
{
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public DateTime? LastLoginAt { get; set; }

    // Navigation
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

public class AppRole : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class Permission : BaseEntity
{
    public string Module { get; set; } = null!;     // e.g., "Uniform", "Issuance"
    public string Action { get; set; } = null!;     // e.g., "View", "Create", "Edit", "Delete"
    public string Code { get; set; } = null!;       // e.g., "Uniform.View"
    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class UserRole : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    public AppUser User { get; set; } = null!;
    public AppRole Role { get; set; } = null!;
}

public class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }

    public AppRole Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public string? RevokedReason { get; set; }
    public string? CreatedByIp { get; set; }
    public string? RevokedByIp { get; set; }

    public AppUser User { get; set; } = null!;
}

// ─── Master Data ─────────────────────────────────────────────────────────────

public class Category : BaseEntity
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<UniformItem> UniformItems { get; set; } = new List<UniformItem>();
}

public class Supplier : BaseEntity
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxCode { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}

public class Department : BaseEntity
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public Guid? ParentId { get; set; }
    public Department? Parent { get; set; }

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}

public class Employee : BaseEntity
{
    public string EmployeeCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Position { get; set; }
    public Guid DepartmentId { get; set; }
    public DateTime JoinDate { get; set; }
    public DateTime? ResignDate { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    public string? Notes { get; set; }

    public Department Department { get; set; } = null!;
    public ICollection<IssuanceOrderLine> IssuanceLines { get; set; } = new List<IssuanceOrderLine>();
    public ICollection<ReturnOrderLine> ReturnLines { get; set; } = new List<ReturnOrderLine>();
}

// ─── Inventory ────────────────────────────────────────────────────────────────

public class UniformItem : BaseEntity
{
    public string ItemCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public UniformSize Size { get; set; }
    public string Color { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public decimal? UnitPrice { get; set; }
    public int MinStockLevel { get; set; } = 0;
    public int CurrentStock { get; set; } = 0;
    public UniformStatus Status { get; set; } = UniformStatus.Active;

    public Category Category { get; set; } = null!;
    public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
    public ICollection<IssuanceOrderLine> IssuanceLines { get; set; } = new List<IssuanceOrderLine>();
    public ICollection<ReturnOrderLine> ReturnLines { get; set; } = new List<ReturnOrderLine>();
    public ICollection<PurchaseOrderLine> PurchaseLines { get; set; } = new List<PurchaseOrderLine>();
}

public class StockTransaction : BaseEntity
{
    public string TransactionCode { get; set; } = null!;
    public Guid ItemId { get; set; }
    public StockTransactionType Type { get; set; }
    public int Quantity { get; set; }
    public int StockBefore { get; set; }
    public int StockAfter { get; set; }
    public string? ReferenceCode { get; set; }   // Mã phiếu gốc
    public Guid? ReferenceId { get; set; }
    public string? Notes { get; set; }

    public UniformItem Item { get; set; } = null!;
}

// ─── Issuance (Cấp phát) ─────────────────────────────────────────────────────

public class IssuanceOrder : BaseEntity
{
    public string OrderCode { get; set; } = null!;
    public string? Description { get; set; }
    public IssuanceStatus Status { get; set; } = IssuanceStatus.Draft;
    public DateTime? IssuedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Notes { get; set; }

    public ICollection<IssuanceOrderLine> Lines { get; set; } = new List<IssuanceOrderLine>();
}

public class IssuanceOrderLine : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid ItemId { get; set; }
    public int Quantity { get; set; }
    public int ActualQuantity { get; set; }  // Số lượng thực cấp
    public string? Notes { get; set; }

    public IssuanceOrder Order { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
    public UniformItem Item { get; set; } = null!;
}

// ─── Return (Thu hồi) ────────────────────────────────────────────────────────

public class ReturnOrder : BaseEntity
{
    public string OrderCode { get; set; } = null!;
    public string? Description { get; set; }
    public ReturnStatus Status { get; set; } = ReturnStatus.Draft;
    public DateTime? ReturnedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Notes { get; set; }

    public ICollection<ReturnOrderLine> Lines { get; set; } = new List<ReturnOrderLine>();
}

public class ReturnOrderLine : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid ItemId { get; set; }
    public int Quantity { get; set; }
    public ItemCondition Condition { get; set; }
    public string? Notes { get; set; }

    public ReturnOrder Order { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
    public UniformItem Item { get; set; } = null!;
}

// ─── Purchase (Nhập kho) ──────────────────────────────────────────────────────

public class PurchaseOrder : BaseEntity
{
    public string OrderCode { get; set; } = null!;
    public Guid SupplierId { get; set; }
    public string? Description { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }

    public Supplier Supplier { get; set; } = null!;
    public ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();
}

public class PurchaseOrderLine : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid ItemId { get; set; }
    public int OrderedQuantity { get; set; }
    public int ReceivedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => UnitPrice * OrderedQuantity;

    public PurchaseOrder Order { get; set; } = null!;
    public UniformItem Item { get; set; } = null!;
}
