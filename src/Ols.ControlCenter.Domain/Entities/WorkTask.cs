using Ols.ControlCenter.Domain.Common;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Domain.Entities;

/// <summary>Görev. (System.Threading.Tasks.Task ile çakışmaması için "WorkTask".)</summary>
public class WorkTask : AuditableEntity, ISoftDelete
{
    public string Title { get; set; } = string.Empty;

    public long? OperationId { get; set; }
    public Operation? Operation { get; set; }

    public long? OwnerUserId { get; set; }
    public User? Owner { get; set; }

    public long? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Normal;
    public DateOnly? DueDate { get; set; }
    public WorkTaskStatus Status { get; set; } = WorkTaskStatus.New;

    public string? Description { get; set; }
    public string? CompletionNote { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>Bir uyarıdan oluşturulduysa kaynak uyarı.</summary>
    public long? SourceAlertId { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
