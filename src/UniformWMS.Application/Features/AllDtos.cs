using UniformWMS.Domain.Enums;

// ─── Users ────────────────────────────────────────────────────────────────────
namespace UniformWMS.Application.Features.Users.DTOs
{

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public UserStatus Status { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public record CreateUserRequest(
    string Username, string Password, string FullName, string Email,
    string? PhoneNumber, List<Guid> RoleIds);

public record UpdateUserRequest(
    string FullName, string Email, string? PhoneNumber, UserStatus Status, List<Guid> RoleIds);
}

// ─── Roles ────────────────────────────────────────────────────────────────────
namespace UniformWMS.Application.Features.Roles.DTOs
{

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public List<string> Permissions { get; set; } = new();
}


public record CreateRoleRequest(string Name, string? Description, List<Guid> PermissionIds);

public record UpdateRoleRequest(string Name, string? Description, List<Guid> PermissionIds);

public class PermissionDto
{
    public Guid Id { get; set; }
    public string Module { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
}
}

// ─── Categories ───────────────────────────────────────────────────────────────
namespace UniformWMS.Application.Features.Categories.DTOs
{

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public record CreateCategoryRequest(string Code, string Name, string? Description, int DisplayOrder);
public record UpdateCategoryRequest(string Name, string? Description, int DisplayOrder, bool IsActive);
}

// ─── Suppliers ────────────────────────────────────────────────────────────────
namespace UniformWMS.Application.Features.Suppliers.DTOs
{

public class SupplierDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxCode { get; set; }
    public bool IsActive { get; set; }
}

public record CreateSupplierRequest(string Code, string Name, string? ContactPerson,
    string? Phone, string? Email, string? Address, string? TaxCode);
public record UpdateSupplierRequest(string Name, string? ContactPerson,
    string? Phone, string? Email, string? Address, string? TaxCode, bool IsActive);
}

// ─── Employees ────────────────────────────────────────────────────────────────
namespace UniformWMS.Application.Features.Employees.DTOs
{

public class EmployeeDto
{
    public Guid Id { get; set; }
    public string EmployeeCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Position { get; set; }
    public Guid DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public DateTime JoinDate { get; set; }
    public EmployeeStatus Status { get; set; }
}

public record CreateEmployeeRequest(string EmployeeCode, string FullName, string? Email,
    string? Phone, string? Position, Guid DepartmentId, DateTime JoinDate);
public record UpdateEmployeeRequest(string FullName, string? Email, string? Phone,
    string? Position, Guid DepartmentId, EmployeeStatus Status);
}

// ─── UniformItems ─────────────────────────────────────────────────────────────
namespace UniformWMS.Application.Features.UniformItems.DTOs
{

public class UniformItemDto
{
    public Guid Id { get; set; }
    public string ItemCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public UniformSize Size { get; set; }
    public string SizeName { get; set; } = null!;
    public string Color { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public decimal? UnitPrice { get; set; }
    public int MinStockLevel { get; set; }
    public int CurrentStock { get; set; }
    public bool IsLowStock => CurrentStock <= MinStockLevel;
    public UniformStatus Status { get; set; }
}

public record CreateUniformItemRequest(string ItemCode, string Name, string? Description,
    Guid CategoryId, UniformSize Size, string Color, decimal? UnitPrice, int MinStockLevel);
public record UpdateUniformItemRequest(string Name, string? Description, Guid CategoryId,
    UniformSize Size, string Color, decimal? UnitPrice, int MinStockLevel, UniformStatus Status);

public record AdjustStockRequest(Guid ItemId, int Quantity, string? Notes);
}

// ─── IssuanceOrders ───────────────────────────────────────────────────────────
namespace UniformWMS.Application.Features.IssuanceOrders.DTOs
{

public class IssuanceOrderDto
{
    public Guid Id { get; set; }
    public string OrderCode { get; set; } = null!;
    public string? Description { get; set; }
    public IssuanceStatus Status { get; set; }
    public string StatusName { get; set; } = null!;
    public DateTime? IssuedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int LineCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class IssuanceOrderLineDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public Guid ItemId { get; set; }
    public string? ItemName { get; set; }
    public string? ItemCode { get; set; }
    public int Quantity { get; set; }
    public int ActualQuantity { get; set; }
    public string? Notes { get; set; }
}

public record CreateIssuanceOrderRequest(string? Description, List<IssuanceLineRequest> Lines);
public record IssuanceLineRequest(Guid EmployeeId, Guid ItemId, int Quantity, string? Notes);
public record ApproveIssuanceRequest(Guid OrderId);
public record IssueIssuanceRequest(Guid OrderId, List<ActualQuantityUpdate> ActualQuantities);
public record ActualQuantityUpdate(Guid LineId, int ActualQuantity);
}

// ─── ReturnOrders ─────────────────────────────────────────────────────────────
namespace UniformWMS.Application.Features.ReturnOrders.DTOs
{

public class ReturnOrderDto
{
    public Guid Id { get; set; }
    public string OrderCode { get; set; } = null!;
    public string? Description { get; set; }
    public ReturnStatus Status { get; set; }
    public string StatusName { get; set; } = null!;
    public DateTime? ReturnedAt { get; set; }
    public int LineCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReturnOrderLineDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public Guid ItemId { get; set; }
    public string? ItemName { get; set; }
    public int Quantity { get; set; }
    public ItemCondition Condition { get; set; }
    public string? Notes { get; set; }
}

public record CreateReturnOrderRequest(string? Description, List<ReturnLineRequest> Lines);
public record ReturnLineRequest(Guid EmployeeId, Guid ItemId, int Quantity, ItemCondition Condition, string? Notes);
}

// ─── PurchaseOrders ───────────────────────────────────────────────────────────
namespace UniformWMS.Application.Features.PurchaseOrders.DTOs
{

public class PurchaseOrderDto
{
    public Guid Id { get; set; }
    public string OrderCode { get; set; } = null!;
    public Guid SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string? Description { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public string StatusName { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PurchaseOrderLineDto
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public string? ItemName { get; set; }
    public string? ItemCode { get; set; }
    public int OrderedQuantity { get; set; }
    public int ReceivedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public record CreatePurchaseOrderRequest(Guid SupplierId, string? Description,
    DateTime OrderDate, DateTime? ExpectedDate, List<PurchaseLineRequest> Lines);
public record PurchaseLineRequest(Guid ItemId, int Quantity, decimal UnitPrice);
public record ReceivePurchaseRequest(Guid OrderId, List<ReceivedQuantityUpdate> ReceivedItems);

public record ReceivedQuantityUpdate(Guid LineId, int ReceivedQuantity);
}
