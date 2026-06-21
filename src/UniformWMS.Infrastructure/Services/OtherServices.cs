using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Security.Principal;
using UniformWMS.Application.Common.Interfaces;

namespace UniformWMS.Infrastructure.Services;

// ─── Password Hasher ──────────────────────────────────────────────────────────

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));

    public bool Verify(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);
}

// ─── Current User Service ─────────────────────────────────────────────────────

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            //var id = Principal?.FindFirst(ClaimTypes.NameIdentifier);
            //return id != null ? Guid.Parse(id) : null;

            var id = Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return id != null ? Guid.Parse(id) : null;
        }
    }

    public string? Username => Principal?.FindFirst(ClaimTypes.Name)?.Value;
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public IEnumerable<string> Roles
        => Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? Enumerable.Empty<string>();

    public IEnumerable<string> Permissions
        => Principal?.FindAll("permission").Select(c => c.Value) ?? Enumerable.Empty<string>();

    public bool HasPermission(string permissionCode)
        => Permissions.Contains(permissionCode);

    public bool HasRole(string roleName)
        => Roles.Contains(roleName);
}

// ─── Code Generator ───────────────────────────────────────────────────────────

public class CodeGenerator : ICodeGenerator
{
    private static readonly object _lock = new();
    private static int _counter = 0;

    private string Next(string prefix)
    {
        lock (_lock) { _counter = (_counter + 1) % 9999; }
        return $"{prefix}{DateTime.UtcNow:yyyyMMdd}{_counter:D4}";
    }

    public string GenerateIssuanceCode() => Next("ISO-");
    public string GenerateReturnCode() => Next("RET-");
    public string GeneratePurchaseCode() => Next("PO-");
    public string GenerateStockTransactionCode() => Next("STK-");
    public string GenerateItemCode(string categoryCode) => $"{categoryCode}-{DateTime.UtcNow:yyyyMMdd}{Random.Shared.Next(100, 999)}";
}
