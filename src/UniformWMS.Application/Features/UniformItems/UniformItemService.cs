using AutoMapper;
using UniformWMS.Application.Common.Exceptions;
using UniformWMS.Application.Common.Interfaces;
using UniformWMS.Application.Common.Models;
using UniformWMS.Application.Features.UniformItems.DTOs;
using UniformWMS.Domain.Entities;
using UniformWMS.Domain.Enums;
using UniformWMS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace UniformWMS.Application.Features.UniformItems;

public interface IUniformItemService
{
    Task<PagedResult<UniformItemDto>> GetPagedAsync(PagedQuery query, CancellationToken ct = default);
    Task<UniformItemDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<UniformItemDto> CreateAsync(CreateUniformItemRequest request, CancellationToken ct = default);
    Task<UniformItemDto> UpdateAsync(Guid id, UpdateUniformItemRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task AdjustStockAsync(AdjustStockRequest request, CancellationToken ct = default);
    Task<IEnumerable<UniformItemDto>> GetLowStockAsync(CancellationToken ct = default);
}

public class UniformItemService : IUniformItemService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly ICodeGenerator _codeGen;

    public UniformItemService(IUnitOfWork uow, IMapper mapper,
        ICurrentUserService currentUser, ICodeGenerator codeGen)
    {
        _uow = uow;
        _mapper = mapper;
        _currentUser = currentUser;
        _codeGen = codeGen;
    }

    public async Task<PagedResult<UniformItemDto>> GetPagedAsync(PagedQuery query, CancellationToken ct = default)
    {
        var q = _uow.UniformItems.Query()
            .Include(i => i.Category)
            .Where(i => !i.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim().ToLower();
            q = q.Where(i => i.Name.ToLower().Contains(term) || i.ItemCode.ToLower().Contains(term));
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderBy(i => i.ItemCode)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return new PagedResult<UniformItemDto>
        {
            Items = _mapper.Map<IEnumerable<UniformItemDto>>(items),
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<UniformItemDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var item = await _uow.UniformItems.Query()
            .Include(i => i.Category)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted, ct)
            ?? throw new NotFoundException("UniformItem", id);

        return _mapper.Map<UniformItemDto>(item);
    }

    public async Task<UniformItemDto> CreateAsync(CreateUniformItemRequest request, CancellationToken ct = default)
    {
        if (await _uow.UniformItems.IsItemCodeExistsAsync(request.ItemCode, null, ct))
            throw new ConflictException($"Mã hàng '{request.ItemCode}' đã tồn tại.");

        var item = _mapper.Map<UniformItem>(request);
        item.CreatedBy = _currentUser.UserId;

        await _uow.UniformItems.AddAsync(item, ct);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(item.Id, ct);
    }

    public async Task<UniformItemDto> UpdateAsync(Guid id, UpdateUniformItemRequest request, CancellationToken ct = default)
    {
        var item = await _uow.UniformItems.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("UniformItem", id);

        _mapper.Map(request, item);
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = _currentUser.UserId;

        _uow.UniformItems.Update(item);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var item = await _uow.UniformItems.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("UniformItem", id);

        _uow.UniformItems.SoftDelete(item);
        item.DeletedBy = _currentUser.UserId;
        await _uow.SaveChangesAsync(ct);
    }

    public async Task AdjustStockAsync(AdjustStockRequest request, CancellationToken ct = default)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var item = await _uow.UniformItems.GetByIdAsync(request.ItemId, ct)
                ?? throw new NotFoundException("UniformItem", request.ItemId);

            var stockBefore = item.CurrentStock;
            item.CurrentStock += request.Quantity;

            if (item.CurrentStock < 0)
                throw new BusinessException("Tồn kho không thể âm.");

            item.UpdatedAt = DateTime.UtcNow;
            item.UpdatedBy = _currentUser.UserId;
            _uow.UniformItems.Update(item);

            var txType = request.Quantity > 0 ? StockTransactionType.Adjustment : StockTransactionType.Adjustment;
            var tx = new StockTransaction
            {
                TransactionCode = _codeGen.GenerateStockTransactionCode(),
                ItemId = item.Id,
                Type = txType,
                Quantity = request.Quantity,
                StockBefore = stockBefore,
                StockAfter = item.CurrentStock,
                Notes = request.Notes,
                CreatedBy = _currentUser.UserId
            };

            await _uow.StockTransactions.AddAsync(tx, ct);
            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    public async Task<IEnumerable<UniformItemDto>> GetLowStockAsync(CancellationToken ct = default)
    {
        var items = await _uow.UniformItems.GetLowStockItemsAsync(ct);
        return _mapper.Map<IEnumerable<UniformItemDto>>(items);
    }
}
