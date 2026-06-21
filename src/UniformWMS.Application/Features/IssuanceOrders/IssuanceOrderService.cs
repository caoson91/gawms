using AutoMapper;
using UniformWMS.Application.Common.Exceptions;
using UniformWMS.Application.Common.Interfaces;
using UniformWMS.Application.Common.Models;
using UniformWMS.Application.Features.IssuanceOrders.DTOs;
using UniformWMS.Domain.Entities;
using UniformWMS.Domain.Enums;
using UniformWMS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace UniformWMS.Application.Features.IssuanceOrders;

public interface IIssuanceOrderService
{
    Task<PagedResult<IssuanceOrderDto>> GetPagedAsync(PagedQuery query, CancellationToken ct = default);
    Task<IssuanceOrderDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IssuanceOrderDto> CreateAsync(CreateIssuanceOrderRequest request, CancellationToken ct = default);
    Task ApproveAsync(Guid id, CancellationToken ct = default);
    Task IssueAsync(IssueIssuanceRequest request, CancellationToken ct = default);
    Task CancelAsync(Guid id, CancellationToken ct = default);
}

public class IssuanceOrderService : IIssuanceOrderService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly ICodeGenerator _codeGen;

    public IssuanceOrderService(IUnitOfWork uow, IMapper mapper,
        ICurrentUserService currentUser, ICodeGenerator codeGen)
    {
        _uow = uow;
        _mapper = mapper;
        _currentUser = currentUser;
        _codeGen = codeGen;
    }

    public async Task<PagedResult<IssuanceOrderDto>> GetPagedAsync(PagedQuery query, CancellationToken ct = default)
    {
        var q = _uow.IssuanceOrders.Query()
            .Include(o => o.Lines)
            .Where(o => !o.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim().ToLower();
            q = q.Where(o => o.OrderCode.ToLower().Contains(term));
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(o => o.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return new PagedResult<IssuanceOrderDto>
        {
            Items = _mapper.Map<IEnumerable<IssuanceOrderDto>>(items),
            TotalCount = total, Page = query.Page, PageSize = query.PageSize
        };
    }

    public async Task<IssuanceOrderDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _uow.IssuanceOrders.GetWithLinesAsync(id, ct)
            ?? throw new NotFoundException("IssuanceOrder", id);

        return _mapper.Map<IssuanceOrderDto>(order);
    }

    public async Task<IssuanceOrderDto> CreateAsync(CreateIssuanceOrderRequest request, CancellationToken ct = default)
    {
        if (!request.Lines.Any())
            throw new BusinessException("Phiếu cấp phát phải có ít nhất một dòng.");

        var order = new IssuanceOrder
        {
            OrderCode = _codeGen.GenerateIssuanceCode(),
            Description = request.Description,
            Status = IssuanceStatus.Draft,
            CreatedBy = _currentUser.UserId
        };

        foreach (var line in request.Lines)
        {
            order.Lines.Add(new IssuanceOrderLine
            {
                EmployeeId = line.EmployeeId,
                ItemId = line.ItemId,
                Quantity = line.Quantity,
                ActualQuantity = line.Quantity,
                Notes = line.Notes,
                CreatedBy = _currentUser.UserId
            });
        }

        await _uow.IssuanceOrders.AddAsync(order, ct);
        await _uow.SaveChangesAsync(ct);
        return await GetByIdAsync(order.Id, ct);
    }

    public async Task ApproveAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _uow.IssuanceOrders.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("IssuanceOrder", id);

        if (order.Status != IssuanceStatus.Draft)
            throw new BusinessException("Chỉ có thể duyệt phiếu ở trạng thái Nháp.");

        order.Status = IssuanceStatus.Approved;
        order.ApprovedBy = _currentUser.UserId;
        order.ApprovedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;
        _uow.IssuanceOrders.Update(order);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task IssueAsync(IssueIssuanceRequest request, CancellationToken ct = default)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var order = await _uow.IssuanceOrders.GetWithLinesAsync(request.OrderId, ct)
                ?? throw new NotFoundException("IssuanceOrder", request.OrderId);

            if (order.Status != IssuanceStatus.Approved)
                throw new BusinessException("Chỉ có thể thực hiện cấp phát với phiếu đã được duyệt.");

            foreach (var update in request.ActualQuantities)
            {
                var line = order.Lines.FirstOrDefault(l => l.Id == update.LineId)
                    ?? throw new NotFoundException("IssuanceOrderLine", update.LineId);

                var item = await _uow.UniformItems.GetByIdAsync(line.ItemId, ct)
                    ?? throw new NotFoundException("UniformItem", line.ItemId);

                if (item.CurrentStock < update.ActualQuantity)
                    throw new BusinessException($"Tồn kho của '{item.Name}' không đủ. Hiện có: {item.CurrentStock}, yêu cầu: {update.ActualQuantity}");

                var stockBefore = item.CurrentStock;
                item.CurrentStock -= update.ActualQuantity;
                item.UpdatedAt = DateTime.UtcNow;
                _uow.UniformItems.Update(item);

                line.ActualQuantity = update.ActualQuantity;
                _uow.IssuanceOrderLines.Update(line);

                await _uow.StockTransactions.AddAsync(new StockTransaction
                {
                    TransactionCode = _codeGen.GenerateStockTransactionCode(),
                    ItemId = item.Id,
                    Type = StockTransactionType.Export,
                    Quantity = -update.ActualQuantity,
                    StockBefore = stockBefore,
                    StockAfter = item.CurrentStock,
                    ReferenceCode = order.OrderCode,
                    ReferenceId = order.Id,
                    CreatedBy = _currentUser.UserId
                }, ct);
            }

            order.Status = IssuanceStatus.Issued;
            order.IssuedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            _uow.IssuanceOrders.Update(order);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    public async Task CancelAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _uow.IssuanceOrders.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("IssuanceOrder", id);

        if (order.Status == IssuanceStatus.Issued)
            throw new BusinessException("Không thể hủy phiếu đã cấp phát.");

        order.Status = IssuanceStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;
        _uow.IssuanceOrders.Update(order);
        await _uow.SaveChangesAsync(ct);
    }
}
