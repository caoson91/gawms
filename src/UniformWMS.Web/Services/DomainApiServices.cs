using Blazored.LocalStorage;
using UniformWMS.Web.Models;

namespace UniformWMS.Web.Services;

// ─── Category Service ─────────────────────────────────────────────────────────

public class CategoryApiService : ApiServiceBase
{
    public CategoryApiService(HttpClient http, ILocalStorageService storage)
        : base(http, storage) { }

    public Task<IEnumerable<CategoryDto>?> GetAllAsync()
        => GetAsync<IEnumerable<CategoryDto>>("api/categories");

    public Task<CategoryDto?> GetByIdAsync(Guid id)
        => GetAsync<CategoryDto>($"api/categories/{id}");

    public Task<ApiResponse<CategoryDto>?> CreateAsync(CreateCategoryRequest req)
        => PostAsync<CategoryDto>("api/categories", req);

    public Task<ApiResponse<CategoryDto>?> UpdateAsync(Guid id, UpdateCategoryRequest req)
        => PutAsync<CategoryDto>($"api/categories/{id}", req);

    public Task<ApiResponse?> DeleteAsync(Guid id)
        => DeleteAsync($"api/categories/{id}");
}

// ─── Supplier Service ─────────────────────────────────────────────────────────

public class SupplierApiService : ApiServiceBase
{
    public SupplierApiService(HttpClient http, ILocalStorageService storage)
        : base(http, storage) { }

    public Task<PagedResult<SupplierDto>?> GetPagedAsync(int page = 1, int pageSize = 20, string? search = null)
        => GetAsync<PagedResult<SupplierDto>>($"api/suppliers?page={page}&pageSize={pageSize}&searchTerm={search}");

    public Task<SupplierDto?> GetByIdAsync(Guid id)
        => GetAsync<SupplierDto>($"api/suppliers/{id}");

    public Task<ApiResponse<SupplierDto>?> CreateAsync(CreateSupplierRequest req)
        => PostAsync<SupplierDto>("api/suppliers", req);

    public Task<ApiResponse<SupplierDto>?> UpdateAsync(Guid id, object req)
        => PutAsync<SupplierDto>($"api/suppliers/{id}", req);

    public Task<ApiResponse?> DeleteAsync(Guid id)
        => DeleteAsync($"api/suppliers/{id}");
}

// ─── Employee Service ─────────────────────────────────────────────────────────

public class EmployeeApiService : ApiServiceBase
{
    public EmployeeApiService(HttpClient http, ILocalStorageService storage)
        : base(http, storage) { }

    public Task<PagedResult<EmployeeDto>?> GetPagedAsync(int page = 1, int pageSize = 20, string? search = null)
        => GetAsync<PagedResult<EmployeeDto>>($"api/employees?page={page}&pageSize={pageSize}&searchTerm={search}");

    public Task<EmployeeDto?> GetByIdAsync(Guid id)
        => GetAsync<EmployeeDto>($"api/employees/{id}");

    public Task<ApiResponse<EmployeeDto>?> CreateAsync(CreateEmployeeRequest req)
        => PostAsync<EmployeeDto>("api/employees", req);

    public Task<ApiResponse<EmployeeDto>?> UpdateAsync(Guid id, object req)
        => PutAsync<EmployeeDto>($"api/employees/{id}", req);

    public Task<ApiResponse?> DeleteAsync(Guid id)
        => DeleteAsync($"api/employees/{id}");
}

public class CreateEmployeeRequest
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Position { get; set; }
    public Guid DepartmentId { get; set; }
    public DateTime JoinDate { get; set; } = DateTime.Today;
}

// ─── Uniform Item Service ─────────────────────────────────────────────────────

public class UniformItemApiService : ApiServiceBase
{
    public UniformItemApiService(HttpClient http, ILocalStorageService storage)
        : base(http, storage) { }

