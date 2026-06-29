using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Application.Abstractions.DataIntegration;
using Ols.ControlCenter.Application.Abstractions.Persistence;
using Ols.ControlCenter.Domain.Entities;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Infrastructure.DataIntegration;

/// <summary>
/// Kaynak satırlarını standart <see cref="Operation"/> modeline upsert eden motor.
/// Operasyonlar (SourceId + SourceOperationNo) ikilisiyle tekilleştirilir; eşleşmeyen kolonlar
/// <see cref="OperationDetail.ExtraAttributes"/>'a yazılır (veri kaybı olmaz). DbContext'e ekler,
/// kaydetmez — bkz. <see cref="IOperationUpsertService"/>.
/// </summary>
public sealed class OperationUpsertService : IOperationUpsertService
{
    private readonly IApplicationDbContext _db;

    public OperationUpsertService(IApplicationDbContext db) => _db = db;

    public async Task<UpsertSummary> UpsertAsync(
        DataSource source,
        IReadOnlyList<IReadOnlyDictionary<string, string?>> rows,
        IReadOnlyList<DataSourceColumnMapping> mappings,
        IReadOnlyDictionary<string, OperationStatus> statusMap,
        long? userId,
        CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dataSourceId = source.Id;

        var opNoColumn = mappings.FirstOrDefault(m => Canon(m.TargetField) == "operationno")?.SourceColumn;
        var mappedSourceColumns = mappings.Select(m => m.SourceColumn).ToHashSet(StringComparer.OrdinalIgnoreCase);

        int upserted = 0, failed = 0;
        var errors = new List<string>();
        var rawRows = new List<ImportedRawRow>();
        var batch = new Dictionary<string, Operation>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            try
            {
                var sourceNo = opNoColumn is not null ? GetValue(row, opNoColumn) : null;
                if (string.IsNullOrWhiteSpace(sourceNo))
                {
                    failed++;
                    if (errors.Count < 20) errors.Add($"Satır {i + 1}: operasyon numarası boş olduğu için satır aktarılmadı.");
                    rawRows.Add(MakeRawRow(dataSourceId, i, row, "Operasyon numarası boş.", now));
                    continue;
                }

                Operation op;
                bool preExisting = false;
                OperationStatus? statusBefore = null;

                if (batch.TryGetValue(sourceNo, out var tracked))
                {
                    op = tracked; // aynı parti içinde tekrar eden operasyon no → aynı kaydı güncelle (mükerrer insert yok)
                }
                else
                {
                    var existing = await _db.Operations.Include(o => o.Detail)
                        .FirstOrDefaultAsync(o => o.SourceId == dataSourceId && o.SourceOperationNo == sourceNo, ct);
                    if (existing is not null)
                    {
                        op = existing;
                        op.Detail ??= new OperationDetail();
                        preExisting = true;
                        statusBefore = existing.Status;
                    }
                    else
                    {
                        op = new Operation
                        {
                            SourceId = dataSourceId,
                            SourceOperationNo = sourceNo,
                            CustomerName = string.Empty,
                            TransportType = source.DefaultTransportType ?? TransportType.Other,
                            CreatedAt = now,
                            CreatedByUserId = userId,
                            Detail = new OperationDetail(),
                        };
                        _db.Operations.Add(op);
                    }
                    batch[sourceNo] = op;
                }

                ApplyMapping(op, op.Detail!, row, mappings, statusMap);
                CaptureUnmapped(op.Detail!, row, mappedSourceColumns);

                op.UpdatedAt = now;
                op.UpdatedByUserId = userId;
                op.RecomputeDerived(today);

                if (preExisting && statusBefore != op.Status)
                {
                    _db.StatusHistories.Add(new StatusHistory
                    {
                        Operation = op,
                        FromStatus = statusBefore,
                        ToStatus = op.Status,
                        Source = "sync",
                        ChangedAt = now,
                    });
                }

                upserted++;
            }
            catch (Exception ex)
            {
                failed++;
                if (errors.Count < 20) errors.Add($"Satır {i + 1}: {ex.Message}");
                rawRows.Add(MakeRawRow(dataSourceId, i, row, ex.Message, now));
            }
        }

