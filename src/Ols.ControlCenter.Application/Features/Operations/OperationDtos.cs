using Ols.ControlCenter.Domain.Enums;
using Ols.ControlCenter.Shared.Pagination;

namespace Ols.ControlCenter.Application.Features.Operations;

/// <summary>Operasyon listesi filtre + sayfalama girdisi.</summary>
public sealed class OperationListRequest : PagedRequest
{
    public TransportType? Transport { get; set; }
    public OperationStatus? Status { get; set; }
    public RiskLevel? Risk { get; set; }

    /// <summary>Dashboard kartı drill-down filtresi: delayed|risky|missingDocs|financialHold|completed|active|critical.</summary>
    public string? Quick { get; set; }
}

public sealed record OperationListItemDto(
    long Id,
    string? OperationNo,
    string TransportType,
    string ServiceType,
    string CustomerName,
    string? OriginCountry,
    string? OriginCity,
    string? DestinationCountry,
    string? DestinationCity,
    string Status,
    string RiskLevel,
    string FinanceStatus,
    string DocumentStatus,
    int DelayDays,
    DateTimeOffset? Eta,
    string? ResponsibleUserName,
    string? DepartmentName,
    decimal? RevenueAmount,
    decimal? GrossProfit,
    string Currency);

public sealed record OperationDetailDto(
    long Id,
    string? OperationNo,
    string TransportType,
    string ServiceType,
    string TradeDirection,
    string CustomerName,
    string? Shipper,
    string? Consignee,
    string? OriginCountry,
    string? OriginCity,
    string? DestinationCountry,
    string? DestinationCity,
    DateOnly? LoadingDate,
    DateTimeOffset? Etd,
    DateTimeOffset? Eta,
    DateTimeOffset? ActualArrivalDate,
    DateOnly? PlannedDeliveryDate,
    DateOnly? DeliveryDate,
    string Status,
    string RiskLevel,
    string FinanceStatus,
    string DocumentStatus,
    string? ResponsibleUserName,
    string? SalesOwnerName,
    string? DepartmentName,
    DateOnly? NextActionDate,
    string? NextActionDescription,
    int DelayDays,
    string DelayReason,
    decimal? RevenueAmount,
    decimal? CostAmount,
    decimal? GrossProfit,
    string Currency,
    OperationDetailInfo? Detail);

public sealed record OperationDetailInfo(
    string? BlNo,
    string? ContainerNo,
    string? ContainerType,
    string? ShippingLine,
    string? VesselName,
    string? Pol,
    string? Pod,
    string? TransshipmentPort,
    string? OrdinoStatus,
    DateOnly? FreeTimeEndDate,
    DateOnly? DemurrageStartDate,
    string? HawbNo,
    string? MawbNo,
    string? Airline,
    string? FlightNo,
    string? DepartureAirport,
    string? ArrivalAirport,
    int? Pieces,
    decimal? GrossWeightKg,
    string? VehiclePlate,
    string? DriverName,
    string? BorderCrossing,
    decimal? FillRate);
