# UniformWMS — Hệ thống quản lý kho đồng phục

## 🏗️ Kiến trúc

```
UniformWMS.sln
├── src/
│   ├── UniformWMS.Domain          # Entities, Enums, Domain Interfaces
│   ├── UniformWMS.Application     # Services, DTOs, Mappings, Validation
│   ├── UniformWMS.Infrastructure  # EF Core, Repositories, UoW, JWT, Seeders
│   ├── UniformWMS.Shared          # Constants, Extensions (dùng chung)
│   ├── UniformWMS.API             # ASP.NET Core Web API (Controllers, Middleware)
│   └── UniformWMS.Web             # Blazor Server (Pages, Components, API Clients)
```

## ⚙️ Tech Stack

| Layer          | Technology                        |
|----------------|-----------------------------------|
| Backend API    | .NET 10 ASP.NET Core Web API      |
| Frontend       | .NET 10 Blazor Server             |
| Database       | SQL Server (EF Core Code First)   |
| ORM            | Entity Framework Core 9           |
| Auth           | JWT + Refresh Token (BCrypt)      |
| Mapping        | AutoMapper 13                     |
| Validation     | FluentValidation 11               |
| Storage (Web)  | Blazored.LocalStorage             |

## 🚀 Hướng dẫn chạy

### 1. Yêu cầu
- .NET 10 SDK
- SQL Server (local hoặc Docker)

### 2. Cấu hình Connection String
Cập nhật `appsettings.json` trong `UniformWMS.API`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=UniformWMS;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 3. Tạo Database & Migration
```bash
# Từ thư mục gốc solution
dotnet ef migrations add InitialCreate \
  --project src/UniformWMS.Infrastructure \
  --startup-project src/UniformWMS.API \
  --output-dir Data/Migrations

dotnet ef database update \
  --project src/UniformWMS.Infrastructure \
  --startup-project src/UniformWMS.API
```

### 4. Chạy API
```bash
cd src/UniformWMS.API
dotnet run
# API chạy tại: https://localhost:7100
# Swagger UI: https://localhost:7100/swagger
```

### 5. Chạy Blazor Web
```bash
cd src/UniformWMS.Web
dotnet run
# Web chạy tại: https://localhost:7200
```

## 🔐 Tài khoản mặc định
| Username | Password   | Vai trò |
|----------|------------|---------|
| admin    | Admin@123  | Admin   |

## 📦 Modules

| Module              | Chức năng                                      |
|---------------------|------------------------------------------------|
| Dashboard           | Tổng quan, cảnh báo tồn kho thấp               |
| Mặt hàng (Uniform)  | CRUD mặt hàng đồng phục theo Size/Màu          |
| Danh mục            | Phân loại mặt hàng                             |
| Tồn kho (Stock)     | Theo dõi tồn, điều chỉnh thủ công              |
| Cấp phát (Issuance) | Tạo → Duyệt → Xuất kho, cập nhật tồn tự động  |
| Thu hồi (Return)    | Tạo → Duyệt → Nhập lại kho, ghi tình trạng    |
| Nhập hàng (PO)      | Tạo → Duyệt → Nhận hàng, cập nhật tồn tự động |
| Nhân viên           | Quản lý nhân viên theo phòng ban               |
| Nhà cung cấp        | Quản lý nhà cung cấp                           |
| Người dùng          | Quản lý tài khoản hệ thống                     |
| Vai trò & Quyền     | RBAC granular permission                       |

## 🔑 Permission Matrix

Quyền được định nghĩa theo dạng `Module.Action`, ví dụ:
- `Uniform.View`, `Uniform.Create`, `Uniform.Edit`, `Uniform.Delete`
- `Issuance.Approve`, `Return.Approve`, `Purchase.Approve`
- `Stock.View`, `Stock.Export`

## 🔄 JWT Flow

```
Login → AccessToken (60 phút) + RefreshToken (30 ngày)
     → AccessToken hết hạn → POST /api/auth/refresh
     → RefreshToken mới + AccessToken mới
```
