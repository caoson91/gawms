using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniformWMS.API.Middlewares;
using UniformWMS.Application.Common.Models;
using UniformWMS.Application.Features.UniformItems;
using UniformWMS.Application.Features.UniformItems.DTOs;
using UniformWMS.Shared.Constants;

namespace UniformWMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UniformItemsController : ControllerBase
{
    private readonly IUniformItemService _service;

    public UniformItemsController(IUniformItemService service) => _service = service;

    [HttpGet]
    [RequirePermission(PermissionConstants.UniformView)]
    public async Task<ActionResult<ApiResponse<PagedResult<UniformItemDto>>>> GetPaged(
        [FromQuery] PagedQuery query, CancellationToken ct)
    {
        var result = await _service.GetPagedAsync(query, ct);
        return Ok(ApiResponse<PagedResult<UniformItemDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequirePermission(PermissionConstants.UniformView)]
    public async Task<ActionResult<ApiResponse<UniformItemDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<UniformItemDto>.Ok(result));
    }

    [HttpGet("low-stock")]
    [RequirePermission(PermissionConstants.UniformView)]
    public async Task<ActionResult<ApiResponse<IEnumerable<UniformItemDto>>>> GetLowStock(CancellationToken ct)
    {
        var result = await _service.GetLowStockAsync(ct);
        return Ok(ApiResponse<IEnumerable<UniformItemDto>>.Ok(result));
    }

    [HttpPost]
    [RequirePermission(PermissionConstants.UniformCreate)]
    public async Task<ActionResult<ApiResponse<UniformItemDto>>> Create(
        [FromBody] CreateUniformItemRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<UniformItemDto>.Ok(result, "Tạo mặt hàng thành công."));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission(PermissionConstants.UniformEdit)]
    public async Task<ActionResult<ApiResponse<UniformItemDto>>> Update(
        Guid id, [FromBody] UpdateUniformItemRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<UniformItemDto>.Ok(result, "Cập nhật thành công."));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(PermissionConstants.UniformDelete)]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return Ok(ApiResponse.OkNoData("Xóa mặt hàng thành công."));
    }

    [HttpPost("adjust-stock")]
    [RequirePermission(PermissionConstants.StockView)]
    public async Task<ActionResult<ApiResponse>> AdjustStock(
        [FromBody] AdjustStockRequest request, CancellationToken ct)
    {
        await _service.AdjustStockAsync(request, ct);
        return Ok(ApiResponse.OkNoData("Điều chỉnh tồn kho thành công."));
    }
}
