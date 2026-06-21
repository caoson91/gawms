using Microsoft.Extensions.Configuration;
using UniformWMS.Application.Common.Exceptions;
using UniformWMS.Application.Common.Interfaces;
using UniformWMS.Application.Features.Auth.DTOs;
using UniformWMS.Domain.Entities;
using UniformWMS.Domain.Interfaces;

namespace UniformWMS.Application.Features.Auth;

public interface IAuthService
{
    Task<TokenResponse> LoginAsync(LoginRequest request, string ipAddress, CancellationToken ct = default);
    Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress, CancellationToken ct = default);
    Task RevokeTokenAsync(string refreshToken, string ipAddress, CancellationToken ct = default);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default);
    Task LogoutAsync(Guid userId, string refreshToken, CancellationToken ct = default);
}

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IJwtService _jwt;
    private readonly IPasswordHasher _hasher;
    private readonly IConfiguration _config;

    public AuthService(IUnitOfWork uow, IJwtService jwt, IPasswordHasher hasher, IConfiguration config)
    {
        _uow = uow;
        _jwt = jwt;
        _hasher = hasher;
        _config = config;
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest request, string ipAddress, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByUsernameAsync(request.Username, ct)
            ?? throw new UnauthorizedException("Tên đăng nhập hoặc mật khẩu không đúng.");

        if (!_hasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Tên đăng nhập hoặc mật khẩu không đúng.");

        if (user.Status != Domain.Enums.UserStatus.Active)
            throw new UnauthorizedException("Tài khoản đã bị khóa hoặc vô hiệu hóa.");

        var userWithRoles = await _uow.Users.GetWithRolesAsync(user.Id, ct) ?? user;
        var roles = userWithRoles.UserRoles.Select(ur => ur.Role?.Name ?? "").Where(r => r != "").ToList();
        var permissions = userWithRoles.UserRoles
            .SelectMany(ur => ur.Role?.RolePermissions ?? [])
            .Select(rp => rp.Permission?.Code ?? "")
            .Where(p => p != "")
            .Distinct()
            .ToList();

        var accessToken = _jwt.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _jwt.GenerateRefreshToken();

        var refreshExpiry = int.Parse(_config["Jwt:RefreshTokenExpiryDays"] ?? "30");
        var tokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshExpiry),
            CreatedByIp = ipAddress
        };

        await _uow.RefreshTokens.AddAsync(tokenEntity, ct);
        user.LastLoginAt = DateTime.UtcNow;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);

        var accessExpiry = int.Parse(_config["Jwt:AccessTokenExpiryMinutes"] ?? "60");
        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(accessExpiry),
            RefreshTokenExpiry = tokenEntity.ExpiresAt,
            UserId = user.Id.ToString(),
            Username = user.Username,
            FullName = user.FullName,
            Roles = roles,
            Permissions = permissions
        };
    }

    public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress, CancellationToken ct = default)
    {
        var userId = _jwt.GetUserIdFromExpiredToken(request.AccessToken)
            ?? throw new UnauthorizedException("Access token không hợp lệ.");

        var storedToken = await _uow.RefreshTokens.GetActiveTokenAsync(request.RefreshToken, ct)
            ?? throw new UnauthorizedException("Refresh token không hợp lệ hoặc đã hết hạn.");

        if (storedToken.UserId != userId)
            throw new UnauthorizedException("Token không khớp.");

        // Revoke old, issue new
        storedToken.IsRevoked = true;
        storedToken.RevokedByIp = ipAddress;
        storedToken.RevokedReason = "Replaced by new token";
        _uow.RefreshTokens.Update(storedToken);

        var user = await _uow.Users.GetWithRolesAsync(userId, ct)
            ?? throw new UnauthorizedException("Không tìm thấy người dùng.");

        var roles = user.UserRoles.Select(ur => ur.Role?.Name ?? "").Where(r => r != "").ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role?.RolePermissions ?? [])
            .Select(rp => rp.Permission?.Code ?? "")
            .Where(p => p != "")
            .Distinct()
            .ToList();

        var accessToken = _jwt.GenerateAccessToken(user, roles, permissions);
        var newRefreshToken = _jwt.GenerateRefreshToken();
        var refreshExpiry = int.Parse(_config["Jwt:RefreshTokenExpiryDays"] ?? "30");

        var newToken = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshExpiry),
            CreatedByIp = ipAddress
        };

        await _uow.RefreshTokens.AddAsync(newToken, ct);
        await _uow.SaveChangesAsync(ct);

        var accessExpiry = int.Parse(_config["Jwt:AccessTokenExpiryMinutes"] ?? "60");
        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(accessExpiry),
            RefreshTokenExpiry = newToken.ExpiresAt,
            UserId = user.Id.ToString(),
            Username = user.Username,
            FullName = user.FullName,
            Roles = roles,
            Permissions = permissions
        };
    }

    public async Task RevokeTokenAsync(string refreshToken, string ipAddress, CancellationToken ct = default)
    {
        var token = await _uow.RefreshTokens.GetActiveTokenAsync(refreshToken, ct)
            ?? throw new NotFoundException("Refresh token không tồn tại.");

        token.IsRevoked = true;
        token.RevokedByIp = ipAddress;
        token.RevokedReason = "Revoked by user";
        _uow.RefreshTokens.Update(token);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        if (request.NewPassword != request.ConfirmPassword)
            throw new ValidationException("ConfirmPassword", "Mật khẩu xác nhận không khớp.");

        var user = await _uow.Users.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User", userId);

        if (!_hasher.Verify(request.CurrentPassword, user.PasswordHash))
            throw new ValidationException("CurrentPassword", "Mật khẩu hiện tại không đúng.");

        user.PasswordHash = _hasher.Hash(request.NewPassword);
        _uow.Users.Update(user);
        await _uow.RefreshTokens.RevokeAllForUserAsync(userId, "Password changed", ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task LogoutAsync(Guid userId, string refreshToken, CancellationToken ct = default)
    {
        var token = await _uow.RefreshTokens.GetActiveTokenAsync(refreshToken, ct);
        if (token != null && token.UserId == userId)
        {
            token.IsRevoked = true;
            token.RevokedReason = "Logout";
            _uow.RefreshTokens.Update(token);
            await _uow.SaveChangesAsync(ct);
        }
    }
}
