using Microsoft.AspNetCore.Mvc;
using Ols.ControlCenter.Application.Features.Operations;
using Ols.ControlCenter.Domain.Enums;
using Ols.ControlCenter.Shared.Api;
using Ols.ControlCenter.Shared.Pagination;

namespace Ols.ControlCenter.Api.Controllers;

[ApiController]
[Route("api/operations")]
public sealed class OperationsController : ControllerBase
{
    private readonly IOperationQueryService _ops;

    public OperationsController(IOperationQueryService ops) => _ops = ops;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<OperationListItemDto>>>> GetList(
        [FromQuery] OperationListRequest req, CancellationToken ct)
        => Ok(ApiResponse<PagedResult<OperationListItemDto>>.Ok(await _ops.GetListAsync(req, ct)));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<OperationDetailDto>>> GetById(long id, CancellationToken ct)
    {
        var dto = await _ops.GetByIdAsync(id, ct);
        return dto is null
            ? NotFound(ApiResponse<OperationDetailDto>.Fail("Operasyon bulunamadı."))
            : Ok(ApiResponse<OperationDetailDto>.Ok(dto));
    }

    [HttpGet("road")]
    public Task<ActionResult<ApiResponse<PagedResult<OperationListItemDto>>>> Road(
        [FromQuery] OperationListRequest req, CancellationToken ct)
    {
        req.Transport = TransportType.Road;
        return GetList(req, ct);
    }

    [HttpGet("sea")]
    public Task<ActionResult<ApiResponse<PagedResult<OperationListItemDto>>>> Sea(
        [FromQuery] OperationListRequest req, CancellationToken ct)
    {
        req.Transport = TransportType.Sea;
        return GetList(req, ct);
    }

    [HttpGet("air")]
    public Task<ActionResult<ApiResponse<PagedResult<OperationListItemDto>>>> Air(
        [FromQuery] OperationListRequest req, CancellationToken ct)
    {
        req.Transport = TransportType.Air;
        return GetList(req, ct);
    }

    [HttpGet("finance")]
    public Task<ActionResult<ApiResponse<PagedResult<OperationListItemDto>>>> Finance(
        [FromQuery] OperationListRequest req, CancellationToken ct)
        => GetList(req, ct);
}
