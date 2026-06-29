using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Application.Abstractions.Persistence;
using Ols.ControlCenter.Application.Features.Boards;
using Ols.ControlCenter.Domain.Entities;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Application.Features.Comments;

public sealed record CommentDto(
    long Id, long? OperationId, string? BoardKey, string? BoardTitle, string? Group, string? RecordRef,
    string AuthorName, string Type, string Body, List<string> Mentions, DateTimeOffset CreatedAt);

public sealed class CreateCommentRequest
{
    public string Type { get; set; } = "General";
    public string Body { get; set; } = string.Empty;
    public List<string>? Mentions { get; set; }
}

public interface ICommentService
{
    Task<IReadOnlyList<CommentDto>> GetByOperationAsync(long operationId, CancellationToken ct);
    Task<CommentDto?> AddAsync(long operationId, CreateCommentRequest req, long userId, CancellationToken ct);

    Task<IReadOnlyList<CommentDto>> GetByBoardRowAsync(string boardKey, string recordRef, CancellationToken ct);
    Task<CommentDto?> AddToBoardRowAsync(string boardKey, string recordRef, CreateCommentRequest req, long userId, CancellationToken ct);

    Task<bool> CancelAsync(long commentId, long userId, CancellationToken ct);
}

public sealed class CommentService : ICommentService
{
    private readonly IApplicationDbContext _db;

    public CommentService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<CommentDto>> GetByOperationAsync(long operationId, CancellationToken ct)
    {
        var raw = await _db.Comments.AsNoTracking()
            .Where(c => c.OperationId == operationId && !c.IsCancelled)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new { c.Id, c.OperationId, Author = c.Author.FullName, c.Type, c.Body, c.Mentions, c.CreatedAt })
            .ToListAsync(ct);

        return raw.Select(c => new CommentDto(
            c.Id, c.OperationId, null, null, null, null, c.Author, c.Type.ToString(), c.Body, c.Mentions, c.CreatedAt)).ToList();
    }

    public async Task<CommentDto?> AddAsync(long operationId, CreateCommentRequest req, long userId, CancellationToken ct)
    {
        var op = await _db.Operations.FirstOrDefaultAsync(o => o.Id == operationId, ct);
        if (op is null) return null;

        var now = DateTimeOffset.UtcNow;
        var type = Enum.TryParse<CommentType>(req.Type, ignoreCase: true, out var t) ? t : CommentType.General;
        var comment = new Comment
        {
            OperationId = operationId,
            AuthorUserId = userId,
            Type = type,
            Body = req.Body,
            Mentions = req.Mentions ?? new List<string>(),
            CreatedAt = now,
            CreatedByUserId = userId,
        };
        _db.Comments.Add(comment);

        // İzleme zaman damgaları — risk motorunu besler
        op.LastInternalCommentDate = now;
        if (type == CommentType.CustomerInfo) op.LastCustomerUpdateDate = now;

        await _db.SaveChangesAsync(ct);

        var author = await _db.Users.Where(u => u.Id == userId).Select(u => u.FullName).FirstOrDefaultAsync(ct) ?? "—";
        return new CommentDto(comment.Id, operationId, null, null, null, null, author, type.ToString(), comment.Body, comment.Mentions, now);
    }

    public async Task<IReadOnlyList<CommentDto>> GetByBoardRowAsync(string boardKey, string recordRef, CancellationToken ct)
    {
        var raw = await _db.Comments.AsNoTracking()
            .Where(c => c.BoardKey == boardKey && c.RecordRef == recordRef && !c.IsCancelled)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new
            {
                c.Id, c.BoardKey, c.BoardTitle, c.Group, c.RecordRef,
                Author = c.Author.FullName, c.Type, c.Body, c.Mentions, c.CreatedAt,
            })
            .ToListAsync(ct);

        return raw.Select(c => new CommentDto(
            c.Id, null, c.BoardKey, c.BoardTitle, c.Group, c.RecordRef, c.Author, c.Type.ToString(), c.Body, c.Mentions, c.CreatedAt)).ToList();
    }

    public async Task<CommentDto?> AddToBoardRowAsync(string boardKey, string recordRef, CreateCommentRequest req, long userId, CancellationToken ct)
    {
        var meta = BoardCatalog.Find(boardKey);
        if (meta is null) return null;

        var now = DateTimeOffset.UtcNow;
        var type = Enum.TryParse<CommentType>(req.Type, ignoreCase: true, out var t) ? t : CommentType.General;
        var comment = new Comment
        {
            BoardKey = meta.Key,
            BoardTitle = meta.Title,
            Group = meta.Group,
            RecordRef = recordRef,
            AuthorUserId = userId,
            Type = type,
            Body = req.Body,
            Mentions = req.Mentions ?? new List<string>(),
            CreatedAt = now,
            CreatedByUserId = userId,
        };
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync(ct);

        var author = await _db.Users.Where(u => u.Id == userId).Select(u => u.FullName).FirstOrDefaultAsync(ct) ?? "—";
        return new CommentDto(
            comment.Id, null, meta.Key, meta.Title, meta.Group, recordRef, author, type.ToString(), comment.Body, comment.Mentions, now);
    }

    public async Task<bool> CancelAsync(long commentId, long userId, CancellationToken ct)
    {
        var comment = await _db.Comments.FirstOrDefaultAsync(c => c.Id == commentId, ct);
        if (comment is null) return false;
        comment.IsCancelled = true;
        comment.CancelledAt = DateTimeOffset.UtcNow;
        comment.CancelledByUserId = userId;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