        return new UpsertSummary(upserted, failed, errors, rawRows);
    }

    private static string? GetValue(IReadOnlyDictionary<string, string?> row, string column)
        => row.TryGetValue(column, out var v) ? v : null;

    private static ImportedRawRow MakeRawRow(long dataSourceId, int index, IReadOnlyDictionary<string, string?> row, string error, DateTimeOffset now)
        => new()
        {
            DataSourceId = dataSourceId,
            RowIndex = index,
            RawJson = JsonSerializer.Serialize(row),
            IsImported = false,
            ErrorMessage = error.Length > 1000 ? error[..1000] : error,
            CreatedAt = now,
        };

    /// <summary>Hiçbir mapping'e denk gelmeyen kaynak kolonları detayın ExtraAttributes'ına yazar (veri kaybı olmaz).</summary>
    private static void CaptureUnmapped(OperationDetail detail, IReadOnlyDictionary<string, string?> row, HashSet<string> mappedSourceColumns)
    {
        foreach (var (key, value) in row)
        {
            if (string.IsNullOrWhiteSpace(value)) continue;
            if (mappedSourceColumns.Contains(key)) continue;
            detail.ExtraAttributes[key] = value!;
        }
    }

    private static void ApplyMapping(
        Operation op, OperationDetail d,
        IReadOnlyDictionary<string, string?> row,
        IReadOnlyList<DataSourceColumnMapping> mappings,
        IReadOnlyDictionary<string, OperationStatus> statusMap)
    {
        foreach (var m in mappings)
        {
            var raw = GetValue(row, m.SourceColumn);
            if (string.IsNullOrWhiteSpace(raw)) raw = m.DefaultValue;
            if (string.IsNullOrWhiteSpace(raw)) continue;
            var v = raw.Trim();

            switch (Canon(m.TargetField))
            {
                case "operationno": op.SourceOperationNo = v; break;
                case "customername": case "customer": case "client": op.CustomerName = v; break;
                case "shipper": op.Shipper = v; break;
                case "consignee": op.Consignee = v; break;
                case "origincountry": op.OriginCountry = v; break;
                case "origincity": op.OriginCity = v; break;
                case "destinationcountry": op.DestinationCountry = v; break;
                case "destinationcity": op.DestinationCity = v; break;
                case "status": op.Status = DataNormalizer.NormalizeStatus(v, statusMap); break;
                case "transporttype": op.TransportType = DataNormalizer.ParseEnum(v, op.TransportType); break;
                case "servicetype": op.ServiceType = DataNormalizer.ParseEnum(v, op.ServiceType); break;
                case "tradedirection": op.TradeDirection = DataNormalizer.ParseEnum(v, op.TradeDirection); break;
                case "financestatus": op.FinanceStatus = DataNormalizer.ParseEnum(v, op.FinanceStatus); break;
                case "documentstatus": op.DocumentStatus = DataNormalizer.ParseEnum(v, op.DocumentStatus); break;
                case "loadingdate": op.LoadingDate = DataNormalizer.ParseDate(v); break;
                case "etd": op.Etd = DataNormalizer.ParseDateTime(v); break;
                case "eta": op.Eta = DataNormalizer.ParseDateTime(v); break;
                case "planneddeliverydate": op.PlannedDeliveryDate = DataNormalizer.ParseDate(v); break;
                case "deliverydate": op.DeliveryDate = DataNormalizer.ParseDate(v); break;
                case "revenue": case "revenueamount": op.RevenueAmount = DataNormalizer.ParseDecimal(v); break;
                case "cost": case "costamount": op.CostAmount = DataNormalizer.ParseDecimal(v); break;
                case "currency": op.Currency = v.ToUpperInvariant(); break;
                case "delayreason": op.DelayReason = DataNormalizer.ParseEnum(v, op.DelayReason); break;
                case "nextaction": case "nextactiondescription": op.NextActionDescription = v; break;

                case "blno": d.BlNo = v; break;
                case "containerno": d.ContainerNo = v; break;
                case "containertype": d.ContainerType = v; break;
                case "vesselname": d.VesselName = v; break;
                case "shippingline": d.ShippingLine = v; break;
                case "pol": d.Pol = v; break;
                case "pod": d.Pod = v; break;
                case "vehicleplate": d.VehiclePlate = v; break;
                case "drivername": d.DriverName = v; break;
                case "hawbno": d.HawbNo = v; break;
                case "mawbno": d.MawbNo = v; break;
                case "flightno": d.FlightNo = v; break;
                case "airline": d.Airline = v; break;

                default: break;
            }
        }
    }

    private static string Canon(string s)
        => new string((s ?? string.Empty).Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
}
