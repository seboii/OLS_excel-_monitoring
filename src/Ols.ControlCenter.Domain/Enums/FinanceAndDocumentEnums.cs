namespace Ols.ControlCenter.Domain.Enums;

/// <summary>Finansal/tahsilat statüsü.</summary>
public enum FinanceStatus
{
    Collected,
    Pending,
    PartiallyCollected,
    Overdue,
    AwaitingCustomerApproval,
    UnderBankReview,
    FinancialHold,
    Cancelled
}

/// <summary>Operasyon bazında evrak tamamlanma durumu.</summary>
public enum DocumentStatus
{
    Complete,
    Missing,
    Pending
}

/// <summary>Evrak/gümrük checklist kalemleri (taşıma tipine göre gruplanır).</summary>
public enum DocumentType
{
    // Ortak
    Invoice,
    PackingList,

    // Karayolu
    Cmr,
    ExportDeclaration,
    T1,
    PowerOfAttorney,
    Insurance,

    // Deniz
    BillOfLading,
    TelexRelease,
    Ordino,
    CertificateOfOrigin,

    // Hava
    Mawb,
    Hawb,
    Msds,
    Permit,

    Other
}
