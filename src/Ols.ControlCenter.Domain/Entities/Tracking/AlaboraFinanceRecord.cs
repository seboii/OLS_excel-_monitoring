namespace Ols.ControlCenter.Domain.Entities.Tracking;

/// <summary>
/// Alabora "СЧЕТА-ПЛАТЕЖИ" sayfası — fatura/tahsilat takibi (Rusça+Türkçe karışık başlıklar).
/// Başlık kaynak dosyada 4. satırdadır. Anahtar: YÜK NO.
/// </summary>
public class AlaboraFinanceRecord : TrackingRecordBase
{
    public string? No { get; set; }              // №
    public DateOnly? FtDate { get; set; }        // FT. TARİH
    public string? CompanyTitle { get; set; }    // FİRMA ÜNVANI
    public string? Voyage { get; set; }          // SEFER
    public string? Amount { get; set; }          // TUTAR (Сумма по счету)
    public string? Currency { get; set; }        // DÖVİZ CİNSİ (Валюта)
    public string? CargoStatus { get; set; }     // статус груза / выгрузка
    public string? DocsReadiness { get; set; }   // Готовность пакета документов
    public string? InvoiceMarked { get; set; }   // счет с отметкой
    public string? TransportDocs { get; set; }   // Транспортные документы
    public string? OrderContract { get; set; }   // поручение / заявка / договор
    public string? CommentOsh { get; set; }      // Комментарии ОШ
    public string? CommentOls { get; set; }      // Комментарии ОЛС
    public string? IncomingPayments { get; set; } // GELEN ÖDEMELER (платеж)
    public string? Collection { get; set; }      // TAHSİLAT (Статус оплаты)
    public DateOnly? PaymentDate { get; set; }   // payment date
    public string? Rub { get; set; }             // rub
    public string? LoadingType { get; set; }     // YÜKLEME CİNSİ (Транспорт)
    public string? LoadingDetails { get; set; }  // YÜKLEME DETAY BİLGİLERİ
    public string? CustomerRep { get; set; }     // MÜŞTERİ TEMSİLCİSİ
    public string? RateNet { get; set; }         // rate net
}
