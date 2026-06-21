using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniformWMS.Domain.Entities;

namespace UniformWMS.Infrastructure.Data.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> b)
    {
        b.ToTable("Users");
        b.HasKey(e => e.Id);
        b.Property(e => e.Username).IsRequired().HasMaxLength(100);
        b.Property(e => e.Email).IsRequired().HasMaxLength(256);
        b.Property(e => e.FullName).IsRequired().HasMaxLength(200);
        b.Property(e => e.PasswordHash).IsRequired();
        b.Property(e => e.PhoneNumber).HasMaxLength(20);
        b.HasIndex(e => e.Username).IsUnique();
        b.HasIndex(e => e.Email).IsUnique();
    }
}

public class AppRoleConfiguration : IEntityTypeConfiguration<AppRole>
{
    public void Configure(EntityTypeBuilder<AppRole> b)
    {
        b.ToTable("Roles");
        b.HasKey(e => e.Id);
        b.Property(e => e.Name).IsRequired().HasMaxLength(100);
        b.HasIndex(e => e.Name).IsUnique();
    }
}

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> b)
    {
        b.ToTable("Permissions");
        b.HasKey(e => e.Id);
        b.Property(e => e.Module).IsRequired().HasMaxLength(50);
        b.Property(e => e.Action).IsRequired().HasMaxLength(50);
        b.Property(e => e.Code).IsRequired().HasMaxLength(100);
        b.HasIndex(e => e.Code).IsUnique();
    }
}

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> b)
    {
        b.ToTable("UserRoles");
        b.HasKey(e => e.Id);
        b.HasOne(e => e.User).WithMany(u => u.UserRoles).HasForeignKey(e => e.UserId);
        b.HasOne(e => e.Role).WithMany(r => r.UserRoles).HasForeignKey(e => e.RoleId);
        b.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();
    }
}

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> b)
    {
        b.ToTable("RolePermissions");
        b.HasKey(e => e.Id);
        b.HasOne(e => e.Role).WithMany(r => r.RolePermissions).HasForeignKey(e => e.RoleId);
        b.HasOne(e => e.Permission).WithMany(p => p.RolePermissions).HasForeignKey(e => e.PermissionId);
        b.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("RefreshTokens");
        b.HasKey(e => e.Id);
        b.Property(e => e.Token).IsRequired().HasMaxLength(500);
        b.HasOne(e => e.User).WithMany(u => u.RefreshTokens).HasForeignKey(e => e.UserId);
        b.HasIndex(e => e.Token);
    }
}

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.ToTable("Categories");
        b.HasKey(e => e.Id);
        b.Property(e => e.Code).IsRequired().HasMaxLength(20);
        b.Property(e => e.Name).IsRequired().HasMaxLength(100);
        b.HasIndex(e => e.Code).IsUnique();
    }
}

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> b)
    {
        b.ToTable("Suppliers");
        b.HasKey(e => e.Id);
        b.Property(e => e.Code).IsRequired().HasMaxLength(20);
        b.Property(e => e.Name).IsRequired().HasMaxLength(200);
        b.HasIndex(e => e.Code).IsUnique();
    }
}

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> b)
    {
        b.ToTable("Departments");
        b.HasKey(e => e.Id);
        b.Property(e => e.Code).IsRequired().HasMaxLength(20);
        b.Property(e => e.Name).IsRequired().HasMaxLength(100);
        b.HasOne(e => e.Parent).WithMany().HasForeignKey(e => e.ParentId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> b)
    {
        b.ToTable("Employees");
        b.HasKey(e => e.Id);
        b.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(20);
        b.Property(e => e.FullName).IsRequired().HasMaxLength(200);
        b.HasIndex(e => e.EmployeeCode).IsUnique();
        b.HasOne(e => e.Department).WithMany(d => d.Employees).HasForeignKey(e => e.DepartmentId);
    }
}

