using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniformWMS.API.Middlewares;
using UniformWMS.Application.Common.Exceptions;
using UniformWMS.Application.Common.Interfaces;
using UniformWMS.Application.Common.Models;
using UniformWMS.Application.Features.Categories.DTOs;
using UniformWMS.Application.Features.Employees.DTOs;
using UniformWMS.Application.Features.PurchaseOrders.DTOs;
using UniformWMS.Application.Features.ReturnOrders.DTOs;
using UniformWMS.Application.Features.Suppliers.DTOs;
using UniformWMS.Domain.Entities;
using UniformWMS.Domain.Enums;
using UniformWMS.Domain.Interfaces;
using UniformWMS.Shared.Constants;

// ─── Categories ───────────────────────────────────────────────────────────────
namespace UniformWMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public CategoriesController(IUnitOfWork uow, IMapper mapper, ICurrentUserService currentUser)
    {
        _uow = uow; _mapper = mapper; _currentUser = currentUser;
    }

    [HttpGet]
    [RequirePermission(PermissionConstants.CategoryView)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetAll(CancellationToken ct)
    {
        var items = await _uow.Categories.Query()
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(ct);
        return Ok(ApiResponse<IEnumerable<CategoryDto>>.Ok(_mapper.Map<IEnumerable<CategoryDto>>(items)));
    }

    [HttpGet("{id:guid}")]
    [RequirePermission(PermissionConstants.CategoryView)]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetById(Guid id, CancellationToken ct)
    {
        var item = await _uow.Categories.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Category", id);
        return Ok(ApiResponse<CategoryDto>.Ok(_mapper.Map<CategoryDto>(item)));
    }

    [HttpPost]
    [RequirePermission(PermissionConstants.CategoryCreate)]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Create(
        [FromBody] CreateCategoryRequest request, CancellationToken ct)
    {
        if (await _uow.Categories.AnyAsync(c => c.Code == request.Code, ct))
            throw new ConflictException($"Mã danh mục '{request.Code}' đã tồn tại.");

        var entity = _mapper.Map<Category>(request);
        entity.CreatedBy = _currentUser.UserId;
        await _uow.Categories.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id },
            ApiResponse<CategoryDto>.Ok(_mapper.Map<CategoryDto>(entity), "Tạo danh mục thành công."));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission(PermissionConstants.CategoryEdit)]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Update(
        Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken ct)
    {
        var entity = await _uow.Categories.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Category", id);
        _mapper.Map(request, entity);
        entity.UpdatedBy = _currentUser.UserId;
        _uow.Categories.Update(entity);
        await _uow.SaveChangesAsync(ct);
        return Ok(ApiResponse<CategoryDto>.Ok(_mapper.Map<CategoryDto>(entity)));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(PermissionConstants.CategoryDelete)]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct)
    {
        var entity = await _uow.Categories.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Category", id);
        _uow.Categories.SoftDelete(entity);
        entity.DeletedBy = _currentUser.UserId;
        await _uow.SaveChangesAsync(ct);
        return Ok(ApiResponse.OkNoData("Xóa danh mục thành công."));
    }
}

