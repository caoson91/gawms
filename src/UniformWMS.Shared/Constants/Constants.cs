namespace UniformWMS.Shared.Constants;

public static class RoleConstants
{
    public const string Admin = "Admin";
    public const string WarehouseManager = "WarehouseManager";
    public const string Staff = "Staff";
}

public static class PermissionConstants
{
    // Dashboard
    public const string DashboardView = "Dashboard.View";

    // Category
    public const string CategoryView = "Category.View";
    public const string CategoryCreate = "Category.Create";
    public const string CategoryEdit = "Category.Edit";
    public const string CategoryDelete = "Category.Delete";

    // Supplier
    public const string SupplierView = "Supplier.View";
    public const string SupplierCreate = "Supplier.Create";
    public const string SupplierEdit = "Supplier.Edit";
    public const string SupplierDelete = "Supplier.Delete";

    // Employee
    public const string EmployeeView = "Employee.View";
    public const string EmployeeCreate = "Employee.Create";
    public const string EmployeeEdit = "Employee.Edit";
    public const string EmployeeDelete = "Employee.Delete";

    // Uniform
    public const string UniformView = "Uniform.View";
    public const string UniformCreate = "Uniform.Create";
    public const string UniformEdit = "Uniform.Edit";
    public const string UniformDelete = "Uniform.Delete";

    // Issuance
    public const string IssuanceView = "Issuance.View";
    public const string IssuanceCreate = "Issuance.Create";
    public const string IssuanceEdit = "Issuance.Edit";
    public const string IssuanceDelete = "Issuance.Delete";
    public const string IssuanceApprove = "Issuance.Approve";

    // Return
    public const string ReturnView = "Return.View";
    public const string ReturnCreate = "Return.Create";
    public const string ReturnEdit = "Return.Edit";
    public const string ReturnApprove = "Return.Approve";

    // Purchase
    public const string PurchaseView = "Purchase.View";
    public const string PurchaseCreate = "Purchase.Create";
    public const string PurchaseEdit = "Purchase.Edit";
    public const string PurchaseApprove = "Purchase.Approve";

    // Stock
    public const string StockView = "Stock.View";
    public const string StockExport = "Stock.Export";

    // User
    public const string UserView = "User.View";
    public const string UserCreate = "User.Create";
    public const string UserEdit = "User.Edit";
    public const string UserDelete = "User.Delete";

    // Role
    public const string RoleView = "Role.View";
    public const string RoleCreate = "Role.Create";
    public const string RoleEdit = "Role.Edit";
    public const string RoleDelete = "Role.Delete";
}

public static class PolicyConstants
{
    public const string RequirePermission = "RequirePermission";
}
