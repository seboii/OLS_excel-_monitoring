using Microsoft.AspNetCore.Mvc;
using Ols.ControlCenter.Application.Abstractions.Realtime;
using Ols.ControlCenter.Application.Abstractions.Security;
using Ols.ControlCenter.Application.Features.Tasks;
using Ols.ControlCenter.Shared.Api;
using Ols.ControlCenter.Shared.Pagination;

namespace Ols.ControlCenter.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public sealed class TasksController : ControllerBase
{
    private readonly ITaskService _tasks;
    private readonly ICurrentUser _current;
    private readonly IRealtimeNotifier _realtime;

    public TasksController(ITaskService tasks, ICurrentUser current, IRealtimeNotifier realtime)
    {
        _tasks = tasks;
        _current = current;
        _realtime = realtime;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<TaskDto>>>> GetList([FromQuery] TaskListRequest req, CancellationToken ct)
        => Ok(ApiResponse<PagedResult<TaskDto>>.Ok(await _tasks.GetListAsync(req, ct)));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TaskDto>>> Create([FromBody] CreateTaskRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Title))
            return BadRequest(ApiResponse<TaskDto>.Fail("Görev başlığı zorunludur."));
        var created = await _tasks.CreateAsync(req, _current.UserId, ct);
        await _realtime.NotifyAsync(RealtimeEvents.TasksChanged, ct: ct);
        return Ok(ApiResponse<TaskDto>.Ok(created, "Görev oluşturuldu."));
    }

    [HttpPost("{id:long}/complete")]
    public async Task<ActionResult<ApiResponse>> Complete(long id, [FromBody] CompleteTaskRequest req, CancellationToken ct)
    {
        if (!await _tasks.CompleteAsync(id, req.Note, _current.UserId, ct))
            return NotFound(ApiResponse.Fail("Görev bulunamadı."));
        await _realtime.NotifyAsync(RealtimeEvents.TasksChanged, ct: ct);
        return Ok(ApiResponse.Ok("Görev tamamlandı."));
    }

    [HttpPost("~/api/alerts/{alertId:long}/create-task")]
    public async Task<ActionResult<ApiResponse<TaskDto>>> CreateFromAlert(long alertId, CancellationToken ct)
    {
        var dto = await _tasks.CreateFromAlertAsync(alertId, _current.UserId, ct);
        if (dto is null)
            return NotFound(ApiResponse<TaskDto>.Fail("Uyarı bulunamadı."));
        await _realtime.NotifyAsync(RealtimeEvents.TasksChanged, ct: ct);
        await _realtime.NotifyAsync(RealtimeEvents.AlertsChanged, ct: ct);
        return Ok(ApiResponse<TaskDto>.Ok(dto, "Uyarıdan görev oluşturuldu."));
    }
}