// ─── Suppliers ────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public SuppliersController(IUnitOfWork uow, IMapper mapper, ICurrentUserService currentUser)
    {
        _uow = uow; _mapper = mapper; _currentUser = currentUser;
    }

    [HttpGet]
    [RequirePermission(PermissionConstants.SupplierView)]
    public async Task<ActionResult<ApiResponse<PagedResult<SupplierDto>>>> GetPaged(
        [FromQuery] PagedQuery query, CancellationToken ct)
    {
        var q = _uow.Suppliers.Query().Where(s => !s.IsDeleted);
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.ToLower();
            q = q.Where(s => s.Name.ToLower().Contains(term) || s.Code.ToLower().Contains(term));
        }
        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(s => s.Name)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return Ok(ApiResponse<PagedResult<SupplierDto>>.Ok(new PagedResult<SupplierDto>
        {
            Items = _mapper.Map<IEnumerable<SupplierDto>>(items),
            TotalCount = total, Page = query.Page, PageSize = query.PageSize
        }));
    }

    [HttpGet("{id:guid}")]
    [RequirePermission(PermissionConstants.SupplierView)]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> GetById(Guid id, CancellationToken ct)
    {
        var entity = await _uow.Suppliers.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Supplier", id);
        return Ok(ApiResponse<SupplierDto>.Ok(_mapper.Map<SupplierDto>(entity)));
    }

    [HttpPost]
    [RequirePermission(PermissionConstants.SupplierCreate)]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> Create(
        [FromBody] CreateSupplierRequest request, CancellationToken ct)
    {
        if (await _uow.Suppliers.AnyAsync(s => s.Code == request.Code, ct))
            throw new ConflictException($"Mã nhà cung cấp '{request.Code}' đã tồn tại.");
        var entity = _mapper.Map<Supplier>(request);
        entity.CreatedBy = _currentUser.UserId;
        await _uow.Suppliers.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id },
            ApiResponse<SupplierDto>.Ok(_mapper.Map<SupplierDto>(entity), "Tạo nhà cung cấp thành công."));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission(PermissionConstants.SupplierEdit)]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> Update(
        Guid id, [FromBody] UpdateSupplierRequest request, CancellationToken ct)
    {
        var entity = await _uow.Suppliers.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Supplier", id);
        _mapper.Map(request, entity);
        entity.UpdatedBy = _currentUser.UserId;
        _uow.Suppliers.Update(entity);
        await _uow.SaveChangesAsync(ct);
        return Ok(ApiResponse<SupplierDto>.Ok(_mapper.Map<SupplierDto>(entity)));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(PermissionConstants.SupplierDelete)]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct)
    {
        var entity = await _uow.Suppliers.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Supplier", id);
        _uow.Suppliers.SoftDelete(entity);
        entity.DeletedBy = _currentUser.UserId;
        await _uow.SaveChangesAsync(ct);
        return Ok(ApiResponse.OkNoData("Xóa nhà cung cấp thành công."));
    }
}

// ─── Employees ────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public EmployeesController(IUnitOfWork uow, IMapper mapper, ICurrentUserService currentUser)
    {
        _uow = uow; _mapper = mapper; _currentUser = currentUser;
    }

    [HttpGet]
    [RequirePermission(PermissionConstants.EmployeeView)]
    public async Task<ActionResult<ApiResponse<PagedResult<EmployeeDto>>>> GetPaged(
        [FromQuery] PagedQuery query, CancellationToken ct)
    {
        var q = _uow.Employees.Query()
            .Include(e => e.Department)
            .Where(e => !e.IsDeleted);
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.ToLower();
            q = q.Where(e => e.FullName.ToLower().Contains(term) || e.EmployeeCode.ToLower().Contains(term));
        }
        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(e => e.FullName)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return Ok(ApiResponse<PagedResult<EmployeeDto>>.Ok(new PagedResult<EmployeeDto>
        {
            Items = _mapper.Map<IEnumerable<EmployeeDto>>(items),
            TotalCount = total, Page = query.Page, PageSize = query.PageSize
        }));
    }

    [HttpGet("{id:guid}")]
    [RequirePermission(PermissionConstants.EmployeeView)]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> GetById(Guid id, CancellationToken ct)
    {
        var entity = await _uow.Employees.Query()
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id, ct)
            ?? throw new NotFoundException("Employee", id);
        return Ok(ApiResponse<EmployeeDto>.Ok(_mapper.Map<EmployeeDto>(entity)));
    }

    [HttpPost]
    [RequirePermission(PermissionConstants.EmployeeCreate)]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> Create(
        [FromBody] CreateEmployeeRequest request, CancellationToken ct)
    {
        if (await _uow.Employees.AnyAsync(e => e.EmployeeCode == request.EmployeeCode, ct))
            throw new ConflictException($"Mã nhân viên '{request.EmployeeCode}' đã tồn tại.");
        var entity = _mapper.Map<Employee>(request);
        entity.CreatedBy = _currentUser.UserId;
        await _uow.Employees.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id },
            ApiResponse<EmployeeDto>.Ok(_mapper.Map<EmployeeDto>(entity), "Tạo nhân viên thành công."));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission(PermissionConstants.EmployeeEdit)]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> Update(
        Guid id, [FromBody] UpdateEmployeeRequest request, CancellationToken ct)
    {
        var entity = await _uow.Employees.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Employee", id);
        _mapper.Map(request, entity);
        entity.UpdatedBy = _currentUser.UserId;
        _uow.Employees.Update(entity);
        await _uow.SaveChangesAsync(ct);
        return Ok(ApiResponse<EmployeeDto>.Ok(_mapper.Map<EmployeeDto>(entity)));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(PermissionConstants.EmployeeDelete)]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct)
    {
        var entity = await _uow.Employees.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Employee", id);
        _uow.Employees.SoftDelete(entity);
        entity.DeletedBy = _currentUser.UserId;
        await _uow.SaveChangesAsync(ct);
        return Ok(ApiResponse.OkNoData("Xóa nhân viên thành công."));
    }
}

