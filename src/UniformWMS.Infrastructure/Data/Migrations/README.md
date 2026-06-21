# EF Core Migrations

Chạy các lệnh sau từ thư mục root solution để tạo và áp dụng migrations:

```bash
# Thêm migration đầu tiên
dotnet ef migrations add InitialCreate \
  --project src/UniformWMS.Infrastructure \
  --startup-project src/UniformWMS.API \
  --output-dir Data/Migrations

# Áp dụng migration lên database
dotnet ef database update \
  --project src/UniformWMS.Infrastructure \
  --startup-project src/UniformWMS.API
```

> Database sẽ được seed tự động khi API khởi động lần đầu (xem `DatabaseSeeder`).
> Tài khoản mặc định: **admin / Admin@123**