    public Task<PagedResult<UniformItemDto>?> GetPagedAsync(int page = 1, int pageSize = 20, string? search = null)
        => GetAsync<PagedResult<UniformItemDto>>($"api/uniformitems?page={page}&pageSize={pageSize}&searchTerm={search}");

    public Task<UniformItemDto?> GetByIdAsync(Guid id)
        => GetAsync<UniformItemDto>($"api/uniformitems/{id}");

    public Task<IEnumerable<UniformItemDto>?> GetLowStockAsync()
        => GetAsync<IEnumerable<UniformItemDto>>("api/uniformitems/low-stock");

    public Task<ApiResponse<UniformItemDto>?> CreateAsync(CreateUniformItemRequest req)
        => PostAsync<UniformItemDto>("api/uniformitems", req);

    public Task<ApiResponse<UniformItemDto>?> UpdateAsync(Guid id, UpdateUniformItemRequest req)
        => PutAsync<UniformItemDto>($"api/uniformitems/{id}", req);

    public Task<ApiResponse?> DeleteAsync(Guid id)
        => DeleteAsync($"api/uniformitems/{id}");

    public Task<ApiResponse<object>?> AdjustStockAsync(Guid itemId, int qty, string? notes)
        => PostAsync<object>("api/uniformitems/adjust-stock",
            new { ItemId = itemId, Quantity = qty, Notes = notes });
}

// ─── Issuance Order Service ───────────────────────────────────────────────────

public class IssuanceOrderApiService : ApiServiceBase
{
    public IssuanceOrderApiService(HttpClient http, ILocalStorageService storage)
        : base(http, storage) { }

    public Task<PagedResult<IssuanceOrderDto>?> GetPagedAsync(int page = 1, int pageSize = 20, string? search = null)
        => GetAsync<PagedResult<IssuanceOrderDto>>($"api/issuanceorders?page={page}&pageSize={pageSize}&searchTerm={search}");

    public Task<IssuanceOrderDto?> GetByIdAsync(Guid id)
        => GetAsync<IssuanceOrderDto>($"api/issuanceorders/{id}");

    public Task<ApiResponse<IssuanceOrderDto>?> CreateAsync(CreateIssuanceOrderRequest req)
        => PostAsync<IssuanceOrderDto>("api/issuanceorders", req);

    public Task<ApiResponse<object>?> ApproveAsync(Guid id)
        => PostAsync<object>($"api/issuanceorders/{id}/approve", new { });

    public Task<ApiResponse<object>?> CancelAsync(Guid id)
        => PostAsync<object>($"api/issuanceorders/{id}/cancel", new { });

    public Task<ApiResponse<object>?> IssueAsync(Guid orderId, List<ActualQuantityUpdate> quantities)
        => PostAsync<object>("api/issuanceorders/issue",
            new { OrderId = orderId, ActualQuantities = quantities });
}

public class ActualQuantityUpdate
{
    public Guid LineId { get; set; }
    public int ActualQuantity { get; set; }
}

// ─── Return Order Service ─────────────────────────────────────────────────────

public class ReturnOrderApiService : ApiServiceBase
{
    public ReturnOrderApiService(HttpClient http, ILocalStorageService storage)
        : base(http, storage) { }

    public Task<PagedResult<ReturnOrderDto>?> GetPagedAsync(int page = 1, int pageSize = 20, string? search = null)
        => GetAsync<PagedResult<ReturnOrderDto>>($"api/returnorders?page={page}&pageSize={pageSize}&searchTerm={search}");

    public Task<ReturnOrderDto?> GetByIdAsync(Guid id)
        => GetAsync<ReturnOrderDto>($"api/returnorders/{id}");

    public Task<ApiResponse<ReturnOrderDto>?> CreateAsync(CreateReturnOrderRequest req)
        => PostAsync<ReturnOrderDto>("api/returnorders", req);

    public Task<ApiResponse<object>?> ApproveAsync(Guid id)
        => PostAsync<object>($"api/returnorders/{id}/approve", new { });

