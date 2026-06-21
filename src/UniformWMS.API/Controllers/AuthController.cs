using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniformWMS.Application.Common.Interfaces;
using UniformWMS.Application.Common.Models;
using UniformWMS.Application.Features.Auth;
using UniformWMS.Application.Features.Auth.DTOs;

namespace UniformWMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUser;

    public AuthController(IAuthService authService, ICurrentUserService currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<TokenResponse>>> Login(
        [FromBody] LoginRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _authService.LoginAsync(request, ip, ct);
        return Ok(ApiResponse<TokenResponse>.Ok(result, "Đăng nhập thành công."));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<TokenResponse>>> Refresh(
        [FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _authService.RefreshTokenAsync(request, ip, ct);
        return Ok(ApiResponse<TokenResponse>.Ok(result));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> Logout(
        [FromBody] string refreshToken, CancellationToken ct)
    {
        await _authService.LogoutAsync(_currentUser.UserId!.Value, refreshToken, ct);
        return Ok(ApiResponse.OkNoData("Đăng xuất thành công."));
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> ChangePassword(
        [FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        await _authService.ChangePasswordAsync(_currentUser.UserId!.Value, request, ct);
        return Ok(ApiResponse.OkNoData("Đổi mật khẩu thành công."));
    }
}
