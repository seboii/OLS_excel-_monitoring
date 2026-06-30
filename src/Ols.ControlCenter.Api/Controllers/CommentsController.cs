using Microsoft.AspNetCore.Mvc;
using Ols.ControlCenter.Application.Abstractions.Realtime;
using Ols.ControlCenter.Application.Abstractions.Security;
using Ols.ControlCenter.Application.Features.Comments;
using Ols.ControlCenter.Shared.Api;

namespace Ols.ControlCenter.Api.Controllers;

[ApiController]
public sealed class CommentsController : ControllerBase
{
    private readonly ICommentService _comments;
    private readonly ICurrentUser _current;
    private readonly IRealtimeNotifier _realtime;

    public CommentsController(ICommentService comments, ICurrentUser current, IRealtimeNotifier realtime)
    {
        _comments = comments;
        _current = current;
        _realtime = realtime;
    }

    [HttpGet("api/operations/{operationId:long}/comments")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CommentDto>>>> GetByOperation(long operationId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CommentDto>>.Ok(await _comments.GetByOperationAsync(operationId, ct)));

    [HttpPost("api/operations/{operationId:long}/comments")]
    public async Task<ActionResult<ApiResponse<CommentDto>>> Add(long operationId, [FromBody] CreateCommentRequest req, CancellationToken ct)
    {
        if (_current.UserId is not { } userId)
            return Unauthorized(ApiResponse<CommentDto>.Fail("Oturum bulunamadı."));
        if (string.IsNullOrWhiteSpace(req.Body))
            return BadRequest(ApiResponse<CommentDto>.Fail("Yorum boş olamaz."));

        var dto = await _comments.AddAsync(operationId, req, userId, ct);
        if (dto is null)
            return NotFound(ApiResponse<CommentDto>.Fail("Operasyon bulunamadı."));
        await _realtime.NotifyAsync(RealtimeEvents.CommentsChanged, new { operationId }, ct);
        return Ok(ApiResponse<CommentDto>.Ok(dto, "Yorum eklendi."));
    }

    [HttpGet("api/comments/recent")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CommentDto>>>> GetRecent(
        [FromQuery] string? group, [FromQuery] int take = 50, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<CommentDto>>.Ok(await _comments.GetRecentAsync(group, take, ct)));

    [HttpGet("api/boards/{boardKey}/comments")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CommentDto>>>> GetByBoardRow(
        string boardKey, [FromQuery] string @ref, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CommentDto>>.Ok(await _comments.GetByBoardRowAsync(boardKey, @ref, ct)));

    [HttpPost("api/boards/{boardKey}/comments")]
    public async Task<ActionResult<ApiResponse<CommentDto>>> AddToBoardRow(
        string boardKey, [FromQuery] string @ref, [FromBody] CreateCommentRequest req, CancellationToken ct)
    {
        if (_current.UserId is not { } userId)
            return Unauthorized(ApiResponse<CommentDto>.Fail("Oturum bulunamadı."));
        if (string.IsNullOrWhiteSpace(req.Body))
            return BadRequest(ApiResponse<CommentDto>.Fail("Yorum boş olamaz."));

        var dto = await _comments.AddToBoardRowAsync(boardKey, @ref, req, userId, ct);
        if (dto is null)
            return NotFound(ApiResponse<CommentDto>.Fail("Sekme bulunamadı."));
        await _realtime.NotifyAsync(RealtimeEvents.CommentsChanged, new { boardKey, @ref }, ct);
        return Ok(ApiResponse<CommentDto>.Ok(dto, "Yorum eklendi."));
    }

    [HttpPut("api/comments/{id:long}/cancel")]
    public async Task<ActionResult<ApiResponse>> Cancel(long id, CancellationToken ct)
    {
        if (_current.UserId is not { } userId)
            return Unauthorized(ApiResponse.Fail("Oturum bulunamadı."));
        if (!await _comments.CancelAsync(id, userId, ct))
            return NotFound(ApiResponse.Fail("Yorum bulunamadı."));
        await _realtime.NotifyAsync(RealtimeEvents.CommentsChanged, ct: ct);
        return Ok(ApiResponse.Ok("Yorum iptal edildi."));
    }
}
