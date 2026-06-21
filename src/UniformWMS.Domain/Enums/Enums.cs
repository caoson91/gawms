namespace UniformWMS.Domain.Enums;

public enum UniformSize
{
    XS = 1, S, M, L, XL, XXL, XXXL,
    Free = 10
}

public enum UniformStatus
{
    Active = 1,
    Discontinued = 2,
    OutOfStock = 3
}

public enum StockTransactionType
{
    Import = 1,       // Nhập kho
    Export = 2,       // Xuất kho (cấp phát)
    Return = 3,       // Thu hồi
    Adjustment = 4,   // Điều chỉnh
    Damage = 5        // Hủy hỏng
}

public enum IssuanceStatus
{
    Draft = 1,
    Approved = 2,
    Issued = 3,
    Cancelled = 4
}

public enum ReturnStatus
{
    Draft = 1,
    Approved = 2,
    Returned = 3,
    Cancelled = 4
}

public enum PurchaseOrderStatus
{
    Draft = 1,
    Ordered = 2,
    PartialReceived = 3,
    FullReceived = 4,
    Cancelled = 5
}

public enum EmployeeStatus
{
    Active = 1,
    Inactive = 2,
    Resigned = 3
}

public enum UserStatus
{
    Active = 1,
    Inactive = 2,
    Locked = 3
}

public enum ItemCondition
{
    New = 1,
    Good = 2,
    Worn = 3,
    Damaged = 4
}
