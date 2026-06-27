using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Application.Abstractions.Persistence;
using Ols.ControlCenter.Domain.Enums;
using Ols.ControlCenter.Shared.Pagination;

namespace Ols.ControlCenter.Application.Features.Operations;

public interface IOperationQueryService
{
    Task<PagedResult<OperationListItemDto>> GetListAsync(OperationListRequest req, CancellationToken ct);
    Task<OperationDetailDto?> GetByIdAsync(long id, CancellationToken ct);
}

public sealed class OperationQueryService : IOperationQueryService
{
    private readonly IApplicationDbContext _db;

    public OperationQueryService(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<OperationListItemDto>> GetListAsync(OperationListRequest req, CancellationToken ct)
    {
        var q = _db.Operations.AsNoTracking();

        if (req.Transport is { } t) q = q.Where(o => o.TransportType == t);
        if (req.Status is { } s) q = q.Where(o => o.Status == s);
        if (req.Risk is { } r) q = q.Where(o => o.RiskLevel == r);

        q = req.Quick switch
        {
            "delayed" => q.Where(o => o.DelayDays > 0),
            "risky" => q.Where(o => o.RiskLevel == RiskLevel.Orange || o.RiskLevel == RiskLevel.Red || o.RiskLevel == RiskLevel.Black),
            "missingDocs" => q.Where(o => o.DocumentStatus == DocumentStatus.Missing || o.Status == OperationStatus.MissingDocuments),
            "financialHold" => q.Where(o => o.Status == OperationStatus.FinancialHold || o.FinanceStatus == FinanceStatus.FinancialHold),
            "completed" => q.Where(o => o.Status == OperationStatus.Completed),
            "active" => q.Where(o => o.Status != OperationStatus.Completed && o.Status != OperationStatus.Cancelled),
            "critical" => q.Where(o => o.Customer != null && o.Customer.IsCritical),
            _ => q
        };

        if (!string.IsNullOrWhiteSpace(req.Search))
        {
            var term = req.Search.Trim().ToLower();
            q = q.Where(o => o.CustomerName.ToLower().Contains(term)
                || (o.SourceOperationNo != null && o.SourceOperationNo.ToLower().Contains(term)));
        }

        var total = await q.CountAsync(ct);

        var raw = await q
            .OrderByDescending(o => o.CreatedAt)
            .Skip(req.Skip).Take(req.PageSize)
            .Select(o => new
            {
                o.Id,
                o.SourceOperationNo,
                o.TransportType,
                o.ServiceType,
                o.CustomerName,
                o.OriginCountry,
                o.OriginCity,
                o.DestinationCountry,
                o.DestinationCity,
                o.Status,
                o.RiskLevel,
                o.FinanceStatus,
                o.DocumentStatus,
                o.DelayDays,
                o.Eta,
                RespName = o.ResponsibleUser != null ? o.ResponsibleUser.FullName : null,
                DeptName = o.Department != null ? o.Department.Name : null,
                o.RevenueAmount,
                o.GrossProfit,
                o.Currency
            })
            .ToListAsync(ct);

        var items = raw.Select(o => new OperationListItemDto(
            o.Id, o.SourceOperationNo, o.TransportType.ToString(), o.ServiceType.ToString(), o.CustomerName,
            o.OriginCountry, o.OriginCity, o.DestinationCountry, o.DestinationCity,
            o.Status.ToString(), o.RiskLevel.ToString(), o.FinanceStatus.ToString(), o.DocumentStatus.ToString(),
            o.DelayDays, o.Eta, o.RespName, o.DeptName, o.RevenueAmount, o.GrossProfit, o.Currency)).ToList();

        return new PagedResult<OperationListItemDto>(items, total, req.Page, req.PageSize);
    }

    public async Task<OperationDetailDto?> GetByIdAsync(long id, CancellationToken ct)
    {
        var o = await _db.Operations.AsNoTracking()
            .Include(x => x.Detail)
            .Include(x => x.ResponsibleUser)
            .Include(x => x.SalesOwner)
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (o is null)
            return null;

        return new OperationDetailDto(
            o.Id, o.SourceOperationNo, o.TransportType.ToString(), o.ServiceType.ToString(), o.TradeDirection.ToString(),
            o.CustomerName, o.Shipper, o.Consignee,
            o.OriginCountry, o.OriginCity, o.DestinationCountry, o.DestinationCity,
            o.LoadingDate, o.Etd, o.Eta, o.ActualArrivalDate, o.PlannedDeliveryDate, o.DeliveryDate,
            o.Status.ToString(), o.RiskLevel.ToString(), o.FinanceStatus.ToString(), o.DocumentStatus.ToString(),
            o.ResponsibleUser?.FullName, o.SalesOwner?.FullName, o.Department?.Name,
            o.NextActionDate, o.NextActionDescription,
            o.DelayDays, o.DelayReason.ToString(),
            o.RevenueAmount, o.CostAmount, o.GrossProfit, o.Currency,
            o.Detail is null ? null : new OperationDetailInfo(
                o.Detail.BlNo, o.Detail.ContainerNo, o.Detail.ContainerType, o.Detail.ShippingLine, o.Detail.VesselName,
                o.Detail.Pol, o.Detail.Pod, o.Detail.TransshipmentPort, o.Detail.OrdinoStatus,
                o.Detail.FreeTimeEndDate, o.Detail.DemurrageStartDate,
                o.Detail.HawbNo, o.Detail.MawbNo, o.Detail.Airline, o.Detail.FlightNo, o.Detail.DepartureAirport, o.Detail.ArrivalAirport,
                o.Detail.Pieces, o.Detail.GrossWeightKg,
                o.Detail.VehiclePlate, o.Detail.DriverName, o.Detail.BorderCrossing, o.Detail.FillRate));
    }
}
