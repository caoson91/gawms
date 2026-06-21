using UniformWMS.Domain.Entities;

namespace UniformWMS.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(AppUser user, IEnumerable<string> roles, IEnumerable<string> permissions);
    string GenerateRefreshToken();
    bool ValidateRefreshToken(string token);
    Guid? GetUserIdFromExpiredToken(string token);
}

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Username { get; }
    bool IsAuthenticated { get; }
    IEnumerable<string> Roles { get; }
    IEnumerable<string> Permissions { get; }
    bool HasPermission(string permissionCode);
    bool HasRole(string roleName);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface ICodeGenerator
{
    string GenerateIssuanceCode();
    string GenerateReturnCode();
    string GeneratePurchaseCode();
    string GenerateStockTransactionCode();
    string GenerateItemCode(string categoryCode);
}

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, CancellationToken ct = default);
}