// ─── Return Orders ────────────────────────────────────────────────────────────

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReturnOrdersController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly Application.Common.Interfaces.ICodeGenerator _codeGen;

    public ReturnOrdersController(IUnitOfWork uow, IMapper mapper,
        ICurrentUserService currentUser, Application.Common.Interfaces.ICodeGenerator codeGen)
    {
        _uow = uow; _mapper = mapper; _currentUser = currentUser; _codeGen = codeGen;
    }

    [HttpGet]
    [RequirePermission(PermissionConstants.ReturnView)]
    public async Task<ActionResult<ApiResponse<PagedResult<ReturnOrderDto>>>> GetPaged(
        [FromQuery] PagedQuery query, CancellationToken ct)
    {
        var q = _uow.ReturnOrders.Query().Include(o => o.Lines).Where(o => !o.IsDeleted);
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.ToLower();
            q = q.Where(o => o.OrderCode.ToLower().Contains(term));
        }
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(o => o.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return Ok(ApiResponse<PagedResult<ReturnOrderDto>>.Ok(new PagedResult<ReturnOrderDto>
        {
            Items = _mapper.Map<IEnumerable<ReturnOrderDto>>(items),
            TotalCount = total, Page = query.Page, PageSize = query.PageSize
        }));
    }

    [HttpGet("{id:guid}")]
    [RequirePermission(PermissionConstants.ReturnView)]
    public async Task<ActionResult<ApiResponse<ReturnOrderDto>>> GetById(Guid id, CancellationToken ct)
    {
        var entity = await _uow.ReturnOrders.GetWithLinesAsync(id, ct)
            ?? throw new NotFoundException("ReturnOrder", id);
        return Ok(ApiResponse<ReturnOrderDto>.Ok(_mapper.Map<ReturnOrderDto>(entity)));
    }

    [HttpPost]
    [RequirePermission(PermissionConstants.ReturnCreate)]
    public async Task<ActionResult<ApiResponse<ReturnOrderDto>>> Create(
        [FromBody] CreateReturnOrderRequest request, CancellationToken ct)
    {
        if (!request.Lines.Any())
            throw new Application.Common.Exceptions.BusinessException("Phiếu thu hồi phải có ít nhất một dòng.");

        var order = new ReturnOrder
        {
            OrderCode = _codeGen.GenerateReturnCode(),
            Description = request.Description,
            Status = ReturnStatus.Draft,
            CreatedBy = _currentUser.UserId
        };
        foreach (var line in request.Lines)
            order.Lines.Add(new ReturnOrderLine
            {
                EmployeeId = line.EmployeeId, ItemId = line.ItemId,
                Quantity = line.Quantity, Condition = line.Condition,
                Notes = line.Notes, CreatedBy = _currentUser.UserId
            });

        await _uow.ReturnOrders.AddAsync(order, ct);
        await _uow.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = order.Id },
            ApiResponse<ReturnOrderDto>.Ok(_mapper.Map<ReturnOrderDto>(order), "Tạo phiếu thu hồi thành công."));
    }

    [HttpPost("{id:guid}/approve")]
    [RequirePermission(PermissionConstants.ReturnApprove)]
    public async Task<ActionResult<ApiResponse>> Approve(Guid id, CancellationToken ct)
    {
        var order = await _uow.ReturnOrders.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("ReturnOrder", id);
        if (order.Status != ReturnStatus.Draft)
            throw new Application.Common.Exceptions.BusinessException("Chỉ có thể duyệt phiếu ở trạng thái Nháp.");
        order.Status = ReturnStatus.Approved;
        order.ApprovedBy = _currentUser.UserId;
        order.ApprovedAt = DateTime.UtcNow;
        _uow.ReturnOrders.Update(order);
        await _uow.SaveChangesAsync(ct);
        return Ok(ApiResponse.OkNoData("Duyệt phiếu thu hồi thành công."));
    }

    [HttpPost("{id:guid}/complete")]
    [RequirePermission(PermissionConstants.ReturnEdit)]
    public async Task<ActionResult<ApiResponse>> Complete(Guid id, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var order = await _uow.ReturnOrders.GetWithLinesAsync(id, ct)
                ?? throw new NotFoundException("ReturnOrder", id);
            if (order.Status != ReturnStatus.Approved)
                throw new Application.Common.Exceptions.BusinessException("Chỉ có thể hoàn thành phiếu đã được duyệt.");

            foreach (var line in order.Lines)
            {
                var item = await _uow.UniformItems.GetByIdAsync(line.ItemId, ct)
                    ?? throw new NotFoundException("UniformItem", line.ItemId);
                var stockBefore = item.CurrentStock;
                item.CurrentStock += line.Quantity;
                item.UpdatedAt = DateTime.UtcNow;
                _uow.UniformItems.Update(item);

                await _uow.StockTransactions.AddAsync(new StockTransaction
                {
                    TransactionCode = _codeGen.GenerateStockTransactionCode(),
                    ItemId = item.Id,
                    Type = StockTransactionType.Return,
                    Quantity = line.Quantity,
                    StockBefore = stockBefore,
                    StockAfter = item.CurrentStock,
                    ReferenceCode = order.OrderCode,
                    ReferenceId = order.Id,
                    CreatedBy = _currentUser.UserId
                }, ct);
            }

            order.Status = ReturnStatus.Returned;
            order.ReturnedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            _uow.ReturnOrders.Update(order);
            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);
            return Ok(ApiResponse.OkNoData("Hoàn thành thu hồi thành công."));
        }
        catch { await _uow.RollbackTransactionAsync(ct); throw; }
    }
}