public class UniformItemConfiguration : IEntityTypeConfiguration<UniformItem>
{
    public void Configure(EntityTypeBuilder<UniformItem> b)
    {
        b.ToTable("UniformItems");
        b.HasKey(e => e.Id);
        b.Property(e => e.ItemCode).IsRequired().HasMaxLength(30);
        b.Property(e => e.Name).IsRequired().HasMaxLength(200);
        b.Property(e => e.Color).IsRequired().HasMaxLength(50);
        b.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
        b.HasIndex(e => e.ItemCode).IsUnique();
        b.HasOne(e => e.Category).WithMany(c => c.UniformItems).HasForeignKey(e => e.CategoryId);
    }
}

public class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
{
    public void Configure(EntityTypeBuilder<StockTransaction> b)
    {
        b.ToTable("StockTransactions");
        b.HasKey(e => e.Id);
        b.Property(e => e.TransactionCode).IsRequired().HasMaxLength(30);
        b.HasOne(e => e.Item).WithMany(i => i.StockTransactions).HasForeignKey(e => e.ItemId);
    }
}

public class IssuanceOrderConfiguration : IEntityTypeConfiguration<IssuanceOrder>
{
    public void Configure(EntityTypeBuilder<IssuanceOrder> b)
    {
        b.ToTable("IssuanceOrders");
        b.HasKey(e => e.Id);
        b.Property(e => e.OrderCode).IsRequired().HasMaxLength(30);
        b.HasIndex(e => e.OrderCode).IsUnique();
    }
}

public class IssuanceOrderLineConfiguration : IEntityTypeConfiguration<IssuanceOrderLine>
{
    public void Configure(EntityTypeBuilder<IssuanceOrderLine> b)
    {
        b.ToTable("IssuanceOrderLines");
        b.HasKey(e => e.Id);
        b.HasOne(e => e.Order).WithMany(o => o.Lines).HasForeignKey(e => e.OrderId);
        b.HasOne(e => e.Employee).WithMany(emp => emp.IssuanceLines).HasForeignKey(e => e.EmployeeId);
        b.HasOne(e => e.Item).WithMany(i => i.IssuanceLines).HasForeignKey(e => e.ItemId);
    }
}

public class ReturnOrderConfiguration : IEntityTypeConfiguration<ReturnOrder>
{
    public void Configure(EntityTypeBuilder<ReturnOrder> b)
    {
        b.ToTable("ReturnOrders");
        b.HasKey(e => e.Id);
        b.Property(e => e.OrderCode).IsRequired().HasMaxLength(30);
        b.HasIndex(e => e.OrderCode).IsUnique();
    }
}

public class ReturnOrderLineConfiguration : IEntityTypeConfiguration<ReturnOrderLine>
{
    public void Configure(EntityTypeBuilder<ReturnOrderLine> b)
    {
        b.ToTable("ReturnOrderLines");
        b.HasKey(e => e.Id);
        b.HasOne(e => e.Order).WithMany(o => o.Lines).HasForeignKey(e => e.OrderId);
        b.HasOne(e => e.Employee).WithMany(emp => emp.ReturnLines).HasForeignKey(e => e.EmployeeId);
        b.HasOne(e => e.Item).WithMany(i => i.ReturnLines).HasForeignKey(e => e.ItemId);
    }
}

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> b)
    {
        b.ToTable("PurchaseOrders");
        b.HasKey(e => e.Id);
        b.Property(e => e.OrderCode).IsRequired().HasMaxLength(30);
        b.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
        b.HasIndex(e => e.OrderCode).IsUnique();
        b.HasOne(e => e.Supplier).WithMany(s => s.PurchaseOrders).HasForeignKey(e => e.SupplierId);
    }
}

public class PurchaseOrderLineConfiguration : IEntityTypeConfiguration<PurchaseOrderLine>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderLine> b)
    {
        b.ToTable("PurchaseOrderLines");
        b.HasKey(e => e.Id);
        b.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
        b.Ignore(e => e.TotalPrice);
        b.HasOne(e => e.Order).WithMany(o => o.Lines).HasForeignKey(e => e.OrderId);
        b.HasOne(e => e.Item).WithMany(i => i.PurchaseLines).HasForeignKey(e => e.ItemId);
    }
}
