using Ols.ControlCenter.Domain.Common;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Domain.Entities;

/// <summary>Operasyona ait tek bir evrak/checklist kalemi.</summary>
public class Document : AuditableEntity, ISoftDelete
{
    public long OperationId { get; set; }
    public Operation Operation { get; set; } = null!;

    public DocumentType DocType { get; set; }

    /// <summary>Checklist grubu (taşıma tipine göre: kara/deniz/hava).</summary>
    public TransportType ChecklistGroup { get; set; }

    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
    public bool IsRequired { get; set; } = true;
    public DateTimeOffset? ReceivedAt { get; set; }
    public string? Note { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