// ─── Purchase Orders ──────────────────────────────────────────────────────────

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly Application.Common.Interfaces.ICodeGenerator _codeGen;

    public PurchaseOrdersController(IUnitOfWork uow, IMapper mapper,
        ICurrentUserService currentUser, Application.Common.Interfaces.ICodeGenerator codeGen)
    {
        _uow = uow; _mapper = mapper; _currentUser = currentUser; _codeGen = codeGen;
    }

    [HttpGet]
    [RequirePermission(PermissionConstants.PurchaseView)]
    public async Task<ActionResult<ApiResponse<PagedResult<PurchaseOrderDto>>>> GetPaged(
        [FromQuery] PagedQuery query, CancellationToken ct)
    {
        var q = _uow.PurchaseOrders.Query()
            .Include(o => o.Supplier).Where(o => !o.IsDeleted);
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.ToLower();
            q = q.Where(o => o.OrderCode.ToLower().Contains(term));
        }
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(o => o.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return Ok(ApiResponse<PagedResult<PurchaseOrderDto>>.Ok(new PagedResult<PurchaseOrderDto>
        {
            Items = _mapper.Map<IEnumerable<PurchaseOrderDto>>(items),
            TotalCount = total, Page = query.Page, PageSize = query.PageSize
        }));
    }

    [HttpGet("{id:guid}")]
    [RequirePermission(PermissionConstants.PurchaseView)]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> GetById(Guid id, CancellationToken ct)
    {
        var entity = await _uow.PurchaseOrders.GetWithLinesAsync(id, ct)
            ?? throw new NotFoundException("PurchaseOrder", id);
        return Ok(ApiResponse<PurchaseOrderDto>.Ok(_mapper.Map<PurchaseOrderDto>(entity)));
    }

    [HttpPost]
    [RequirePermission(PermissionConstants.PurchaseCreate)]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> Create(
        [FromBody] CreatePurchaseOrderRequest request, CancellationToken ct)
    {
        if (!request.Lines.Any())
            throw new Application.Common.Exceptions.BusinessException("Đơn hàng phải có ít nhất một dòng.");

        var order = new PurchaseOrder
        {
            OrderCode = _codeGen.GeneratePurchaseCode(),
            SupplierId = request.SupplierId,
            Description = request.Description,
            OrderDate = request.OrderDate,
            ExpectedDate = request.ExpectedDate,
            Status = PurchaseOrderStatus.Draft,
            TotalAmount = request.Lines.Sum(l => l.Quantity * l.UnitPrice),
            CreatedBy = _currentUser.UserId
        };
        foreach (var line in request.Lines)
            order.Lines.Add(new PurchaseOrderLine
            {
                ItemId = line.ItemId,
                OrderedQuantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                CreatedBy = _currentUser.UserId
            });

        await _uow.PurchaseOrders.AddAsync(order, ct);
        await _uow.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = order.Id },
            ApiResponse<PurchaseOrderDto>.Ok(_mapper.Map<PurchaseOrderDto>(order), "Tạo đơn hàng thành công."));
    }

    [HttpPost("{id:guid}/approve")]
    [RequirePermission(PermissionConstants.PurchaseApprove)]
    public async Task<ActionResult<ApiResponse>> Approve(Guid id, CancellationToken ct)
    {
        var order = await _uow.PurchaseOrders.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("PurchaseOrder", id);
        if (order.Status != PurchaseOrderStatus.Draft)
            throw new Application.Common.Exceptions.BusinessException("Chỉ có thể duyệt đơn ở trạng thái Nháp.");
        order.Status = PurchaseOrderStatus.Ordered;
        order.UpdatedAt = DateTime.UtcNow;
        _uow.PurchaseOrders.Update(order);
        await _uow.SaveChangesAsync(ct);
        return Ok(ApiResponse.OkNoData("Duyệt đơn hàng thành công."));
    }

    [HttpPost("receive")]
    [RequirePermission(PermissionConstants.PurchaseEdit)]
    public async Task<ActionResult<ApiResponse>> Receive(
        [FromBody] ReceivePurchaseRequest request, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var order = await _uow.PurchaseOrders.GetWithLinesAsync(request.OrderId, ct)
                ?? throw new NotFoundException("PurchaseOrder", request.OrderId);

            if (order.Status is not (PurchaseOrderStatus.Ordered or PurchaseOrderStatus.PartialReceived))
                throw new Application.Common.Exceptions.BusinessException("Đơn hàng chưa được duyệt hoặc đã nhận đủ.");

            foreach (var update in request.ReceivedItems)
            {
                var line = order.Lines.FirstOrDefault(l => l.Id == update.LineId)
                    ?? throw new NotFoundException("PurchaseOrderLine", update.LineId);

                var item = await _uow.UniformItems.GetByIdAsync(line.ItemId, ct)
                    ?? throw new NotFoundException("UniformItem", line.ItemId);

                var stockBefore = item.CurrentStock;
                item.CurrentStock += update.ReceivedQuantity;
                item.UpdatedAt = DateTime.UtcNow;
                _uow.UniformItems.Update(item);

                line.ReceivedQuantity += update.ReceivedQuantity;
                _uow.PurchaseOrderLines.Update(line);

                await _uow.StockTransactions.AddAsync(new StockTransaction
                {
                    TransactionCode = _codeGen.GenerateStockTransactionCode(),
                    ItemId = item.Id,
                    Type = StockTransactionType.Import,
                    Quantity = update.ReceivedQuantity,
                    StockBefore = stockBefore,
                    StockAfter = item.CurrentStock,
                    ReferenceCode = order.OrderCode,
                    ReferenceId = order.Id,
                    CreatedBy = _currentUser.UserId
                }, ct);
            }

            bool allReceived = order.Lines.All(l => l.ReceivedQuantity >= l.OrderedQuantity);
            order.Status = allReceived ? PurchaseOrderStatus.FullReceived : PurchaseOrderStatus.PartialReceived;
            if (allReceived) order.ReceivedDate = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            _uow.PurchaseOrders.Update(order);
            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);
            return Ok(ApiResponse.OkNoData("Nhập kho thành công."));
        }
        catch { await _uow.RollbackTransactionAsync(ct); throw; }
    }
}
