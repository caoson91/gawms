using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniformWMS.API.Middlewares;
using UniformWMS.Application.Common.Exceptions;
using UniformWMS.Application.Common.Interfaces;
using UniformWMS.Application.Common.Models;
using UniformWMS.Application.Features.Roles.DTOs;
using UniformWMS.Application.Features.Users.DTOs;
using UniformWMS.Domain.Entities;
using UniformWMS.Domain.Interfaces;
using UniformWMS.Shared.Constants;

namespace UniformWMS.API.Controllers;

// ─── Users ────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _hasher;
    private readonly ICurrentUserService _currentUser;

    public UsersController(IUnitOfWork uow, IMapper mapper,
        IPasswordHasher hasher, ICurrentUserService currentUser)
    {
        _uow = uow; _mapper = mapper; _hasher = hasher; _currentUser = currentUser;
    }

    [HttpGet]
    [RequirePermission(PermissionConstants.UserView)]
    public async Task<ActionResult<ApiResponse<PagedResult<UserDto>>>> GetPaged(
        [FromQuery] PagedQuery query, CancellationToken ct)
    {
        var q = _uow.Users.Query()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Where(u => !u.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.ToLower();
            q = q.Where(u => u.Username.ToLower().Contains(term) || u.FullName.ToLower().Contains(term));
        }

        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(u => u.Username)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);

        return Ok(ApiResponse<PagedResult<UserDto>>.Ok(new PagedResult<UserDto>
        {
            Items = _mapper.Map<IEnumerable<UserDto>>(items),
            TotalCount = total, Page = query.Page, PageSize = query.PageSize
        }));
    }

    [HttpGet("{id:guid}")]
    [RequirePermission(PermissionConstants.UserView)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(Guid id, CancellationToken ct)
    {
        var user = await _uow.Users.GetWithRolesAsync(id, ct)
            ?? throw new NotFoundException("User", id);
        return Ok(ApiResponse<UserDto>.Ok(_mapper.Map<UserDto>(user)));
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser(CancellationToken ct)
    {
        var user = await _uow.Users.GetWithRolesAsync(_currentUser.UserId!.Value, ct)
            ?? throw new NotFoundException("User", _currentUser.UserId!.Value);
        return Ok(ApiResponse<UserDto>.Ok(_mapper.Map<UserDto>(user)));
    }

    [HttpPost]
    [RequirePermission(PermissionConstants.UserCreate)]
    public async Task<ActionResult<ApiResponse<UserDto>>> Create(
        [FromBody] CreateUserRequest request, CancellationToken ct)
    {
        if (await _uow.Users.AnyAsync(u => u.Username == request.Username, ct))
            throw new ConflictException($"Tên đăng nhập '{request.Username}' đã tồn tại.");
        if (await _uow.Users.AnyAsync(u => u.Email == request.Email, ct))
            throw new ConflictException($"Email '{request.Email}' đã được sử dụng.");

        var user = new AppUser
        {
            Username = request.Username,
            PasswordHash = _hasher.Hash(request.Password),
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            CreatedBy = _currentUser.UserId
        };

        foreach (var roleId in request.RoleIds)
            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId });

        await _uow.Users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = user.Id },
            ApiResponse<UserDto>.Ok(_mapper.Map<UserDto>(user), "Tạo người dùng thành công."));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission(PermissionConstants.UserEdit)]
    public async Task<ActionResult<ApiResponse<UserDto>>> Update(
        Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        var user = await _uow.Users.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("User", id);

        user.FullName = request.FullName;
        user.Email = request.Email;
        user.PhoneNumber = request.PhoneNumber;
        user.Status = request.Status;
        user.UpdatedBy = _currentUser.UserId;

        // Sync roles: remove old, add new
        var existingRoles = await _uow.UserRoles.Query()
            .Where(ur => ur.UserId == id).ToListAsync(ct);
        foreach (var r in existingRoles) _uow.UserRoles.Remove(r);
        foreach (var roleId in request.RoleIds)
            await _uow.UserRoles.AddAsync(new UserRole { UserId = id, RoleId = roleId }, ct);

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);
        return Ok(ApiResponse<UserDto>.Ok(_mapper.Map<UserDto>(user)));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(PermissionConstants.UserDelete)]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct)
    {
        if (id == _currentUser.UserId)
            throw new BusinessException("Không thể xóa tài khoản đang đăng nhập.");
        var user = await _uow.Users.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("User", id);
        _uow.Users.SoftDelete(user);
        user.DeletedBy = _currentUser.UserId;
        await _uow.SaveChangesAsync(ct);
        return Ok(ApiResponse.OkNoData("Xóa người dùng thành công."));
    }

    [HttpPost("{id:guid}/reset-password")]
    [RequirePermission(PermissionConstants.UserEdit)]
    public async Task<ActionResult<ApiResponse>> ResetPassword(
        Guid id, [FromBody] string newPassword, CancellationToken ct)
    {
        var user = await _uow.Users.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("User", id);
        user.PasswordHash = _hasher.Hash(newPassword);
        user.UpdatedBy = _currentUser.UserId;
        _uow.Users.Update(user);
        await _uow.RefreshTokens.RevokeAllForUserAsync(id, "Password reset by admin", ct);
        await _uow.SaveChangesAsync(ct);
        return Ok(ApiResponse.OkNoData("Đặt lại mật khẩu thành công."));
    }
}

