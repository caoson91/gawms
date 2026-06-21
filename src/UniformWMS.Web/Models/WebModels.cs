namespace UniformWMS.Web.Models;

// ─── Generic wrappers ─────────────────────────────────────────────────────────

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class ApiResponse : ApiResponse<object> { }

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

// ─── Auth ─────────────────────────────────────────────────────────────────────

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiry { get; set; }
    public DateTime RefreshTokenExpiry { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

// ─── Category ─────────────────────────────────────────────────────────────────

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class CreateCategoryRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}

public class UpdateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

// ─── Supplier ─────────────────────────────────────────────────────────────────

public class SupplierDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxCode { get; set; }
    public bool IsActive { get; set; }
}

public class CreateSupplierRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxCode { get; set; }
}

// ─── Employee ─────────────────────────────────────────────────────────────────

public class EmployeeDto
{
    public Guid Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Position { get; set; }
    public Guid DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public DateTime JoinDate { get; set; }
    public int Status { get; set; }
}

// ─── Uniform Item ─────────────────────────────────────────────────────────────

public class UniformItemDto
{
    public Guid Id { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int Size { get; set; }
    public string SizeName { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal? UnitPrice { get; set; }
    public int MinStockLevel { get; set; }
    public int CurrentStock { get; set; }
    public bool IsLowStock { get; set; }
    public int Status { get; set; }
}

public class CreateUniformItemRequest
{
    public string ItemCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public int Size { get; set; }
    public string Color { get; set; } = string.Empty;
    public decimal? UnitPrice { get; set; }
    public int MinStockLevel { get; set; }
}

public class UpdateUniformItemRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public int Size { get; set; }
    public string Color { get; set; } = string.Empty;
    public decimal? UnitPrice { get; set; }
    public int MinStockLevel { get; set; }
    public int Status { get; set; }
}

// ─── Issuance ─────────────────────────────────────────────────────────────────

public class IssuanceOrderDto
{
    public Guid Id { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime? IssuedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int LineCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<IssuanceOrderLineDto> Lines { get; set; } = new();
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

public class CreateIssuanceOrderRequest
{
    public string? Description { get; set; }
    public List<IssuanceLineRequest> Lines { get; set; } = new();
}

public class IssuanceLineRequest
{
    public Guid EmployeeId { get; set; }
    public Guid ItemId { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
}

// ─── Return Order ─────────────────────────────────────────────────────────────

public class ReturnOrderDto
{
    public Guid Id { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime? ReturnedAt { get; set; }
    public int LineCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ReturnOrderLineDto> Lines { get; set; } = new();
}

public class ReturnOrderLineDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public Guid ItemId { get; set; }
    public string? ItemName { get; set; }
    public int Quantity { get; set; }
    public int Condition { get; set; }
    public string? Notes { get; set; }
}

public class CreateReturnOrderRequest
{
    public string? Description { get; set; }
    public List<ReturnLineRequest> Lines { get; set; } = new();
}

public class ReturnLineRequest
{
    public Guid EmployeeId { get; set; }
    public Guid ItemId { get; set; }
    public int Quantity { get; set; }
    public int Condition { get; set; } = 2; // Good
    public string? Notes { get; set; }
}

// ─── Purchase Order ───────────────────────────────────────────────────────────

public class PurchaseOrderDto
{
    public Guid Id { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string? Description { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PurchaseOrderLineDto> Lines { get; set; } = new();
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

// ─── User & Role ──────────────────────────────────────────────────────────────

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public int Status { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Permissions { get; set; } = new();
}

public class PermissionDto
{
    public Guid Id { get; set; }
    public string Module { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
}
