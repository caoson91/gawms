using Microsoft.EntityFrameworkCore;
using UniformWMS.Application.Common.Interfaces;
using UniformWMS.Domain.Entities;
using UniformWMS.Infrastructure.Data;
using UniformWMS.Shared.Constants;

namespace UniformWMS.Infrastructure.Data.Seeders;

public class DatabaseSeeder
{
    private readonly AppDbContext _ctx;
    private readonly IPasswordHasher _hasher;

    public DatabaseSeeder(AppDbContext ctx, IPasswordHasher hasher)
    {
        _ctx = ctx;
        _hasher = hasher;
    }

    public async Task SeedAsync()
    {
        await _ctx.Database.MigrateAsync();
        await SeedPermissionsAsync();
        await SeedRolesAsync();
        await SeedAdminUserAsync();
        await SeedCategoriesAsync();
        await SeedDepartmentsAsync();
        await _ctx.SaveChangesAsync();
    }

    private async Task SeedPermissionsAsync()
    {
        if (await _ctx.Permissions.AnyAsync()) return;

        var modules = new[] { "Dashboard", "Category", "Supplier", "Employee", "Uniform", "Issuance", "Return", "Purchase", "Stock", "Report", "User", "Role" };
        var actions = new[] { "View", "Create", "Edit", "Delete", "Approve", "Export" };

        var permissions = new List<Permission>();
        foreach (var module in modules)
        {
            foreach (var action in actions)
            {
                // Not all modules need all actions
                if ((action == "Approve") && module is not ("Issuance" or "Return" or "Purchase")) continue;
                if ((action == "Export") && module is not ("Report" or "Stock")) continue;

                permissions.Add(new Permission
                {
                    Module = module,
                    Action = action,
                    Code = $"{module}.{action}",
                    Description = $"{action} {module}"
                });
            }
        }
        await _ctx.Permissions.AddRangeAsync(permissions);
        await _ctx.SaveChangesAsync();
    }

    private async Task SeedRolesAsync()
    {
        if (await _ctx.Roles.AnyAsync()) return;

        var allPermissions = await _ctx.Permissions.ToListAsync();

        // Admin - all permissions
        var adminRole = new AppRole { Name = RoleConstants.Admin, Description = "Quản trị hệ thống" };
        adminRole.RolePermissions = allPermissions.Select(p => new RolePermission
        {
            RoleId = adminRole.Id,
            PermissionId = p.Id
        }).ToList();
        await _ctx.Roles.AddAsync(adminRole);

        // Warehouse Manager
        var warehousePerms = allPermissions
            .Where(p => p.Module is "Uniform" or "Issuance" or "Return" or "Purchase" or "Stock" or "Category" or "Supplier" or "Employee" or "Dashboard")
            .ToList();
        var warehouseRole = new AppRole { Name = RoleConstants.WarehouseManager, Description = "Quản lý kho" };
        warehouseRole.RolePermissions = warehousePerms.Select(p => new RolePermission
        {
            RoleId = warehouseRole.Id,
            PermissionId = p.Id
        }).ToList();
        await _ctx.Roles.AddAsync(warehouseRole);

        // Staff - view and create only
        var staffPerms = allPermissions
            .Where(p => p.Action is "View" or "Create" && p.Module is "Issuance" or "Return" or "Dashboard")
            .ToList();
        var staffRole = new AppRole { Name = RoleConstants.Staff, Description = "Nhân viên kho" };
        staffRole.RolePermissions = staffPerms.Select(p => new RolePermission
        {
            RoleId = staffRole.Id,
            PermissionId = p.Id
        }).ToList();
        await _ctx.Roles.AddAsync(staffRole);

        await _ctx.SaveChangesAsync();
    }

    private async Task SeedAdminUserAsync()
    {
        if (await _ctx.Users.AnyAsync()) return;

        var adminRole = await _ctx.Roles.FirstAsync(r => r.Name == RoleConstants.Admin);
        var admin = new AppUser
        {
            Username = "admin",
            PasswordHash = _hasher.Hash("Admin@123"),
            FullName = "System Administrator",
            Email = "admin@company.com",
            Status = Domain.Enums.UserStatus.Active
        };
        admin.UserRoles.Add(new UserRole { UserId = admin.Id, RoleId = adminRole.Id });
        await _ctx.Users.AddAsync(admin);
        await _ctx.SaveChangesAsync();
    }

    private async Task SeedCategoriesAsync()
    {
        if (await _ctx.Categories.AnyAsync()) return;

        var categories = new[]
        {
            new Category { Code = "AO", Name = "Áo đồng phục", DisplayOrder = 1 },
            new Category { Code = "QUAN", Name = "Quần đồng phục", DisplayOrder = 2 },
            new Category { Code = "GIAY", Name = "Giày bảo hộ", DisplayOrder = 3 },
            new Category { Code = "MU", Name = "Mũ / Nón", DisplayOrder = 4 },
            new Category { Code = "YEM", Name = "Yếm / Tạp dề", DisplayOrder = 5 },
            new Category { Code = "BAO_TAY", Name = "Bao tay / Găng tay", DisplayOrder = 6 },
            new Category { Code = "KHAC", Name = "Khác", DisplayOrder = 99 },
        };
        await _ctx.Categories.AddRangeAsync(categories);
        await _ctx.SaveChangesAsync();
    }

    private async Task SeedDepartmentsAsync()
    {
        if (await _ctx.Departments.AnyAsync()) return;

        var departments = new[]
        {
            new Department { Code = "IT", Name = "Phòng Công nghệ thông tin" },
            new Department { Code = "HR", Name = "Phòng Nhân sự" },
            new Department { Code = "KH", Name = "Phòng Kế hoạch" },
            new Department { Code = "KHO", Name = "Phòng Kho" },
            new Department { Code = "SX", Name = "Xưởng Sản xuất" },
        };
        await _ctx.Departments.AddRangeAsync(departments);
        await _ctx.SaveChangesAsync();
    }
}