// ─── Roles ────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public RolesController(IUnitOfWork uow, IMapper mapper, ICurrentUserService currentUser)
    {
        _uow = uow; _mapper = mapper; _currentUser = currentUser;
    }

    [HttpGet]
    [RequirePermission(PermissionConstants.RoleView)]
    public async Task<ActionResult<ApiResponse<IEnumerable<RoleDto>>>> GetAll(CancellationToken ct)
    {
        var roles = await _uow.Roles.Query()
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .Where(r => !r.IsDeleted)
            .ToListAsync(ct);
        return Ok(ApiResponse<IEnumerable<RoleDto>>.Ok(_mapper.Map<IEnumerable<RoleDto>>(roles)));
    }

    [HttpGet("{id:guid}")]
    [RequirePermission(PermissionConstants.RoleView)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetById(Guid id, CancellationToken ct)
    {
        var role = await _uow.Roles.Query()
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new NotFoundException("Role", id);
        return Ok(ApiResponse<RoleDto>.Ok(_mapper.Map<RoleDto>(role)));
    }

    [HttpGet("permissions")]
    [RequirePermission(PermissionConstants.RoleView)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PermissionDto>>>> GetPermissions(CancellationToken ct)
    {
        var perms = await _uow.Permissions.Query()
            .OrderBy(p => p.Module).ThenBy(p => p.Action)
            .ToListAsync(ct);
        return Ok(ApiResponse<IEnumerable<PermissionDto>>.Ok(_mapper.Map<IEnumerable<PermissionDto>>(perms)));
    }

    [HttpPost]
    [RequirePermission(PermissionConstants.RoleCreate)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Create(
        [FromBody] CreateRoleRequest request, CancellationToken ct)
    {
        if (await _uow.Roles.AnyAsync(r => r.Name == request.Name, ct))
            throw new ConflictException($"Tên vai trò '{request.Name}' đã tồn tại.");

        var role = new AppRole
        {
            Name = request.Name,
            Description = request.Description,
            CreatedBy = _currentUser.UserId
        };

        foreach (var permId in request.PermissionIds)
            role.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permId });

        await _uow.Roles.AddAsync(role, ct);
        await _uow.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = role.Id },
            ApiResponse<RoleDto>.Ok(_mapper.Map<RoleDto>(role), "Tạo vai trò thành công."));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission(PermissionConstants.RoleEdit)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Update(
        Guid id, [FromBody] UpdateRoleRequest request, CancellationToken ct)
    {
        var role = await _uow.Roles.Query()
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new NotFoundException("Role", id);

        role.Name = request.Name;
        role.Description = request.Description;
        role.UpdatedBy = _currentUser.UserId;

        // Sync permissions
        foreach (var rp in role.RolePermissions.ToList()) _uow.RolePermissions.Remove(rp);
        foreach (var permId in request.PermissionIds)
            await _uow.RolePermissions.AddAsync(new RolePermission { RoleId = id, PermissionId = permId }, ct);

        _uow.Roles.Update(role);
        await _uow.SaveChangesAsync(ct);
        return Ok(ApiResponse<RoleDto>.Ok(_mapper.Map<RoleDto>(role)));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(PermissionConstants.RoleDelete)]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct)
    {
        var role = await _uow.Roles.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Role", id);
        if (role.Name == RoleConstants.Admin)
            throw new BusinessException("Không thể xóa vai trò Admin.");
        _uow.Roles.SoftDelete(role);
        role.DeletedBy = _currentUser.UserId;
        await _uow.SaveChangesAsync(ct);
        return Ok(ApiResponse.OkNoData("Xóa vai trò thành công."));
    }
}
