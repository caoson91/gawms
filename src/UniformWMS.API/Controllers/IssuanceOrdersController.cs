using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniformWMS.API.Middlewares;
using UniformWMS.Application.Common.Models;
using UniformWMS.Application.Features.IssuanceOrders;
using UniformWMS.Application.Features.IssuanceOrders.DTOs;
using UniformWMS.Shared.Constants;

namespace UniformWMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IssuanceOrdersController : ControllerBase
{
    private readonly IIssuanceOrderService _service;

    public IssuanceOrdersController(IIssuanceOrderService service) => _service = service;

    [HttpGet]
    [RequirePermission(PermissionConstants.IssuanceView)]
    public async Task<ActionResult<ApiResponse<PagedResult<IssuanceOrderDto>>>> GetPaged(
        [FromQuery] PagedQuery query, CancellationToken ct)
    {
        var result = await _service.GetPagedAsync(query, ct);
        return Ok(ApiResponse<PagedResult<IssuanceOrderDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequirePermission(PermissionConstants.IssuanceView)]
    public async Task<ActionResult<ApiResponse<IssuanceOrderDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<IssuanceOrderDto>.Ok(result));
    }

    [HttpPost]
    [RequirePermission(PermissionConstants.IssuanceCreate)]
    public async Task<ActionResult<ApiResponse<IssuanceOrderDto>>> Create(
        [FromBody] CreateIssuanceOrderRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<IssuanceOrderDto>.Ok(result, "Tạo phiếu cấp phát thành công."));
    }

    [HttpPost("{id:guid}/approve")]
    [RequirePermission(PermissionConstants.IssuanceApprove)]
    public async Task<ActionResult<ApiResponse>> Approve(Guid id, CancellationToken ct)
    {
        await _service.ApproveAsync(id, ct);
        return Ok(ApiResponse.OkNoData("Duyệt phiếu thành công."));
    }

    [HttpPost("issue")]
    [RequirePermission(PermissionConstants.IssuanceEdit)]
    public async Task<ActionResult<ApiResponse>> Issue(
        [FromBody] IssueIssuanceRequest request, CancellationToken ct)
    {
        await _service.IssueAsync(request, ct);
        return Ok(ApiResponse.OkNoData("Thực hiện cấp phát thành công."));
    }

    [HttpPost("{id:guid}/cancel")]
    [RequirePermission(PermissionConstants.IssuanceEdit)]
    public async Task<ActionResult<ApiResponse>> Cancel(Guid id, CancellationToken ct)
    {
        await _service.CancelAsync(id, ct);
        return Ok(ApiResponse.OkNoData("Hủy phiếu thành công."));
    }
}
