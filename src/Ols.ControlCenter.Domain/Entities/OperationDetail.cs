using Ols.ControlCenter.Domain.Common;

namespace Ols.ControlCenter.Domain.Entities;

/// <summary>
/// Operasyona 1:1 bağlı, taşıma tipine özel detaylar (deniz/hava/karayolu).
/// Eşleşmeyen ek kaynak alanları <see cref="ExtraAttributes"/> (JSONB) içinde tutulur.
/// </summary>
public class OperationDetail : BaseEntity
{
    public long OperationId { get; set; }
    public Operation Operation { get; set; } = null!;

    // --- Deniz ---
    public string? BlNo { get; set; }
    public string? ContainerNo { get; set; }
    public string? ContainerType { get; set; }
    public string? ShippingLine { get; set; }
    public string? VesselName { get; set; }
    public string? Pol { get; set; }
    public string? Pod { get; set; }
    public string? TransshipmentPort { get; set; }
    public string? OrdinoStatus { get; set; }
    public DateOnly? FreeTimeEndDate { get; set; }
    public DateOnly? DemurrageStartDate { get; set; }
    public int? DemurrageRiskDays { get; set; }

    // --- Hava ---
    public string? HawbNo { get; set; }
    public string? MawbNo { get; set; }
    public string? Airline { get; set; }
    public string? FlightNo { get; set; }
    public string? DepartureAirport { get; set; }
    public string? ArrivalAirport { get; set; }
    public int? Pieces { get; set; }
    public decimal? GrossWeightKg { get; set; }
    public decimal? VolumeWeightKg { get; set; }
    public string? CargoType { get; set; }

    // --- Karayolu ---
    public string? VehiclePlate { get; set; }
    public string? DriverName { get; set; }
    public decimal? Ldm { get; set; }
    public decimal? VolumeM3 { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? FillRate { get; set; }
    public string? BorderCrossing { get; set; }

    /// <summary>Eşleşmemiş ek kaynak alanları (JSONB).</summary>
    public Dictionary<string, string> ExtraAttributes { get; set; } = new();
}