    public Task<ApiResponse<object>?> CompleteAsync(Guid id)
        => PostAsync<object>($"api/returnorders/{id}/complete", new { });
}

// ─── Purchase Order Service ───────────────────────────────────────────────────

public class PurchaseOrderApiService : ApiServiceBase
{
    public PurchaseOrderApiService(HttpClient http, ILocalStorageService storage)
        : base(http, storage) { }

    public Task<PagedResult<PurchaseOrderDto>?> GetPagedAsync(int page = 1, int pageSize = 20, string? search = null)
        => GetAsync<PagedResult<PurchaseOrderDto>>($"api/purchaseorders?page={page}&pageSize={pageSize}&searchTerm={search}");

    public Task<PurchaseOrderDto?> GetByIdAsync(Guid id)
        => GetAsync<PurchaseOrderDto>($"api/purchaseorders/{id}");

    public Task<ApiResponse<PurchaseOrderDto>?> CreateAsync(object req)
        => PostAsync<PurchaseOrderDto>("api/purchaseorders", req);

    public Task<ApiResponse<object>?> ApproveAsync(Guid id)
        => PostAsync<object>($"api/purchaseorders/{id}/approve", new { });

    public Task<ApiResponse<object>?> ReceiveAsync(Guid orderId, List<ReceivedQtyUpdate> items)
        => PostAsync<object>("api/purchaseorders/receive",
            new { OrderId = orderId, ReceivedItems = items });
}

public class ReceivedQtyUpdate
{
    public Guid LineId { get; set; }
    public int ReceivedQuantity { get; set; }
}

// ─── User & Role Services ─────────────────────────────────────────────────────

public class UserApiService : ApiServiceBase
{
    public UserApiService(HttpClient http, ILocalStorageService storage)
        : base(http, storage) { }

    public Task<PagedResult<UserDto>?> GetPagedAsync(int page = 1, int pageSize = 20, string? search = null)
        => GetAsync<PagedResult<UserDto>>($"api/users?page={page}&pageSize={pageSize}&searchTerm={search}");

    public Task<UserDto?> GetByIdAsync(Guid id)
        => GetAsync<UserDto>($"api/users/{id}");

    public Task<UserDto?> GetCurrentUserAsync()
        => GetAsync<UserDto>("api/users/me");

    public Task<ApiResponse<UserDto>?> CreateAsync(object req)
        => PostAsync<UserDto>("api/users", req);

    public Task<ApiResponse<UserDto>?> UpdateAsync(Guid id, object req)
        => PutAsync<UserDto>($"api/users/{id}", req);

    public Task<ApiResponse?> DeleteAsync(Guid id)
        => DeleteAsync($"api/users/{id}");

    public Task<ApiResponse<object>?> ResetPasswordAsync(Guid id, string newPassword)
        => PostAsync<object>($"api/users/{id}/reset-password", newPassword);
}

public class RoleApiService : ApiServiceBase
{
    public RoleApiService(HttpClient http, ILocalStorageService storage)
        : base(http, storage) { }

    public Task<IEnumerable<RoleDto>?> GetAllAsync()
        => GetAsync<IEnumerable<RoleDto>>("api/roles");

    public Task<RoleDto?> GetByIdAsync(Guid id)
        => GetAsync<RoleDto>($"api/roles/{id}");

    public Task<IEnumerable<PermissionDto>?> GetPermissionsAsync()
        => GetAsync<IEnumerable<PermissionDto>>("api/roles/permissions");

    public Task<ApiResponse<RoleDto>?> CreateAsync(object req)
        => PostAsync<RoleDto>("api/roles", req);

    public Task<ApiResponse<RoleDto>?> UpdateAsync(Guid id, object req)
        => PutAsync<RoleDto>($"api/roles/{id}", req);

    public Task<ApiResponse?> DeleteAsync(Guid id)
        => DeleteAsync($"api/roles/{id}");
}
