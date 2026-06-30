using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ols.ControlCenter.Application.Abstractions.Security;
using Ols.ControlCenter.Application.Features.Users;
using Ols.ControlCenter.Shared.Api;
using Ols.ControlCenter.Shared.Authorization;

namespace Ols.ControlCenter.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = AppRoles.Admin)]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _users;
    private readonly ICurrentUser _current;

    public UsersController(IUserService users, ICurrentUser current)
    {
        _users = users;
        _current = current;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserListItemDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<UserListItemDto>>.Ok(await _users.GetUsersAsync(ct)));

    [HttpGet("roles")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RoleDto>>>> Roles(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<RoleDto>>.Ok(await _users.GetRolesAsync(ct)));

    [HttpGet("departments")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DepartmentDto>>>> Departments(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<DepartmentDto>>.Ok(await _users.GetDepartmentsAsync(ct)));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserListItemDto>>> Create([FromBody] CreateUserRequest req, CancellationToken ct)
    {
        var result = await _users.CreateAsync(req, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<UserListItemDto>.Ok(result.Value, "Kullanıcı oluşturuldu."))
            : BadRequest(ApiResponse<UserListItemDto>.Fail(result.Error.Message));
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse<UserListItemDto>>> Update(long id, [FromBody] UpdateUserRequest req, CancellationToken ct)
    {
        var result = await _users.UpdateAsync(id, req, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<UserListItemDto>.Ok(result.Value, "Kullanıcı güncellendi."))
            : BadRequest(ApiResponse<UserListItemDto>.Fail(result.Error.Message));
    }

    [HttpPost("{id:long}/reset-password")]
    public async Task<ActionResult<ApiResponse>> ResetPassword(long id, [FromBody] ResetPasswordRequest req, CancellationToken ct)
    {
        var result = await _users.ResetPasswordAsync(id, req.NewPassword, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Parola sıfırlandı. Kullanıcının yeniden giriş yapması gerekir."))
            : BadRequest(ApiResponse.Fail(result.Error.Message));
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResponse>> Delete(long id, CancellationToken ct)
    {
        var result = await _users.DeleteAsync(id, _current.UserId ?? 0, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Kullanıcı silindi."))
            : BadRequest(ApiResponse.Fail(result.Error.Message));
    }
}

public sealed class ResetPasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}
