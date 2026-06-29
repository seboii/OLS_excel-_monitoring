using System.Text;
using Ols.ControlCenter.Application.Abstractions.DataIntegration;

namespace Ols.ControlCenter.Application.Features.DataSources;

/// <summary>Kaynak kolon adlarından sistem alanı otomatik önerisi (kullanıcı değiştirebilir).</summary>
public sealed class ColumnMappingService : IColumnMappingService
{
    // kanonik (ASCII, küçük harf) kaynak kolon → hedef alan
    private static readonly Dictionary<string, string> Synonyms = new(StringComparer.OrdinalIgnoreCase)
    {
        ["operationno"] = "operationNo", ["dosyano"] = "operationNo", ["refno"] = "operationNo",
        ["reference"] = "operationNo", ["dosya"] = "operationNo", ["isno"] = "operationNo",
        ["client"] = "customerName", ["customer"] = "customerName", ["customername"] = "customerName",
        ["musteri"] = "customerName", ["firma"] = "customerName",
        ["status"] = "status", ["durum"] = "status", ["statu"] = "status",
        ["eta"] = "eta", ["etd"] = "etd",
        ["paymentstatus"] = "financeStatus", ["tahsilatdurumu"] = "financeStatus",
        ["tahsilat"] = "financeStatus", ["odeme"] = "financeStatus", ["finance"] = "financeStatus",
        ["loadingcountry"] = "originCountry", ["yuklemeulke"] = "originCountry", ["yuklemeulkesi"] = "originCountry",
        ["loadingcity"] = "originCity", ["yuklemesehir"] = "originCity", ["yuklemesehri"] = "originCity",
        ["deliverycountry"] = "destinationCountry", ["teslimulke"] = "destinationCountry", ["teslimulkesi"] = "destinationCountry",
        ["deliverycity"] = "destinationCity", ["teslimsehir"] = "destinationCity", ["teslimsehri"] = "destinationCity", ["varis"] = "destinationCity",
        ["service"] = "serviceType", ["hizmet"] = "serviceType", ["servicetype"] = "serviceType", ["yuktipi"] = "serviceType",
        ["plate"] = "vehiclePlate", ["plaka"] = "vehiclePlate",
        ["driver"] = "driverName", ["surucu"] = "driverName", ["sofor"] = "driverName",
        ["bl"] = "blNo", ["blno"] = "blNo", ["billoflading"] = "blNo",
        ["container"] = "containerNo", ["containerno"] = "containerNo", ["konteyner"] = "containerNo", ["konteynerno"] = "containerNo",
        ["vessel"] = "vesselName", ["gemi"] = "vesselName", ["vesselname"] = "vesselName",
        ["hawb"] = "hawbNo", ["mawb"] = "mawbNo", ["flight"] = "flightNo", ["ucus"] = "flightNo", ["ucusno"] = "flightNo",
        ["revenue"] = "revenue", ["gelir"] = "revenue", ["tutar"] = "revenue", ["amount"] = "revenue",
        ["cost"] = "cost", ["maliyet"] = "cost",
        ["currency"] = "currency", ["parabirimi"] = "currency", ["doviz"] = "currency",
        ["loadingdate"] = "loadingDate", ["yuklemetarihi"] = "loadingDate",
        ["planneddelivery"] = "plannedDeliveryDate", ["planlananteslim"] = "plannedDeliveryDate",
        ["deliverydate"] = "deliveryDate", ["teslimtarihi"] = "deliveryDate",
        ["shipper"] = "shipper", ["gonderici"] = "shipper", ["consignee"] = "consignee", ["alici"] = "consignee",
    };

    public IReadOnlyList<MappingSuggestion> Suggest(IReadOnlyList<SheetColumn> columns)
    {
        var result = new List<MappingSuggestion>();
        foreach (var c in columns)
        {
            var key = Canon(c.Name);
            string? target = null;
            if (key.Length > 0)
            {
                if (Synonyms.TryGetValue(key, out var exact)) target = exact;
                else
                {
                    foreach (var kv in Synonyms)
                        if (key.Contains(kv.Key) || kv.Key.Contains(key)) { target = kv.Value; break; }
                }
            }
            result.Add(new MappingSuggestion(c.Name, c.Index, target));
        }
        return result;
    }

    private static string Canon(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        var sb = new StringBuilder();
        foreach (var raw in s.ToLowerInvariant())
        {
            var ch = raw switch
            {
                'ş' => 's', 'ı' => 'i', 'ğ' => 'g', 'ü' => 'u', 'ö' => 'o', 'ç' => 'c',
                'â' => 'a', 'î' => 'i', 'û' => 'u', _ => raw,
            };
            if (char.IsLetterOrDigit(ch)) sb.Append(ch);
        }
        return sb.ToString();
    }
}
